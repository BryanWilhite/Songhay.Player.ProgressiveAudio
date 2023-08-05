namespace Songhay.Player.ProgressiveAudio.Models

open System
open Elmish
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open FsToolkit.ErrorHandling

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models
open Songhay.Modules.Bolero.JsRuntimeUtility
open Songhay.Player.ProgressiveAudio.ProgressiveAudioScalars

type ProgressiveAudioModel =
    {
        blazorServices: {| jsRuntime: IJSRuntime; navigationManager: NavigationManager |}
        currentPlaylistItem: (DisplayText * Uri) option
        error: string option
        isCreditsModalVisible: bool
        isPlaying: bool
        playingDuration: string
        playingProgress: string
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

    static member buildAudioRootUri (relativeUri: Uri) =
        if relativeUri.IsAbsoluteUri then relativeUri
        else
            let builder = UriBuilder(rxProgressiveAudioRoot)
            builder.Path <- $"{builder.Path}{relativeUri.OriginalString.TrimStart([|'.';'/'|])}"
            builder.Uri

    static member initialize (jsRuntime: IJSRuntime) (navigationManager: NavigationManager) =
        {
            blazorServices = {| jsRuntime = jsRuntime; navigationManager = navigationManager |}
            currentPlaylistItem = None
            error = None
            isCreditsModalVisible = false
            isPlaying = false
            playingDuration = "00:00" 
            playingProgress = "00:00" 
            presentation = None
        }

    static member updateModel (message: ProgressiveAudioMessage) (model: ProgressiveAudioModel) =
        match message with
        | GetPlayerManifest -> { model with presentation = None }
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

            { model with presentation = presentationOption; currentPlaylistItem = item }

        | PlayPauseControlClick ->
            let dotNetObjectReference = DotNetObjectReference.Create(model)
            let qualifiedName =
                if model.isPlaying then rxProgressiveAudioInteropStopAnimation
                else rxProgressiveAudioInteropStartAnimation

            model.blazorServices.jsRuntime.InvokeVoidAsync(qualifiedName, dotNetObjectReference) |> ignore

            { model with isPlaying = not model.isPlaying }

        | PlayerAnimationTick ->
            model
        | PlayerCreditsClick -> { model with isCreditsModalVisible = not model.isCreditsModalVisible }
        | PlaylistClick item ->
            let dotNetObjectReference = DotNetObjectReference.Create(model)

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

    [<JSInvokable>]
    member this.animateAsync(uiData: {|
                                       animationStatus: string option
                                       audioDuration: double option
                                       audioReadyState: int option
                                       isAudioPaused: bool option |}) =

        let cmd = Cmd.ofMsg PlayerAnimationTick

        this.blazorServices.jsRuntime |> consoleInfoAsync [| uiData |]
