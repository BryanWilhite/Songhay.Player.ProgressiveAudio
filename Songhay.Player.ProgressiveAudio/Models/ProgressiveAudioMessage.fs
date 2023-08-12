namespace Songhay.Player.ProgressiveAudio.Models

open System
open Bolero
open Microsoft.JSInterop

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models
open Songhay.Modules.Bolero

type ProgressiveAudioMessage =
    | GetPlayerManifest | GotPlayerManifest of Presentation option
    | GotPlayerControlsRef of Component
    | PlayPauseButtonClickEvent | PlayPauseInputEvent | PlayPauseChangeEvent of HtmlRef
    | PlayAudioMetadataLoadedEvent | PlayAudioEndedEvent
    | PlayerAnimationTick of PlayerAnimationTickData
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
