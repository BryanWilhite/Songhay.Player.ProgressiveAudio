namespace Songhay.Player.ProgressiveAudio

open Microsoft.FSharp.Core

open Songhay.Modules.Bolero.JsRuntimeUtility

module ProgressiveAudioScalars =

    [<Literal>]
    let rxProgressiveAudioApiRoot = "http://songhay-system-player.azurewebsites.net"

    [<Literal>]
    let rxProgressiveAudioRoot = "https://songhaystorage.blob.core.windows.net/player-audio/"

    let rxProgressiveAudioInteropHandleMetadataLoaded = $"{rx}.ProgressiveAudioUtility.handleAudioMetadataLoadedAsync"

    let rxProgressiveAudioInteropLoadTrack = $"{rx}.ProgressiveAudioUtility.loadAudioTrack"

    let rxProgressiveAudioInteropSetAudioCurrentTime = $"{rx}.ProgressiveAudioUtility.setAudioCurrentTime"

    let rxProgressiveAudioInteropStartAnimation = $"{rx}.ProgressiveAudioUtility.startPlayAnimation"

    let rxProgressiveAudioInteropStopAnimation = $"{rx}.ProgressiveAudioUtility.stopPlayAnimationAsync"
