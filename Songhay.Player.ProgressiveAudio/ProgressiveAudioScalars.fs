namespace Songhay.Player.ProgressiveAudio

open Songhay.Modules.Bolero.JsRuntimeUtility

/// <summary>
/// Conventional scalar values of this domain.
/// </summary>
module ProgressiveAudioScalars =

    /// <summary> conventional scalar </summary>
    let rxProgressiveAudioInteropHandleMetadataLoaded = $"{rx}.ProgressiveAudioUtility.handleAudioMetadataLoadedAsync"

    /// <summary> conventional scalar </summary>
    let rxProgressiveAudioInteropLoadTrack = $"{rx}.ProgressiveAudioUtility.loadAudioTrack"

    /// <summary> conventional scalar </summary>
    let rxProgressiveAudioInteropSetAudioCurrentTime = $"{rx}.ProgressiveAudioUtility.setAudioCurrentTime"

    /// <summary> conventional scalar </summary>
    let rxProgressiveAudioInteropStartAnimation = $"{rx}.ProgressiveAudioUtility.startPlayAnimationAsync"

    /// <summary> conventional scalar </summary>
    let rxProgressiveAudioInteropStopAnimation = $"{rx}.ProgressiveAudioUtility.stopPlayAnimationAsync"
