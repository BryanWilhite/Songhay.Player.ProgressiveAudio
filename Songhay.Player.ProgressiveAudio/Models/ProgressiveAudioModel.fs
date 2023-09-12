namespace Songhay.Player.ProgressiveAudio.Models

open System
open Microsoft.AspNetCore.Components
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
                          navigationManager: NavigationManager
                          sectionElementRef: HtmlRef option
                          audioElementRef: HtmlRef option
                          buttonElementRef: HtmlRef option
                          playerControlsComp: Component option
                        |}
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
    /// <param name="navigationManager">the <see cref="NavigationManager"/></param>
    static member initialize (jsRuntime: IJSRuntime) (navigationManager: NavigationManager) =
        {
            blazorServices = {|
                               jsRuntime = jsRuntime
                               navigationManager = navigationManager
                               sectionElementRef = None
                               audioElementRef = None
                               buttonElementRef = None
                               playerControlsComp = None
                            |}
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

        let button() = model.blazorServices.buttonElementRef.Value |> tryGetElementReference |> Result.valueOr raise
        let audio() = model.blazorServices.audioElementRef.Value |> tryGetElementReference |> Result.valueOr raise

        let handleInputChange (htmlRef: HtmlRef) =
            let elementRef = htmlRef |> tryGetElementReference |> Result.valueOr raise
            model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropSetAudioCurrentTime, elementRef, audio())

        let handleMeta() =
            model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropHandleMetadataLoaded, dotNetObjectReference(), button(), audio())

        let load (uri: Uri) =
            let htmlRef = model.blazorServices.audioElementRef.Value
            let elementRef = htmlRef |> tryGetElementReference |> Result.valueOr raise
            model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropLoadTrack, elementRef, uri.AbsoluteUri)

        let pause() =
            model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropStopAnimation, dotNetObjectReference(), button(), audio())

        let play() =
            model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropStartAnimation, dotNetObjectReference(), button(), audio())

        match message with
        | GetPlayerManifest -> { model with presentation = None }
        | GotPlayerSection sectionElementRef ->
            {
                model with blazorServices = {|
                                              jsRuntime = model.blazorServices.jsRuntime
                                              navigationManager = model.blazorServices.navigationManager
                                              sectionElementRef = sectionElementRef |> Some
                                              audioElementRef = model.blazorServices.audioElementRef
                                              buttonElementRef = model.blazorServices.buttonElementRef
                                              playerControlsComp = model.blazorServices.playerControlsComp
                                            |}
            }
        | GotPlayerControlsRefs bag ->
            {
                model with blazorServices = {|
                                              jsRuntime = model.blazorServices.jsRuntime
                                              navigationManager = model.blazorServices.navigationManager
                                              sectionElementRef = model.blazorServices.sectionElementRef
                                              audioElementRef = bag.audioElementRef |> Some
                                              buttonElementRef = bag.buttonElementRef |> Some
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

        | PlayAudioMetadataLoadedEvent ->
            handleMeta() |> ignore
            model

        | PlayAudioEndedEvent -> { model with isPlaying = false }
        | PlayPauseButtonClickEvent ->
            if model.isPlaying then pause() |> ignore else play() |> ignore
            { model with isPlaying = not model.isPlaying }

        | PlayPauseInputEvent ->
            task {
                do! pause()
                do! handleMeta()
            } |> ignore

            { model with isPlaying = false }

        | PlayPauseChangeEvent inputRef ->
            task {
                do! handleInputChange inputRef
                do! play()
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
            task {
                do! pause()
                do! load uri
                do! play()
            } |> ignore

            { model with currentPlaylistItem = (txt, uri) |> Some; isPlaying = true }

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
