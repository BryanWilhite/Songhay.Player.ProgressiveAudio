namespace Songhay.Player.ProgressiveAudio.Models

open System.Collections.Generic

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

/// <summary>defines the <see cref="ProgressiveAudioState"/> collection</summary>
type ProgressiveAudioStates =
    /// <summary>collects the <see cref="ProgressiveAudioState"/></summary>
    | ProgressiveAudioStates of HashSet<ProgressiveAudioState>

    static member initialize = ProgressiveAudioStates <| HashSet<ProgressiveAudioState>()

    /// <summary>adds the specified <see cref="ProgressiveAudioState"/></summary>
    member this.addState state = this.value.Add(state) |> ignore

    /// <summary>adds the specified list of <see cref="ProgressiveAudioState"/></summary>
    member this.addStates states = states |> Set.ofList |> Set.iter this.addState

    /// <summary>returns <c>true</c> when the specified <see cref="ProgressiveAudioState"/> is collected</summary>
    member this.hasState state = this.value.Contains(state)

    /// <summary>removes the specified <see cref="ProgressiveAudioState"/></summary>
    member this.removeState state = this.value.Remove(state) |> ignore

    /// <summary>removes the specified list of <see cref="ProgressiveAudioState"/></summary>
    member this.removeStates states = states |> Set.ofList |> Set.iter this.removeState

    /// <summary>toggles the specified <see cref="ProgressiveAudioState"/></summary>
    member this.toggleState state =
        if this.hasState state then this.removeState state
        else this.addState state

    /// <summary>returns the underlying <see cref="HashSet{T}"/></summary>
    member this.value: HashSet<ProgressiveAudioState> = let (ProgressiveAudioStates set) = this in set
