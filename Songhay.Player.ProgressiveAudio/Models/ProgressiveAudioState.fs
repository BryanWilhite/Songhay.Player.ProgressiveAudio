namespace Songhay.Player.ProgressiveAudio.Models

/// <summary>defines player states for the <see cref="Presentation"/></summary>
type ProgressiveAudioState =
    /// <summary>when the event of the same name fires for the <c>audio</c> DOM element</summary>
    /// <remarks>See https://developer.mozilla.org/en-US/docs/Web/Guide/Audio_and_video_delivery/Cross-browser_audio_basics#canplay</remarks>
    | CanPlay
    /// <summary>when the credits modal of the <see cref="Presentation"/> is visible</summary>
    | CreditsModalVisible
    /// <summary>when the <see cref="Presentation"/> is loading (after playlist is clicked)</summary>
    | LoadingAfterPlaylistIsClicked
    /// <summary>when the <see cref="Presentation"/> is playing</summary>
    | Playing
    /// <summary>after the input range slider for the <c>audio</c> DOM element is released</summary>
    | SeekingAfterSliderDrag
