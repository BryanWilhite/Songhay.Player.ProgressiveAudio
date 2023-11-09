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
        presentationStates: AppStateSet<ProgressiveAudioState>
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
            presentationStates = AppStates Set.empty
        }

    /// <summary>
    /// Centralizes the model-updating for the Elmish <c>update</c> function.
    /// </summary>
    /// <param name="message">the <see cref="ProgressiveAudioMessage"/></param>
    /// <param name="model">the <see cref="ProgressiveAudioModel"/></param>
    static member updateModel (message: ProgressiveAudioMessage) (model: ProgressiveAudioModel) =
        let dotNetObjectReference() = DotNetObjectReference.Create(model.blazorServices.playerControlsComp.Value)

        let handleInputChange (htmlRef: HtmlRef) =
            let elementRef = htmlRef |> tryGetElementReference |> Result.valueOr raise
            model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropSetAudioCurrentTime, elementRef)

        let handleMeta() =
            model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropHandleMetadataLoaded, dotNetObjectReference())

        let pause() =
            model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropStopAnimation, dotNetObjectReference())

        let play() =
            model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropStartAnimation, dotNetObjectReference())

        match message with
        | GetPlayerManifest _ -> { model with presentation = None }

        | GotPlayerSection sectionElementRef ->
            {
                model with blazorServices = {|
                                              jsRuntime = model.blazorServices.jsRuntime
                                              sectionElementRef = sectionElementRef |> Some
                                              playerControlsComp = model.blazorServices.playerControlsComp
                                            |}
            }

        | GotPlayerControlsRefs bag ->
            {
                model with blazorServices = {|
                                              jsRuntime = model.blazorServices.jsRuntime
                                              sectionElementRef = model.blazorServices.sectionElementRef
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

            let autoplay =
                not (model.presentationStates.hasState Playing)
                &&
                (
                    model.presentationStates.hasState LoadingAfterPlaylistIsClicked
                    ||
                    model.presentationStates.hasState SeekingAfterSliderDrag
                )

            model.blazorServices.jsRuntime
            |> consoleWarnAsync [| $"{message.StringValue}"; $"{nameof autoplay}: {autoplay}" |] |> ignore

            if autoplay then
                play() |> ignore

                {
                    model with
                        presentationStates = model
                                                 .presentationStates
                                                 .addStates(CanPlay, Playing)
                                                 .removeStates(LoadingAfterPlaylistIsClicked, SeekingAfterSliderDrag)
                }
            else
                { model with presentationStates = model.presentationStates.addState CanPlay }

        | PlayerAudioLoadStartEvent ->
            model.blazorServices.jsRuntime |> consoleWarnAsync [| $"{message.StringValue}" |] |> ignore
            model

        | PlayerAudioMetadataLoadedEvent ->
            model.blazorServices.jsRuntime |> consoleWarnAsync [| $"{message.StringValue}" |] |> ignore
            handleMeta() |> ignore
            model

        | PlayerAudioEndedEvent ->
            model.blazorServices.jsRuntime |> consoleWarnAsync [| $"{message.StringValue}" |] |> ignore

            { model with presentationStates = model.presentationStates.removeState Playing }

        | PlayerPauseOrPlayButtonClickEvent ->
            task {
                if model.presentationStates.hasState Playing then do! pause()
                else
                    if model.presentationStates.hasState CanPlay then
                        do! play()
                    else
                        do! model.blazorServices.jsRuntime |> consoleWarnAsync [| "player cannot play!" |]
            } |> ignore

            { model with presentationStates = model.presentationStates.toggleState Playing }

        | PlayerInputRangeInputEvent ->
            task {
                do! pause()
                do! handleMeta()
            } |> ignore

            { model with presentationStates = model.presentationStates.removeState Playing }

        | PlayerInputRangeChangeEvent inputRef ->
            task {
                do! handleInputChange inputRef
            } |> ignore

            { model with presentationStates = model.presentationStates.addState SeekingAfterSliderDrag }

        | PlayerAnimationTick data ->

            {
                model with
                    playingCurrentTime = data.audioCurrentTime
                    playingCurrentTimeDisplay = data.audioCurrentTime |> getTimeDisplayText
                    playingDuration = data.audioDuration |> Math.Floor
                    playingDurationDisplay = data.audioDuration |> getTimeDisplayText
                    presentationStates =
                        if data.audioReadyState > 2 // `HAVE_FUTURE_DATA` or `HAVE_ENOUGH_DATA`
                        then model.presentationStates.addState CanPlay
                        else model.presentationStates
            }

        | PlayerCreditsClick ->
            { model with presentationStates = model.presentationStates.toggleState CreditsModalVisible }

        | PlaylistClick (txt, uri) ->
            {
                model with
                    currentPlaylistItem = (txt, uri) |> Some
                    presentationStates = model.presentationStates
                                             .removeStates(CanPlay, Playing)
                                             .addState LoadingAfterPlaylistIsClicked
            }

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
