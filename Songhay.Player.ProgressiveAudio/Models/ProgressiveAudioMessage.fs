namespace Songhay.Player.ProgressiveAudio.Models

open Microsoft.JSInterop
open Songhay.Modules.Bolero
open Songhay.Modules.Publications.Models

type ProgressiveAudioMessage =
    | GetPlayerManifest | GotPlayerManifest of Presentation option
    | PlayPauseControl
    | PlayerCreditsClick
    | PlayerError of exn

    member this.failureMessage (jsRuntime: IJSRuntime option) ex =
        let failureMsg = PlayerError ex

        if jsRuntime.IsSome then
            jsRuntime.Value |> JsRuntimeUtility.consoleErrorAsync [|
                $"{this} failure:", ex
            |] |> ignore

        failureMsg

    override this.ToString() =
        $"{nameof ProgressiveAudioMessage}.{this.ToString()}"
