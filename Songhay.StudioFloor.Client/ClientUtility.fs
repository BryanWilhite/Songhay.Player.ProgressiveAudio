namespace Songhay.StudioFloor.Client

open System
open System.Net.Http
open Elmish
open Bolero.Remoting.Client
open Microsoft.JSInterop

open Songhay.Modules.Bolero
open Songhay.Modules.HttpClientUtility
open Songhay.Modules.HttpRequestMessageUtility
open Songhay.Modules.Bolero.RemoteHandlerUtility

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

    let update (_: IJSRuntime) (_: HttpClient) paMsg model =

        match paMsg with
        | _ -> model, Cmd.none
