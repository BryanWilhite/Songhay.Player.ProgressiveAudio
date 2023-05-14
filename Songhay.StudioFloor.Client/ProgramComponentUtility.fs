namespace Songhay.StudioFloor.Client

open System
open System.Net
open System.Net.Http
open System.Text.Json
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open Elmish

open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result
open Bolero.Remoting.Client

open Songhay.Modules.Bolero
open Songhay.Modules.HttpClientUtility
open Songhay.Modules.HttpRequestMessageUtility
open Songhay.Modules.Bolero.RemoteHandlerUtility

open Songhay.Modules.Publications.Models
open Songhay.Player.ProgressiveAudio.ProgressiveAudioScalars
open Songhay.Player.ProgressiveAudio.Models

open Songhay.StudioFloor.Client.Models

module ProgramComponentUtility =

    module Remote =
        let getPresentationManifestUri (presentationKey: string ) =
            ($"{rxProgressiveAudioApiRootUri}/api/Player/v1/audio/{presentationKey}", UriKind.Absolute) |> Uri

        let getPresentationPlaylistItemUri (presentationKey: string ) (relativePath: string) =
            let segment = relativePath.TrimStart('.', '/')
            ($"{rxProgressiveAudioApiRootUri}/api/Player/v1/audio/{presentationKey}/{segment}", UriKind.Absolute) |> Uri

        let tryDownloadToStringAsync (client: HttpClient, uri: Uri) =
            async {
                let! responseResult = client |> trySendAsync (get uri) |> Async.AwaitTask
                let! output =
                    (None, responseResult) ||> tryDownloadToStringAsync
                    |> Async.AwaitTask

                return output
            }

    let getPresentationKey (jsRuntime: IJSRuntime) (navMan: NavigationManager) =

        let uriFragmentOption =
            match (navMan.Uri, UriKind.Absolute) |> Uri |> fun uri -> uri.Fragment with
            | s when s.Length > 0 -> Some s
            | _ -> None

        jsRuntime |> JsRuntimeUtility.consoleWarnAsync [| nameof uriFragmentOption ; uriFragmentOption |] |> ignore

        let getTypeAndKey (s: string) =
            match s.Split('/') with
            | [| _ ; t ; k |] ->
                if t = "audio" then Some k
                else None
            | _ -> None

        uriFragmentOption
        |> Option.bind (fun s -> s |> getTypeAndKey)

    let passFailureToConsole (jsRuntime: IJSRuntime option) ex =
        if jsRuntime.IsSome then
            jsRuntime.Value |> JsRuntimeUtility.consoleErrorAsync [|
                "failure:", ex
            |] |> ignore
        ex

    let getCommandForGetReadMe model =
        let success (result: Result<string, HttpStatusCode>) =
            let data = result |> Result.valueOr (fun code -> $"The expected README data is not here. [error code: {code}]")
            GotReadMe data
        let failure ex = ((model.blazorServices.jsRuntime |> Some), ex) ||> passFailureToConsole |> Error
        let uri = ("./README.html", UriKind.Relative) |> Uri

        Cmd.OfAsync.either Remote.tryDownloadToStringAsync (model.blazorServices.httpClient, uri) success failure

    let getCommandForSetTab tab =
        match tab with
        | PlayerTab ->
            let msg = StudioFloorMessage.ProgressiveAudioMessage ProgressiveAudioMessage.GetPlayerManifest
            Cmd.ofMsg msg
        | _ -> Cmd.none

    let getCommandForProgressiveAudio model (message: ProgressiveAudioMessage) =

        let failure ex =
            (Some model.blazorServices.jsRuntime, ex) ||> message.failureMessage
            |> StudioFloorMessage.ProgressiveAudioMessage
        let httpFailure statusCode =
            let ex = JsonException($"{nameof HttpStatusCode}: {statusCode}")
            Result.Error ex

        match message with
        | GetPlayerManifest ->
            let keyOption =
                (
                    model.blazorServices.jsRuntime,
                    model.blazorServices.navigationManager
                ) ||> getPresentationKey
            if keyOption.IsNone then
                let msg = StudioFloorMessage.Error <| FormatException $"The expected {nameof Presentation} key was not found."
                Cmd.ofMsg msg
            else
                let uri = keyOption.Value |> Remote.getPresentationManifestUri
                let success (result: Result<string, HttpStatusCode>) =
                    let presentationOption =
                        result
                        |> Result.either LegacyPresentationUtility.tryGetPresentation httpFailure
                        |> Result.toOption
                    StudioFloorMessage.ProgressiveAudioMessage <| ProgressiveAudioMessage.GotPlayerManifest presentationOption

                Cmd.OfAsync.either Remote.tryDownloadToStringAsync (model.blazorServices.httpClient, uri) success failure
        | _ -> Cmd.none
