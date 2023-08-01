namespace Songhay.Player.ProgressiveAudio.Models

open System
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open FsToolkit.ErrorHandling

open Songhay.Modules.Models
open Songhay.Player.ProgressiveAudio.ProgressiveAudioScalars
open Songhay.Modules.Publications.Models

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
            builder.Path <- relativeUri.OriginalString
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
        | GotPlayerManifest presentationOption ->
            let item =
                option {
                    let! presentation = presentationOption
                    let list =
                        presentation
                        |> ProgressiveAudioModel.getPlayList
                        |> List.map (fun (txt, uri) -> (txt, uri |> ProgressiveAudioModel.buildAudioRootUri))

                    return list |> List.head
                }
            { model with presentation = presentationOption; currentPlaylistItem = item }
        | PlayPauseControl -> { model with isPlaying = not model.isPlaying }
        | PlayerCreditsClick -> { model with isCreditsModalVisible = not model.isCreditsModalVisible }
        | PlayerError exn -> { model with error = Some exn.Message }

    member this.presentationCredits = this.presentation |> Option.map ProgressiveAudioModel.getCredits

    member this.presentationDescription = this.presentation |> Option.map ProgressiveAudioModel.getDescription

    member this.presentationPlayList = this.presentation |> Option.map ProgressiveAudioModel.getPlayList
