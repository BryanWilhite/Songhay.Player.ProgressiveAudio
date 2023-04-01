namespace Songhay.Player.ProgressiveAudio.Tests

open System.IO
open System.Reflection
open System.Text.Json
open System.Text.Json.Serialization

open Songhay.Modules.Publications.Models
open Xunit
open Xunit.Abstractions

open FsUnit.Xunit
open FsToolkit.ErrorHandling

open Songhay.Modules
open Songhay.Modules.Models
open Songhay.Modules.ProgramFileUtility

open Songhay.Player.ProgressiveAudio.Models

type PresentationTests (testOutputHelper: ITestOutputHelper) =

    let projectDirectoryInfo =
        Assembly.GetExecutingAssembly()
        |> ProgramAssemblyInfo.getPathFromAssembly "../../../"
        |> Result.valueOr raiseProgramFileError
        |> DirectoryInfo

    let myPresentationPath =
        "./json/my-presentation.json"
        |> tryGetCombinedPath projectDirectoryInfo.FullName
        |> Result.valueOr raiseProgramFileError

    let presentation =
        {
            id = "my-id" |> Alphanumeric |> Id
            title = "this is the title, man" |> Title
            cssVariables = [
                CssVariableAndValue (CssVariable "--my-css-var-one", CssValue "var-one-value")
            ]
            parts = [
                CopyRights [ { year = 2023; name = "Bryan Wilhite" } ]
            ]
        }

    [<Fact>]
    let ``serialize and deserialize `Presentation` test``() =
        let options = JsonSerializerOptions()
        options.WriteIndented <- true
        options.Converters.Add(JsonFSharpConverter())

        let json = JsonSerializer.Serialize(presentation, options)

        $"saving JSON to `{myPresentationPath}`..." |> testOutputHelper.WriteLine

        File.WriteAllText(myPresentationPath, json)

        let actual = JsonSerializer.Deserialize<Presentation>(json, options)

        actual |> should equal presentation
