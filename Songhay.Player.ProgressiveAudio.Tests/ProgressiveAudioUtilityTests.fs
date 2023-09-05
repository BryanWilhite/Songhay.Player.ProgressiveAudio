namespace Songhay.Player.ProgressiveAudio.Tests

open System.IO
open System.Reflection
open Xunit.Abstractions

open FsToolkit.ErrorHandling

open Songhay.Modules.Models
open Songhay.Modules.ProgramFileUtility


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

