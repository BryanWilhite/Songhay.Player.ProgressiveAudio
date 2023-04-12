namespace Songhay.StudioFloor.Client

open System
open System.Net
open System.Net.Http
open System.Text.Json
open Microsoft.JSInterop
open Elmish

open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result
open Bolero.Remoting.Client

open Songhay.Modules.Bolero
open Songhay.Modules.HttpClientUtility
open Songhay.Modules.HttpRequestMessageUtility
open Songhay.Modules.Bolero.RemoteHandlerUtility
open Songhay.Player.ProgressiveAudio.Models
open Songhay.StudioFloor.Client.Models

module ClientUtility =

    module Remote =
        let tryDownloadToStringAsync (client: HttpClient, uri: Uri) =
            async {
                let! responseResult = client |> trySendAsync (get uri) |> Async.AwaitTask
                let! output =
                    (None, responseResult) ||> tryDownloadToStringAsync
                    |> Async.AwaitTask

                return output
            }

    let passFailureToConsole (jsRuntime: IJSRuntime option) ex =
        if jsRuntime.IsSome then
            jsRuntime.Value |> JsRuntimeUtility.consoleErrorAsync [|
                "failure:", ex
            |] |> ignore
        ex

    let updatePlayer (jsRuntime: IJSRuntime) (client: HttpClient) (message: ProgressiveAudioMessage) (model: StudioFloorModel) =
        let paModel = { model with playerModel = ProgressiveAudioModel.updateModel message model.playerModel }

        let failure ex = ((jsRuntime |> Some), ex) ||> message.failureMessage |> StudioFloorMessage.ProgressiveAudioMessage

        match message with
        | GetPlayerManifest ->
            let uri = Uri ""
            let success (result: Result<string, HttpStatusCode>) =
                let presentationOption =
                    result
                    |> Result.either
                        LegacyPresentationUtility.tryGetPresentation
                        (fun statusCode ->
                            let ex = JsonException($"{nameof HttpStatusCode}: {statusCode}")
                            Result.Error ex)
                    |> Result.toOption
                StudioFloorMessage.ProgressiveAudioMessage <| ProgressiveAudioMessage.GotPlayerManifest presentationOption

            model, Cmd.OfAsync.either Remote.tryDownloadToStringAsync (client, uri) success failure
        | _ ->
            model, Cmd.none
