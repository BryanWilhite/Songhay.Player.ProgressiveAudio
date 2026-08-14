[<AutoOpen>]
module Songhay.Player.ProgressiveAudio.Tests.TestUtility

open System.IO
open System.Linq
open System.Net.Http
open System.Reflection
open System.Text.Json
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging.Abstractions
open FsToolkit.ErrorHandling

open Songhay.Modules.Models
open Songhay.Modules.ProgramFileUtility

let nullLogger = NullLogger.Instance :> ILogger

let httpClient = new HttpClient()

let projectDirectoryInfo =
    Assembly.GetExecutingAssembly()
    |> ProgramAssemblyInfo.getPathFromAssembly "../../../"
    |> Result.valueOr raiseProgramFileError
    |> DirectoryInfo

let appSettingsPath = projectDirectoryInfo
                          .Parent.GetDirectories()
                          .First(_.Name.EndsWith(".Client"))
                          .GetDirectories()
                          .First(_.Name.Equals("wwwroot"))
                          .GetFiles()
                          .First(_.Name.Equals("appsettings.json"))
                          .FullName

let studioFloorConfiguration = ConfigurationBuilder().AddJsonFile(appSettingsPath).Build()

let getJson (rootDirectoryInfo: DirectoryInfo) (fileName: string) =
    let path =
        $"./json/{fileName}"
        |> tryGetCombinedPath rootDirectoryInfo.FullName
        |> Result.valueOr raiseProgramFileError
    File.ReadAllText(path)

let getProjectJson (fileName: string) =
    fileName |> getJson projectDirectoryInfo

let getJsonDocument (rootDirectoryInfo: DirectoryInfo) (fileName: string) =
    JsonDocument.Parse(fileName |> getJson rootDirectoryInfo)

let getProjectJsonDocument (fileName: string) =
    JsonDocument.Parse(fileName |> getJson projectDirectoryInfo)

let writeJsonAsync (rootDirectoryInfo: DirectoryInfo) (fileName: string) (json:string) =
    let path =
        $"./json/{fileName}"
        |> tryGetCombinedPath rootDirectoryInfo.FullName
        |> Result.valueOr raiseProgramFileError
    File.WriteAllTextAsync(path, json)
