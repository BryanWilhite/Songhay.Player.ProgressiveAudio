namespace Songhay.Player.ProgressiveAudio.Models

/// <summary>
/// Defines the animation tick data
/// passed from the browser to this domain.
/// </summary>
type PlayerAnimationTickData =
    {
        /// <summary> maps to <c>https://developer.mozilla.org/en-US/docs/Web/API/HTMLMediaElement/currentTime</c></summary>
        audioCurrentTime: decimal
        /// <summary> maps to <c>https://developer.mozilla.org/en-US/docs/Web/API/HTMLMediaElement/duration</c></summary>
        audioDuration: decimal
        /// <summary> maps to <c>https://developer.mozilla.org/en-US/docs/Web/API/HTMLMediaElement/readyState</c></summary>
        audioReadyState: int
    }
