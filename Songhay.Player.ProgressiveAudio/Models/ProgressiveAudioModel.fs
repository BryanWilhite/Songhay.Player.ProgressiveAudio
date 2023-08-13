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
open Songhay.Player.ProgressiveAudio.ProgressiveAudioUtility

type ProgressiveAudioModel =
    {
        blazorServices: {|
                          jsRuntime: IJSRuntime
                          navigationManager: NavigationManager
                          audioElementRef: HtmlRef option
                          buttonElementRef: HtmlRef option
                          playerControlsComp: Component option
                        |}
        currentPlaylistItem: (DisplayText * Uri) option
        error: string option
        isCreditsModalVisible: bool
        isPlaying: bool
        playingCurrentTime: decimal
        playingDuration: decimal
        playingDurationDisplay: string
        playingCurrentTimeDisplay: string
        presentation: Presentation option
    }

    static member private getCredits p =
        p.parts
        |> List.choose (function | PresentationPart.Credits l -> Some l | _ -> None)

    static member private getDescription p =
        p.parts
        |> List.choose (function | PresentationPart.PresentationDescription s -> Some s | _ -> None)
        |> List.head

    static member private getPlayList p =
        p.parts
        |> List.choose (function | PresentationPart.Playlist pl -> pl |> Some | _ -> None)
        |> List.head

    static member initialize (jsRuntime: IJSRuntime) (navigationManager: NavigationManager) =
        {
            blazorServices = {|
                               jsRuntime = jsRuntime
                               navigationManager = navigationManager
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
        }

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
        | GotPlayerControlsRefs bag ->
            {
                model with blazorServices = {|
                                              jsRuntime = model.blazorServices.jsRuntime
                                              navigationManager = model.blazorServices.navigationManager
                                              audioElementRef = bag.audioElementRef |> Some
                                              buttonElementRef = bag.buttonElementRef |> Some
                                              playerControlsComp = bag.playerControlsComp |> Some
                                            |}
            }

        | GotPlayerManifest data ->
            let presentationOption =
                toPresentationOption
                    data
                    (fun (txt, uri) -> (txt, uri |> buildAudioRootUri))

            let currentItem =
                option {
                    let! presentation = presentationOption

                    return presentation |> ProgressiveAudioModel.getPlayList |> List.head
                }

            { model with
                    presentation = presentationOption
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

    member this.presentationCredits = this.presentation |> Option.map ProgressiveAudioModel.getCredits

    member this.presentationDescription = this.presentation |> Option.map ProgressiveAudioModel.getDescription

    member this.presentationPlayList = this.presentation |> Option.map ProgressiveAudioModel.getPlayList
