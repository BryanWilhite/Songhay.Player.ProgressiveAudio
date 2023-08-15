namespace Songhay.Player.ProgressiveAudio.Tests

open System.IO
open System.Reflection
open System.Text.Json
open Xunit
open Xunit.Abstractions

open FsToolkit.ErrorHandling
open FsUnit.CustomMatchers
open FsUnit.Xunit

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models
open Songhay.Modules.ProgramFileUtility
open Songhay.Modules.Bolero.LegacyPresentationUtility

open Songhay.Player.ProgressiveAudio.ProgressiveAudioUtility

type ProgressiveAudioUtilityTests(outputHelper: ITestOutputHelper) =
    let projectDirectoryInfo =
        Assembly.GetExecutingAssembly()
        |> ProgramAssemblyInfo.getPathFromAssembly "../../../"
        |> Result.valueOr raiseProgramFileError
        |> DirectoryInfo

    let audioJsonDocumentPath =
        "./json/progressive-audio-default.json"
        |> tryGetCombinedPath projectDirectoryInfo.FullName
        |> Result.valueOr raiseProgramFileError

    [<Fact>]
    let ``mapCssPair test`` () =
        let json = File.ReadAllText(audioJsonDocumentPath)
        let presentationResult = json |> tryGetPresentation
        presentationResult |> should be (ofCase <@ Result<Presentation, JsonException>.Ok @>)

        let presentation = presentationResult |> Result.valueOr raise
        presentation.cssVariables
        |> List.iter (fun vv -> outputHelper.WriteLine((vv |> mapCssPair).ToString()))
