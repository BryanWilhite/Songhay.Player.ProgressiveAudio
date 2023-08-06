namespace Songhay.Player.ProgressiveAudio.Models

open System
open Microsoft.JSInterop

open Elmish

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models
open Songhay.Modules.Bolero

type ProgressiveAudioMessage =
    | GetPlayerManifest | GotPlayerManifest of Presentation option
    | InitializeDispatch of Dispatch<ProgressiveAudioMessage>
    | PlayPauseControlClick
    | PlayerAnimationTick
    | PlayerCreditsClick
    | PlaylistClick of (DisplayText * Uri)
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
