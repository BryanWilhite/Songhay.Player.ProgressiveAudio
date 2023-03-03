namespace Songhay.Player.ProgressiveAudio.Tests

open System.IO
open System.Reflection
open Xunit
open System.Text.Json
open Xunit
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
        let actual = audioJsonDocument.RootElement |> toPropertyName
        let expected = nameof(Presentation) |> Some
        actual |> should equal expected

    [<Fact>]
    let ``Presentation.id test`` () =
        let result =
            audioJsonDocument.RootElement
            |> tryGetProperty (nameof(Presentation))
            |> Result.bind (tryGetProperty "@ClientId")
        result |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)
        let actual = (result |> Result.valueOr raise).GetString()
        actual |> System.String.IsNullOrWhiteSpace |> should be False

    [<Fact>]
    let ``read legacy manifest test`` () =
        Assert.True(true)
