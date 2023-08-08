namespace Songhay.Player.ProgressiveAudio

open Microsoft.FSharp.Core

open Songhay.Modules.Bolero.JsRuntimeUtility

module ProgressiveAudioScalars =

    [<Literal>]
    let rxProgressiveAudioApiRoot = "http://songhay-system-player.azurewebsites.net"

    [<Literal>]
    let rxProgressiveAudioRoot = "https://songhay.blob.core.windows.net/player-audio/"

    let rxProgressiveAudioInteropHandleMetadataLoaded = $"{rx}.ProgressiveAudioUtility.handleAudioMetadataLoadedAsync"

    let rxProgressiveAudioInteropLoadTrack = $"{rx}.ProgressiveAudioUtility.loadAudioTrack"

    let rxProgressiveAudioInteropStartAnimation = $"{rx}.ProgressiveAudioUtility.startPlayAnimationAsync"

    let rxProgressiveAudioInteropStopAnimation = $"{rx}.ProgressiveAudioUtility.stopPlayAnimationAsync"
