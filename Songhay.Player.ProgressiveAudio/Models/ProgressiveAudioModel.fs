namespace Songhay.Player.ProgressiveAudio.Models

open System
open Microsoft.JSInterop

open FsToolkit.ErrorHandling

open Bolero

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models
open Songhay.Modules.Bolero.JsRuntimeUtility

open Songhay.Player.ProgressiveAudio.ProgressiveAudioScalars
open Songhay.Player.ProgressiveAudio.ProgressiveAudioPresentationUtility

/// <summary>
/// The Elmish model of this domain.
/// </summary>
type ProgressiveAudioModel =
    {
        /// <summary>conventional Blazor services of this domain</summary>
        blazorServices: {|
                          jsRuntime: IJSRuntime
                          sectionElementRef: HtmlRef option
                          audioElementRef: HtmlRef option
                          playerControlsComp: Component option
                        |}
        /// <summary>returns <c>true</c> when the event of the same name fires for the <c>audio</c> element</summary>
        /// <remarks>See https://developer.mozilla.org/en-US/docs/Web/Guide/Audio_and_video_delivery/Cross-browser_audio_basics#canplay</remarks>
        canPlay: bool
        /// <summary>current playlist item info</summary>
        currentPlaylistItem: (DisplayText * Uri) option
        /// <summary>current error text</summary>
        error: string option
        /// <summary>returns <c>true</c> when the credits modal is visible</summary>
        isCreditsModalVisible: bool
        /// <summary>returns <c>true</c> when the <see cref="Presentation"/> is playing</summary>
        isPlaying: bool
        /// <summary>the latest value of <see cref="PlayerAnimationTickData.audioCurrentTime"/></summary>
        playingCurrentTime: decimal
        /// <summary>the latest value of <see cref="PlayerAnimationTickData.audioDuration"/></summary>
        playingDuration: decimal
        /// <summary>the latest value of <see cref="PlayerAnimationTickData.audioDuration"/> formatted with <see cref="getTimeDisplayText"/></summary>
        playingDurationDisplay: string
        /// <summary>the latest value of <see cref="PlayerAnimationTickData.audioCurrentTime"/> formatted with <see cref="getTimeDisplayText"/></summary>
        playingCurrentTimeDisplay: string
        /// <summary>the current <see cref="Presentation"/></summary>
        presentation: Presentation option
        /// <summary>the current <see cref="Presentation"/> <see cref="Identifier"/></summary>
        presentationKey: Identifier option
    }

    /// <summary>
    /// Centralizes the Elmish initialization routine.
    /// </summary>
    /// <param name="jsRuntime">the <see cref="IJSRuntime"/></param>
    static member initialize (jsRuntime: IJSRuntime) =
        {
            blazorServices = {|
                               jsRuntime = jsRuntime
                               sectionElementRef = None
                               audioElementRef = None
                               playerControlsComp = None
                            |}
            canPlay = false 
            currentPlaylistItem = None
            error = None
            isCreditsModalVisible = false
            isPlaying = false
            playingDuration = 0m
            playingDurationDisplay = "00:00"
            playingCurrentTime = 0m
            playingCurrentTimeDisplay = "00:00"
            presentation = None
            presentationKey = None 
        }

    /// <summary>
    /// Centralizes the model-updating for the Elmish <c>update</c> function.
    /// </summary>
    /// <param name="message">the <see cref="ProgressiveAudioMessage"/></param>
    /// <param name="model">the <see cref="ProgressiveAudioModel"/></param>
    static member updateModel (message: ProgressiveAudioMessage) (model: ProgressiveAudioModel) =
        let dotNetObjectReference() = DotNetObjectReference.Create(model.blazorServices.playerControlsComp.Value)

        let audio() = model.blazorServices.audioElementRef.Value |> tryGetElementReference |> Result.valueOr raise

        let handleInputChange (htmlRef: HtmlRef) =
            let elementRef = htmlRef |> tryGetElementReference |> Result.valueOr raise
            model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropSetAudioCurrentTime, elementRef, audio())

        let handleMeta() =
            model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropHandleMetadataLoaded, dotNetObjectReference(), audio())

        let load (uri: Uri) =
            let htmlRef = model.blazorServices.audioElementRef.Value
            let elementRef = htmlRef |> tryGetElementReference |> Result.valueOr raise
            model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropLoadTrack, elementRef, uri.AbsoluteUri)

        let pause() =
            model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropStopAnimation, dotNetObjectReference(), audio())

        let play() =
            model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropStartAnimation, dotNetObjectReference(), audio())

        match message with
        | GetPlayerManifest _ -> { model with presentation = None }

        | GotPlayerSection sectionElementRef ->
            {
                model with blazorServices = {|
                                              jsRuntime = model.blazorServices.jsRuntime
                                              sectionElementRef = sectionElementRef |> Some
                                              audioElementRef = model.blazorServices.audioElementRef
                                              playerControlsComp = model.blazorServices.playerControlsComp
                                            |}
            }

        | GotPlayerControlsRefs bag ->
            {
                model with blazorServices = {|
                                              jsRuntime = model.blazorServices.jsRuntime
                                              sectionElementRef = model.blazorServices.sectionElementRef
                                              audioElementRef = bag.audioElementRef |> Some
                                              playerControlsComp = bag.playerControlsComp |> Some
                                            |}
            }

        | GotPlayerManifest data ->
            let presentationOption =
                data
                |>
                toPresentationOption
                    model.blazorServices.jsRuntime
                    model.blazorServices.sectionElementRef
                    (fun (txt, uri) -> (txt, uri |> buildAudioRootUri))

            let currentItem =
                option {
                    let! presentation = presentationOption
                    let! playList = presentation.playList

                    return playList |> List.head
                }

            { model with
                    presentation = presentationOption
                    presentationKey = data |> fst |> Some 
                    currentPlaylistItem = currentItem }

        | PlayerAudioCanPlayEvent ->
            play() |> ignore
            { model with canPlay = true; isPlaying = true }

        | PlayerAudioLoadStartEvent ->
            model.blazorServices.jsRuntime |> consoleInfoAsync [| nameof PlayerAudioLoadStartEvent |] |> ignore
            model

        | PlayerAudioMetadataLoadedEvent ->
            handleMeta() |> ignore
            model

        | PlayerAudioEndedEvent -> { model with isPlaying = false }

        | PlayerPauseButtonClickEvent ->
            if model.isPlaying then pause() |> ignore
            else
                if model.canPlay then
                    play() |> ignore
                else
                    ()
            { model with isPlaying = not model.isPlaying }

        | PlayerPauseInputEvent ->
            task {
                do! pause()
                do! handleMeta()
            } |> ignore

            { model with isPlaying = false }

        | PlayerPauseChangeEvent inputRef ->
            task {
                do! handleInputChange inputRef
                if model.canPlay then
                    do! play()
                else
                    ()
            } |> ignore

            { model with isPlaying = true }

        | PlayerAnimationTick data ->
            {
                model with
                    playingCurrentTime = data.audioCurrentTime
                    playingCurrentTimeDisplay = data.audioCurrentTime |> getTimeDisplayText
                    playingDuration = data.audioDuration |> Math.Floor
                    playingDurationDisplay = data.audioDuration |> getTimeDisplayText 
            }

        | PlayerCreditsClick -> { model with isCreditsModalVisible = not model.isCreditsModalVisible }

        | PlaylistClick (txt, uri) ->
            load uri |> ignore

            { model with currentPlaylistItem = (txt, uri) |> Some; canPlay = false;  isPlaying = false }

        | PlayerError exn -> { model with error = Some exn.Message }

    /// <summary>
    /// Chooses any <see cref="RoleCredit"/> list of the current <see cref="Presentation"/>.
    /// </summary>
    member this.presentationCredits =
        option {
            let! pres = this.presentation
            return! pres.credits
        }

    /// <summary>
    /// Chooses any <see cref="Description"/> <see cref="string"/> of the current <see cref="Presentation"/>.
    /// </summary>
    member this.presentationDescription =
        option {
            let! pres = this.presentation
            return! pres.description
        }

    /// <summary>
    /// Chooses any <see cref="Playlist"/> tuple list of the current <see cref="Presentation"/>.
    /// </summary>
    member this.presentationPlayList =
        option {
            let! pres = this.presentation
            return! pres.playList
        }
