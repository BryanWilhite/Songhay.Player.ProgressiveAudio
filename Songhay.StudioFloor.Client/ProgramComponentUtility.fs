namespace Songhay.StudioFloor.Client

open System
open System.Net
open System.Net.Http
open System.Text.Json
open Microsoft.FSharp.Core
open Elmish

open FsToolkit.ErrorHandling
open Bolero.Remoting.Client

open Songhay.Modules.HttpClientUtility
open Songhay.Modules.HttpRequestMessageUtility
open Songhay.Modules.Models
open Songhay.Modules.Publications.Models
open Songhay.Modules.Bolero.JsRuntimeUtility
open Songhay.Modules.Bolero.RemoteHandlerUtility

open Songhay.Player.ProgressiveAudio.ProgressiveAudioUtility
open Songhay.Player.ProgressiveAudio.Models

open Songhay.StudioFloor.Client.Models

module ProgramComponentUtility =

    module Remote =
        let tryDownloadToStringAsync (client: HttpClient, uri: Uri) =
            async {
                let! responseResult = client |> trySendAsync (get uri) |> Async.AwaitTask
                let! output =
                    (None, responseResult) ||> tryDownloadToStringAsync
                    |> Async.AwaitTask

                return output
            }

    let getCommandForGetReadMe model =
        let success (result: Result<string, HttpStatusCode>) =
            let data = result |> Result.valueOr (fun code -> $"The expected README data is not here. [error code: {code}]")
            GotReadMe data
        let label = $"{nameof Remote.tryDownloadToStringAsync}:" |> Some
        let failure ex = model.blazorServices.jsRuntime |> passErrorToConsole label ex |> Error
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

        match message with
        | GetPlayerManifest ->
            let keyOption = (model.blazorServices.jsRuntime, model.blazorServices.navigationManager) ||> getPresentationKey
            if keyOption.IsNone then
                let ex = FormatException $"The expected {nameof Presentation} key was not found."
                let msg = model.blazorServices.jsRuntime |> passErrorToConsole None ex |> StudioFloorMessage.Error
                Cmd.ofMsg msg
            else
                let uri = keyOption.Value |> getPresentationManifestUri
                let success (result: Result<string, HttpStatusCode>) =
                    result
                    |> Result.either
                        Presentation.fromInput
                        (
                            fun statusCode ->
                                let ex = JsonException($"{nameof HttpStatusCode}: {statusCode}")
                                Result.Error ex
                        )
                    |> Result.either
                        (
                            fun presentation ->
                                let id = Identifier.fromString keyOption.Value
                                let paMessage = ProgressiveAudioMessage.GotPlayerManifest <| (id ,Some presentation)
                                StudioFloorMessage.ProgressiveAudioMessage paMessage
                        )
                        (
                            fun ex ->
                                let label = $"{nameof Presentation}.{nameof Presentation.fromInput}:" |> Some
                                model.blazorServices.jsRuntime |> passErrorToConsole label ex |> StudioFloorMessage.Error
                        )

                Cmd.OfAsync.either Remote.tryDownloadToStringAsync (model.blazorServices.httpClient, uri) success failure
        | _ -> Cmd.none
