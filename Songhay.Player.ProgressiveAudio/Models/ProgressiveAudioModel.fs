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
        /// <summary>current playlist item info</summary>
        currentPlaylistItem: (DisplayText * Uri) option
        /// <summary>current error text</summary>
        error: string option
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
        /// <summary>defines the <see cref="ProgressiveAudioState"/> collection</summary>
        presentationStates: ProgressiveAudioStates
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
            currentPlaylistItem = None
            error = None
            playingDuration = 0m
            playingDurationDisplay = "00:00"
            playingCurrentTime = 0m
            playingCurrentTimeDisplay = "00:00"
            presentation = None
            presentationKey = None
            presentationStates = ProgressiveAudioStates.initialize
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

            {
                model with
                    presentation = presentationOption
                    presentationKey = data |> fst |> Some 
                    currentPlaylistItem = currentItem
            }

        | PlayerAudioCanPlayEvent ->
            model.blazorServices.jsRuntime |> consoleWarnAsync [| $"{message.StringValue}" |] |> ignore

            if model.presentationStates.hasState LoadingAfterPlaylistIsClicked && not (model.presentationStates.hasState Playing) then
                play() |> ignore
                model.presentationStates.addStates [CanPlay; Playing]
                model.presentationStates.removeState LoadingAfterPlaylistIsClicked
            else
                model.presentationStates.addState CanPlay

            model

        | PlayerAudioLoadStartEvent ->
            model.blazorServices.jsRuntime |> consoleWarnAsync [| $"{message.StringValue}" |] |> ignore
            model

        | PlayerAudioMetadataLoadedEvent ->
            model.blazorServices.jsRuntime |> consoleWarnAsync [| $"{message.StringValue}" |] |> ignore
            handleMeta() |> ignore
            model

        | PlayerAudioEndedEvent ->
            model.blazorServices.jsRuntime |> consoleWarnAsync [| $"{message.StringValue}" |] |> ignore
            model.presentationStates.removeState Playing
            model

        | PlayerPauseOrPlayButtonClickEvent ->
            if model.presentationStates.hasState Playing then pause() |> ignore
            else
                if model.presentationStates.hasState CanPlay then
                    play() |> ignore
                else
                    model.blazorServices.jsRuntime |> consoleWarnAsync [| "player cannot play!" |] |> ignore
                    ()
            model.presentationStates.toggleState Playing
            model

        | PlayerInputRangeInputEvent ->
            task {
                do! pause()
                do! handleMeta()
            } |> ignore

            model.presentationStates.removeState Playing
            model

        | PlayerInputRangeChangeEvent inputRef ->
            task {
                do! handleInputChange inputRef
            } |> ignore

            model.presentationStates.addState SeekingAfterSliderDrag
            model

        | PlayerAnimationTick data ->
            if data.audioReadyState > 2 // `HAVE_FUTURE_DATA` or `HAVE_ENOUGH_DATA`
            then model.presentationStates.addState CanPlay

            {
                model with
                    playingCurrentTime = data.audioCurrentTime
                    playingCurrentTimeDisplay = data.audioCurrentTime |> getTimeDisplayText
                    playingDuration = data.audioDuration |> Math.Floor
                    playingDurationDisplay = data.audioDuration |> getTimeDisplayText 
            }

        | PlayerCreditsClick ->
            model.presentationStates.toggleState CreditsModalVisible
            model

        | PlaylistClick (txt, uri) ->
            load uri |> ignore

            model.presentationStates.removeStates [CanPlay; Playing]
            model.presentationStates.addState LoadingAfterPlaylistIsClicked

            { model with currentPlaylistItem = (txt, uri) |> Some }

        | PlayerError exn ->
            model.blazorServices.jsRuntime |> consoleErrorAsync [| "player error!"; $"{message.StringValue}"; exn |] |> ignore
            { model with error = Some exn.Message }

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
