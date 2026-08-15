namespace Songhay.Player.ProgressiveAudio.Models

open System
open Microsoft.JSInterop

open FsToolkit.ErrorHandling

open Bolero

open Songhay.Modules
open Songhay.Modules.Models
open Songhay.Modules.Publications.Models

open Songhay.Modules.Bolero.JsRuntimeUtility
open Songhay.Modules.Bolero.ServiceProviderUtility

open Songhay.Player.ProgressiveAudio.ProgressiveAudioScalars
open Songhay.Player.ProgressiveAudio.ProgressiveAudioPresentationUtility

/// <summary>
/// The Elmish model of this domain.
/// </summary>
type ProgressiveAudioModel =
    {
        /// <summary>conventional Blazor services of this domain</summary>
        blazorServices: {|
                            sectionElementRef: HtmlRef option
                            audioElementRef: HtmlRef option
                            playerControlsComp: Component option
                        |}
        /// <summary>current playlist item info</summary>
        currentPlaylistItem: (DisplayText * Uri) option
        /// <summary>playlist of absolute URIs</summary>
        playlist: (DisplayText * Uri) list option
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
        /// <summary>defines the conventional <see cref="RestApiMetadata"/></summary>
        restApiMetadataOption: RestApiMetadata option
    }

    /// <summary>
    /// Centralizes the Elmish initialization routine.
    /// </summary>
    /// <param name="serviceProvider">the <see cref="IServiceProvider"/></param>
    static member initialize (serviceProvider: IServiceProvider) =
        setBlazorServiceProvider serviceProvider
        {
            blazorServices = {|
                               sectionElementRef = None
                               audioElementRef = None
                               playerControlsComp = None
                            |}
            currentPlaylistItem = None
            playlist = None
            error = None
            playingDuration = 0m
            playingDurationDisplay = "00:00"
            playingCurrentTime = 0m
            playingCurrentTimeDisplay = "00:00"
            presentation = None
            presentationKey = None
            presentationStates = AppStateSet.initialize
            restApiMetadataOption = "PlayerApi"
                                    |> RestApiMetadata.fromConfiguration (getIConfiguration())
                                    |> RestApiMetadata.toRestApiMetadataOption (getILogger().LogException)
        }

    /// <summary>
    /// Centralizes the model-updating for the Elmish <c>update</c> function.
    /// </summary>
    /// <param name="message">the <see cref="ProgressiveAudioMessage"/></param>
    /// <param name="model">the <see cref="ProgressiveAudioModel"/></param>
    static member updateModel (message: ProgressiveAudioMessage) (model: ProgressiveAudioModel) =
        let jsRuntime = getIJSRuntime()

        let dotNetObjectReference() = DotNetObjectReference.Create(model.blazorServices.playerControlsComp.Value)

        let audio() = model.blazorServices.audioElementRef.Value |> tryGetElementReference |> Result.valueOr raise

        let handleInputChange (htmlRef: HtmlRef) =
            let elementRef = htmlRef |> tryGetElementReference |> Result.valueOr raise
            jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropSetAudioCurrentTime, elementRef, audio())

        let handleMeta() =
            jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropHandleMetadataLoaded, dotNetObjectReference(), audio())

        let load (uri: Uri) =
            let htmlRef = model.blazorServices.audioElementRef.Value
            let elementRef = htmlRef |> tryGetElementReference |> Result.valueOr raise
            jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropLoadTrack, elementRef, uri.AbsoluteUri)

        let pause() =
            jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropStopAnimation, dotNetObjectReference(), audio())

        let play() =
            jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropStartAnimation, dotNetObjectReference(), audio())

        match message with
        | GetPlayerManifest _ -> { model with presentation = None }

        | GotPlayerSection sectionElementRef ->
            {
                model with blazorServices = {|
                                              sectionElementRef = sectionElementRef |> Some
                                              audioElementRef = model.blazorServices.audioElementRef
                                              playerControlsComp = model.blazorServices.playerControlsComp
                                            |}
            }

        | GotPlayerControlsRefs bag ->
            {
                model with blazorServices = {|
                                              sectionElementRef = model.blazorServices.sectionElementRef
                                              audioElementRef = bag.audioElementRef |> Some
                                              playerControlsComp = bag.playerControlsComp |> Some
                                            |}
            }

        | GotPlayerManifest data ->

            let bgImgUriOption =
                model.ToUriResultFromClaim("cdn-route-for-background", $"{(data |> fst).StringValue}")
                |> Option.ofResult

            data
            |>
            initializePresentation
                jsRuntime
                model.blazorServices.sectionElementRef
                bgImgUriOption

            let modifiedPlaylist =
                option {
                    let! presentation = data |> snd
                    let! meta = model.restApiMetadataOption
                    let! playlist = meta.GetApiBase() |> ApiBase |> presentation.toPlaylistWithApiBase None

                    return playlist
                }

            let currentItem =
                option {
                    let! playlist = modifiedPlaylist
                    let! head = playlist |> List.head

                    return head
                }

            {
                model with
                    presentation = data |> snd
                    presentationKey = data |> fst |> Some 
                    currentPlaylistItem = currentItem
                    playlist = modifiedPlaylist
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

            jsRuntime
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
            jsRuntime |> consoleWarnAsync [| $"{message.StringValue}" |] |> ignore
            model

        | PlayerAudioMetadataLoadedEvent ->
            jsRuntime |> consoleWarnAsync [| $"{message.StringValue}" |] |> ignore
            handleMeta() |> ignore
            model

        | PlayerAudioEndedEvent ->
            jsRuntime |> consoleWarnAsync [| $"{message.StringValue}" |] |> ignore

            { model with presentationStates = model.presentationStates.removeState Playing }

        | PlayerPauseOrPlayButtonClickEvent ->
            task {
                if model.presentationStates.hasState Playing then do! pause()
                else
                    if model.presentationStates.hasState CanPlay then
                        do! play()
                    else
                        do! jsRuntime |> consoleWarnAsync [| "player cannot play!" |]
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
            load uri |> ignore

            {
                model with
                    currentPlaylistItem = (txt, uri) |> Some
                    presentationStates = model.presentationStates
                                             .removeStates(CanPlay, Playing)
                                             .addState LoadingAfterPlaylistIsClicked
            }

        | PlayerError exn ->
            jsRuntime |> consoleErrorAsync [| "player error!"; $"{message.StringValue}"; exn |] |> ignore
            { model with error = Some exn.Message }

    /// <summary>
    /// An <c>option</c> wrapper for <see cref="RestApiMetadata.ToUriResultFromClaim"/>.
    /// </summary>
    member this.ToUriResultFromClaim(key: string, [<ParamArray>] args: string[]) =
        this.restApiMetadataOption
        |> Option.either
            (
                fun restApiMetadata ->
                    restApiMetadata.ToUriResultFromClaim(key, args)
                    |> Result.teeError (getILogger().LogException)
            )
            (fun () -> Error <| exn $"The expected {nameof this.restApiMetadataOption} is not here.")

    /// <summary>
    /// Chooses any <see cref="RoleCredit"/> list of the current <see cref="Presentation"/>.
    /// </summary>
    member this.PresentationCredits =
        option {
            let! pres = this.presentation
            return! pres.credits
        }

    /// <summary>
    /// Chooses any <see cref="Description"/> <see cref="string"/> of the current <see cref="Presentation"/>.
    /// </summary>
    member this.PresentationDescription =
        option {
            let! pres = this.presentation
            return! pres.description
        }

    /// <summary>
    /// Chooses any <see cref="Playlist"/> tuple list of the current <see cref="Presentation"/>.
    /// </summary>
    member this.PresentationPlaylist =
        option {
            let! pres = this.presentation
            return! pres.playlist
        }
