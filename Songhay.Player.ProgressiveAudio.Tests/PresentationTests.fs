namespace Songhay.Player.ProgressiveAudio.Tests

open System
open System.Data
open System.IO
open System.Reflection
open Xunit
open System.Text.Json
open System.Text.RegularExpressions
open FsUnit.Xunit
open FsUnit.CustomMatchers
open FsToolkit.ErrorHandling

open Songhay.Modules.Models
open Songhay.Modules.JsonDocumentUtility
open Songhay.Modules.ProgramFileUtility

open Songhay.Player.ProgressiveAudio.Models

module PresentationTests =

    let projectDirectoryInfo =
        Assembly.GetExecutingAssembly()
        |> ProgramAssemblyInfo.getPathFromAssembly "../../../"
        |> Result.valueOr raiseProgramFileError
        |> DirectoryInfo

    let audioJsonDocumentPath =
        "./json/progressive-audio-default.json"
        |> tryGetCombinedPath projectDirectoryInfo.FullName
        |> Result.valueOr raiseProgramFileError

    let audioJsonDocument =
        JsonDocument.Parse(File.ReadAllText(audioJsonDocumentPath))

    [<Fact>]
    let ``validate root element test`` () =
        let actual = audioJsonDocument |> toFirstPropertyName
        let expected = nameof(Presentation) |> Some
        actual |> should equal expected

    [<Theory>]
    [<InlineData("2005-12-10-22-19-14-IDAMAQDBIDANAQDB-1")>]
    let ``Presentation.id test`` (expected: string) =
        let result =
            audioJsonDocument.RootElement
            |> tryGetProperty (nameof(Presentation))
            |> Result.bind (tryGetProperty "@ClientId")
        result |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let actual = (result |> Result.valueOr raise).GetString()
        actual |> should equal expected

    [<Theory>]
    [<InlineData("Songhay Audio Presentation")>]
    let ``Presentation.title test`` (expected: string) =
        let result =
            audioJsonDocument.RootElement
            |> tryGetProperty (nameof(Presentation))
            |> Result.bind (tryGetProperty "Title")
        result |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)

        let actual = (result |> Result.valueOr raise).GetString()
        actual |> should equal expected

    [<Theory>]
    [<InlineData("This InfoPath Form data is packaged with the audio presentation")>]
    let ``Presentation.parts PresentationDescription test`` (expected: string) =
        let result =
            audioJsonDocument.RootElement
            |> tryGetProperty (nameof(Presentation))
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
            audioJsonDocument.RootElement
            |> tryGetProperty (nameof(Presentation))
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
            DisplayText s

        let processMatches (creditsMatch: Match) =
            match creditsMatch.Groups |> List.ofSeq with
            | [_; r; n] -> Ok { role = r |> getRole; name = DisplayText <| n.Value }
            | _ -> Error <| DataException $"The expected {nameof(Regex)} group data is not here."

        let results = matches |> Seq.map processMatches
        results |> should not' (be Empty)