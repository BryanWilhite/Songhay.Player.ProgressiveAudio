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
    let ``read legacy manifest test`` () =
        Assert.True(true)
