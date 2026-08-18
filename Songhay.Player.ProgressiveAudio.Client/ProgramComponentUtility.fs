namespace Songhay.Player.ProgressiveAudio.Client

open System
open System.Net
open System.Net.Http
open System.Text.Json
open Microsoft.FSharp.Core
open Elmish

open FsToolkit.ErrorHandling

open Songhay.Modules.HttpClientUtility
open Songhay.Modules.HttpRequestMessageUtility
open Songhay.Modules.Models
open Songhay.Modules.Publications.Models
open Songhay.Modules.Bolero.JsRuntimeUtility
open Songhay.Modules.Bolero.RemoteHandlerUtility

open Songhay.Player.ProgressiveAudio.Client.Models
open Songhay.Player.ProgressiveAudio.Models

module ProgramComponentUtility =
    let httpClient = Songhay.Modules.Bolero.ServiceProviderUtility.getHttpClient()
    let jsRuntime = Songhay.Modules.Bolero.ServiceProviderUtility.getIJSRuntime()

    module Remote =
        let tryDownloadToStringAsync (client: HttpClient, uri: Uri) =
            async {
                let! responseResult = client |> trySendAsync (get uri) |> Async.AwaitTask
                let! output =
                    (None, responseResult) ||> tryDownloadToStringAsync
                    |> Async.AwaitTask

                return output
            }

    let getCommandForProgressiveAudio (message: ProgressiveAudioMessage) (model: ProgressiveAudioModel)=

        let failure ex =
            (Some jsRuntime, ex) ||> message.failureMessage
            |> ProgressiveAudioMessage

        match message with
        | GetPlayerManifest key ->
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
                            let id = Identifier.fromString key
                            let paMessage = ProgressiveAudioMessage.GotPlayerManifest <| (id ,Some presentation)
                            ProgressiveAudioMessage paMessage
                    )
                    (
                        fun ex ->
                            let label = $"{nameof Presentation}.{nameof Presentation.fromInput}:" |> Some
                            jsRuntime |> passErrorToConsole label ex |> Error
                    )

            let uriResult = model.ToUriResultFromClaim("route-for-audio-manifest", key)
            uriResult
            |> Result.either
                (fun uri -> Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, uri) success failure)
                (
                    fun e ->
                        failure <| exn($"{nameof e.Message}: {e.Message} [{nameof key} `{key}`].") |> ignore
                        Cmd.none
                )
        | _ -> Cmd.none

    let getCommandForSetPage page =
        match page with
        | BRollAudioPage key ->
            let msg = ProgressiveAudioMessage <| ProgressiveAudioMessage.GetPlayerManifest key
            Cmd.ofMsg msg
        | _ -> Cmd.none
