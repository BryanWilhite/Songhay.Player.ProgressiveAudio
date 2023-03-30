namespace Songhay.Player.ProgressiveAudio.Tests

open System
open System.Data
open System.IO
open System.Linq
open System.Reflection
open System.Text.Json
open System.Text.RegularExpressions

open FsToolkit.ErrorHandling
open FsUnit.CustomMatchers
open FsUnit.Xunit
open Xunit
open Xunit.Abstractions

open Songhay.Modules.Models
open Songhay.Modules.JsonDocumentUtility
open Songhay.Modules.Publications.Models
open Songhay.Modules.ProgramFileUtility

open Songhay.Player.ProgressiveAudio.Models
open Songhay.Player.ProgressiveAudio.LegacyPresentationUtility

type LegacyPresentationUtilityTests(outputHelper: ITestOutputHelper) =

    let projectDirectoryInfo =
        Assembly.GetExecutingAssembly()
        |> ProgramAssemblyInfo.getPathFromAssembly "../../../"
        |> Result.valueOr raiseProgramFileError
        |> DirectoryInfo

    let audioJsonDocumentPath =
        "./json/progressive-audio-default.json"
        |> tryGetCombinedPath projectDirectoryInfo.FullName
        |> Result.valueOr raiseProgramFileError

    let presentationElementResult =
        File.ReadAllText(audioJsonDocumentPath)
        |> tryGetPresentationElementResult

    [<Theory>]
    [<InlineData("2005-12-10-22-19-14-IDAMAQDBIDANAQDB-1")>]
    let ``Presentation.id test`` (expected: string) =
        let result = presentationElementResult |> tryGetPresentationIdResult
        result |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let actual = result |> toResultFromStringElement (fun el -> el.GetString() |> Identifier.fromString |> Id)
        actual |> should be (ofCase <@ Result<Id, JsonException>.Ok @>)

        (actual |> Result.valueOr raise).Value.StringValue |> should equal expected

    [<Theory>]
    [<InlineData("Songhay Audio Presentation")>]
    let ``Presentation.title test`` (expected: string) =
        let result = presentationElementResult |> tryGetPresentationTitleResult
        result |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let actual = result |> toResultFromStringElement (fun el -> el.GetString() |> Title)
        actual |> should be (ofCase <@ Result<Title, JsonException>.Ok @>)

        match (actual |> Result.valueOr raise) with | Title t -> t |> should equal expected

    [<Theory>]
    [<InlineData("This InfoPath Form data is packaged with the audio presentation")>]
    let ``Presentation.parts PresentationDescription test`` (expected: string) =
        let result = presentationElementResult |> tryGetPresentationDescriptionResult
        result |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let actual =
            result
            |> toResultFromStringElement (fun el -> el.GetString() |> DisplayText |> Description |> PresentationDescription)
        actual |> should be (ofCase <@ Result<PresentationPart, JsonException>.Ok @>)

        (actual |> Result.valueOr raise).StringValue.Contains(expected) |> should be True

    [<Fact>]
    let ``Presentation.parts Credits test`` () =
        let result = presentationElementResult |> tryGetPresentationCreditsResult
        result |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let htmlResult = result |> toResultFromStringElement (fun el -> el.GetString())
        htmlResult |> should be (ofCase <@ Result<string, JsonException>.Ok @>)
        (htmlResult |> Result.valueOr raise) |> String.IsNullOrWhiteSpace |> should be False

        let rx = Regex("<div>([^<>]+)<strong>([^<>]+)<\/strong><\/div>", RegexOptions.Compiled)
        let matches = (htmlResult |> Result.valueOr raise) |> rx.Matches
        matches |> should not' (be Empty)

        let getRole (group: Group) = Regex.Replace(group.Value, " by[ , ]. . . . . . . ", String.Empty)

        let processMatches (creditsMatch: Match) =
            match creditsMatch.Groups |> List.ofSeq with
            | [_; r; n] -> Ok { role = r |> getRole; name = n.Value }
            | _ -> Error <| DataException $"The expected {nameof(Regex)} group data is not here."

        let actual = matches |> Seq.map processMatches |> List.ofSeq
        actual |> should not' (be Empty)

    [<Theory>]
    [<InlineData("--rx-player-playlist-background-color", "0xEAEAEA")>]
    let ``Presentation.cssVariables test``(expectedVarName: string) (expectedValue: string) =
        let result = presentationElementResult |> tryGetLayoutMetadataResult
        result |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let actual = result |> toPresentationCssVariables
        actual |> should be (ofCase <@ Result<CssVariableAndValues, JsonException>.Ok @>)
        (actual |> Result.valueOr raise)
        |> List.find
            (
                fun i ->
                    let cssVar, cssVal = i.Pair
                    cssVar.Value = expectedVarName
                    && cssVal.Value = expectedValue
            )
        |> (fun i -> i.toCssDeclaration |> outputHelper.WriteLine)

    [<Theory>]
    [<InlineData("©2006 Songhay System")>]
    let ``Presentation.parts Copyrights test`` (expected: string) =
        let nameElementResult = presentationElementResult |> tryGetCopyrightNameResult
        nameElementResult |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let yearElementResult = presentationElementResult |> tryGetCopyrightYearResult
        yearElementResult |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let actual = (nameElementResult, yearElementResult) ||> toPresentationCopyrights

        actual |> should be (ofCase <@ Result<PresentationPart, JsonException>.Ok @>)

        (actual |> Result.valueOr raise).StringValues[0] |> should equal expected

    [<Fact>]
    let ``Presentation.parts Playlist test`` () =
        let result = presentationElementResult |> tryGetPlaylistRootResult
        result |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let toPlaylistItem el =
            let titleResult =
                el
                |> tryGetProperty "#text"
                |> toResultFromStringElement ( fun el -> el.GetString() |> DisplayText)
            titleResult |> should be (ofCase <@ Result<DisplayText, JsonException>.Ok @>)

            let uriResult =
                el
                |> tryGetProperty "@Uri" |>toResultFromStringElement (fun el -> Uri (el.GetString(), UriKind.Relative))
            uriResult |> should be (ofCase <@ Result<Uri, JsonException>.Ok @>)

            ( titleResult |> Result.valueOr raise, uriResult |> Result.valueOr raise )

        let actual =
            result
            |> toResultFromJsonElement (fun kind -> kind = JsonValueKind.Array) (fun el -> el.EnumerateArray().ToArray())
            |> Result.map (fun a -> a |> Array.map toPlaylistItem |> List.ofArray |> Playlist)

        actual |> should be (ofCase <@ Result<PresentationPart, JsonException>.Ok @>)
