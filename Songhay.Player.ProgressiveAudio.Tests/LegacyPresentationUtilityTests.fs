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
open Songhay.Modules.ProgramFileUtility
open Songhay.Modules.Publications.Models

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
        let result =
            presentationElementResult
            |> Result.bind (tryGetProperty "@ClientId")
        result |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let actual = (result |> Result.valueOr raise).GetString()
        actual |> should equal expected

    [<Theory>]
    [<InlineData("Songhay Audio Presentation")>]
    let ``Presentation.title test`` (expected: string) =
        let result =
            presentationElementResult
            |> Result.bind (tryGetProperty <| nameof(Title))
        result |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let actual = (result |> Result.valueOr raise).GetString()
        actual |> should equal expected

    [<Theory>]
    [<InlineData("This InfoPath Form data is packaged with the audio presentation")>]
    let ``Presentation.parts PresentationDescription test`` (expected: string) =
        let result =
            presentationElementResult
            |> Result.bind (tryGetProperty <| nameof(Description))
            |> Result.bind (tryGetProperty "#text")
        result |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let actual =
            (result |> Result.valueOr raise).GetString()
            |> DisplayText
            |> Description
            |> PresentationDescription

        actual.StringValue.Contains(expected) |> should be True

    [<Fact>]
    let ``Presentation.parts Credits test`` () =
        let result =
            presentationElementResult
            |> Result.bind (tryGetProperty <| nameof(Credits))
            |> Result.bind (tryGetProperty "#text")
        result |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let actual = (result |> Result.valueOr raise).GetString()

        actual |> String.IsNullOrWhiteSpace |> should be False

        let rx = Regex("<div>([^<>]+)<strong>([^<>]+)<\/strong><\/div>", RegexOptions.Compiled)
        let matches = actual |> rx.Matches
        matches |> should not' (be Empty)

        let getRole (group: Group) =
            let s = Regex.Replace(group.Value, " by[ , ]. . . . . . . ", String.Empty)
            s

        let processMatches (creditsMatch: Match) =
            match creditsMatch.Groups |> List.ofSeq with
            | [_; r; n] -> Ok { role = r |> getRole; name = n.Value }
            | _ -> Error <| DataException $"The expected {nameof(Regex)} group data is not here."

        let results = matches |> Seq.map processMatches
        results |> should not' (be Empty)

    [<Fact>]
    let ``Presentation.cssVariables test``() =
        let result =
            presentationElementResult
            |> Result.bind (tryGetProperty "LayoutMetadata")
        result |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let actual = Array.Empty<CssVariable>().ToList()

        let rec processProperty (prefix: string) (p: JsonProperty) =
            match p.Value.ValueKind with
            | JsonValueKind.Object ->
                outputHelper.WriteLine <| String.Empty
                p.Value.EnumerateObject().ToArray()
                |> Array.iter (fun el -> ($"{prefix}{p.Name}-", el) ||> processProperty)
                ()
            | _ ->
                match p.Name with
                | "@uri" | "@version" -> ()
                | _ ->
                    let cssVar = $"{prefix}{p.Name.TrimStart('@')}" |> CssVariable.fromInput
                    outputHelper.WriteLine $"{cssVar}"
                    actual.Add(cssVar)
                    ()

        result
        |> Result.iter
            (fun el ->
                el.EnumerateObject().ToArray()
                |> Array.iter (fun el -> ("rx-player-", el) ||> processProperty)
            )

        actual |> should not' (be Empty)

    [<Theory>]
    [<InlineData("©2006 Songhay System")>]
    let ``Presentation.parts Copyrights test`` (expected: string) =
        let nameResult =
            presentationElementResult
            |> Result.bind (tryGetProperty <| nameof(Copyright))
            |> Result.bind (tryGetProperty "@Name")
        nameResult |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let yearResult =
            presentationElementResult
            |> Result.bind (tryGetProperty <| nameof(Copyright))
            |> Result.bind (tryGetProperty "@Year")
        nameResult |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let actual =
            [
                {
                    year = (yearResult |> Result.valueOr raise).GetString() |> Int32.Parse
                    name = (nameResult |> Result.valueOr raise).GetString()
                }
            ]
            |> CopyRights

        actual.StringValues[0].ToString() |> should equal expected

    [<Fact>]
    let ``Presentation.parts Playlist test`` () =
        let result =
            presentationElementResult
            |> Result.bind (tryGetProperty "ItemGroup")
            |> Result.bind (tryGetProperty "Item")
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
