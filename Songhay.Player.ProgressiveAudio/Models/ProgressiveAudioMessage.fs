namespace Songhay.Player.ProgressiveAudio.Models

open System
open Bolero
open Microsoft.JSInterop

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models
open Songhay.Modules.Bolero

/// <summary>
/// The Elmish messages of this domain.
/// </summary>
type ProgressiveAudioMessage =
    | GetPlayerManifest | GotPlayerManifest of (Identifier * Presentation option)
    | GotPlayerControlsRefs of {| audioElementRef: HtmlRef; buttonElementRef: HtmlRef; playerControlsComp: Component |}
    | GotPlayerSection of HtmlRef
    | PlayPauseButtonClickEvent | PlayPauseInputEvent | PlayPauseChangeEvent of HtmlRef
    | PlayAudioMetadataLoadedEvent | PlayAudioEndedEvent
    | PlayerAnimationTick of PlayerAnimationTickData
    | PlayerCreditsClick
    | PlaylistClick of (DisplayText * Uri)
    | PlayerError of exn

    /// <summary>Centralizes failure message reporting to the browser</summary>
    member this.failureMessage (jsRuntime: IJSRuntime option) ex =
        let failureMsg = PlayerError ex

        if jsRuntime.IsSome then
            jsRuntime.Value |> JsRuntimeUtility.consoleErrorAsync [|
                $"{this} failure:", ex
            |] |> ignore

        failureMsg

    /// <summary>The <see cref="string"/> representation of this instance.</summary>
    override this.ToString() =
        $"{nameof ProgressiveAudioMessage}.{this.ToString()}"
