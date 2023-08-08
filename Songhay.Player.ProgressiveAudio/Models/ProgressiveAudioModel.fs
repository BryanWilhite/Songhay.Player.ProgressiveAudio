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

type ProgressiveAudioModel =
    {
        blazorServices: {| jsRuntime: IJSRuntime; navigationManager: NavigationManager; playerControlsRef: Component option |}
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

    static member internal buildAudioRootUri (relativeUri: Uri) =
        if relativeUri.IsAbsoluteUri then relativeUri
        else
            let builder = UriBuilder(rxProgressiveAudioRoot)
            builder.Path <- $"{builder.Path}{relativeUri.OriginalString.TrimStart([|'.';'/'|])}"
            builder.Uri

    static member internal getTimeDisplayText secs =
        let minutes = Math.Floor(secs / 60m)
        let seconds = Math.Floor(secs % 60m)

        $"{minutes:``00``}:{seconds:``00``}"

    static member initialize (jsRuntime: IJSRuntime) (navigationManager: NavigationManager) =
        {
            blazorServices = {| jsRuntime = jsRuntime; navigationManager = navigationManager; playerControlsRef = None |}
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
        match message with
        | GetPlayerManifest -> { model with presentation = None }
        | GotPlayerControlsRef ref ->
            {
                model with blazorServices = {|
                                              jsRuntime = model.blazorServices.jsRuntime
                                              navigationManager = model.blazorServices.navigationManager
                                              playerControlsRef = ref |> Some
                                            |}
            }
        | GotPlayerManifest data ->

            let presentationOption =
                option {
                    let! presentation = data

                    let map list =
                        list
                        |> List.map(fun (txt, uri) -> (txt, uri |> ProgressiveAudioModel.buildAudioRootUri))
                        |> Playlist

                    let parts =
                        presentation.parts
                        |> List.map(fun part -> match part with | Playlist list -> list |> map | _ -> part)

                    return { presentation with parts = parts }
                }

            let item =
                option {
                    let! presentation = presentationOption

                    return presentation |> ProgressiveAudioModel.getPlayList |> List.head
                }

            {
                model with
                    presentation = presentationOption
                    currentPlaylistItem = item
            }

        | PlayPauseControlClick ->
            let dotNetObjectReference = DotNetObjectReference.Create(model.blazorServices.playerControlsRef.Value)
            let qualifiedName =
                if model.isPlaying then rxProgressiveAudioInteropStopAnimation
                else rxProgressiveAudioInteropStartAnimation

            model.blazorServices.jsRuntime.InvokeVoidAsync(qualifiedName, dotNetObjectReference) |> ignore

            { model with isPlaying = not model.isPlaying }

        | PlayerAnimationTick data ->
            model.blazorServices.jsRuntime |> consoleInfoAsync [| nameof(PlayerAnimationTick) |] |> ignore

            {
                model with
                    playingCurrentTime = data.audioCurrentTime
                    playingCurrentTimeDisplay = data.audioCurrentTime |> ProgressiveAudioModel.getTimeDisplayText
                    playingDuration = data.audioDuration |> Math.Floor
                    playingDurationDisplay = data.audioDuration |> ProgressiveAudioModel.getTimeDisplayText 
            }

        | PlayerCreditsClick -> { model with isCreditsModalVisible = not model.isCreditsModalVisible }
        | PlaylistClick item ->
            let dotNetObjectReference = DotNetObjectReference.Create(model.blazorServices.playerControlsRef.Value)

            task {
                do! model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropStopAnimation, dotNetObjectReference)
                do! model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropLoadTrack, (item |> snd).AbsoluteUri)
                do! model.blazorServices.jsRuntime.InvokeVoidAsync(rxProgressiveAudioInteropStartAnimation, dotNetObjectReference)
            } |> ignore

            { model with currentPlaylistItem = item |> Some }

        | PlayerError exn -> { model with error = Some exn.Message }

    member this.presentationCredits = this.presentation |> Option.map ProgressiveAudioModel.getCredits

    member this.presentationDescription = this.presentation |> Option.map ProgressiveAudioModel.getDescription

    member this.presentationPlayList = this.presentation |> Option.map ProgressiveAudioModel.getPlayList
