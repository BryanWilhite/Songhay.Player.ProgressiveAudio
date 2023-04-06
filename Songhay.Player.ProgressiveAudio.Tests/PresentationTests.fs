namespace Songhay.Player.ProgressiveAudio.Tests

open System
open System.IO
open System.Reflection
open System.Text.Json
open System.Text.Json.Serialization

open Xunit
open Xunit.Abstractions

open FsUnit.Xunit
open FsToolkit.ErrorHandling

open Songhay.Modules
open Songhay.Modules.Models
open Songhay.Modules.ProgramFileUtility
open Songhay.Modules.Publications.Models
open Songhay.Modules.Bolero.Models

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
                CssVariableAndValue (CssVariable "--my-css-var-two", CssValue "var-two-value")
                CssVariableAndValue (CssVariable "--my-css-var-three", CssValue "var-three-value")
            ]
            parts = [
                CopyRights [
                    { year = 2023; name = "Bryan Wilhite" }
                    { year = 2023; name = "Creflo Dollar" }
                ]
                Credits [
                    { role = "Cinematographer"; name = "Kodak Black" }
                    { role = "Catering"; name = "Arlendo Calrissian" }
                    { role = "Location Scout"; name = "Ranger Rick" }
                ]
                PresentationDescription "this is a wonderful work of art"
                Pages [
                    "page, the one"
                    "second page"
                    "the end"
                ]
                Playlist [
                    DisplayText "track one", Uri("track-one.mp3", UriKind.Relative)
                    DisplayText "track two", Uri("track-two.mp3", UriKind.Relative)
                    DisplayText "track three", Uri("track-three.mp3", UriKind.Relative)
                ]
                Stream [
                    {
                        id = "mpeg-dash-001" |> Alphanumeric |> Id
                        thumbnailUri = Uri("mpeg-dash-001.jpg", UriKind.Relative)
                    }
                    {
                        id = "mpeg-dash-002" |> Alphanumeric |> Id
                        thumbnailUri = Uri("mpeg-dash-002.jpg", UriKind.Relative)
                    }
                    {
                        id = "mpeg-dash-003" |> Alphanumeric |> Id
                        thumbnailUri = Uri("mpeg-dash-003.jpg", UriKind.Relative)
                    }
                ]
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
