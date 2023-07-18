namespace Songhay.Player.ProgressiveAudio.Models

open System
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models

type ProgressiveAudioModel =
    {
        blazorServices: {| jsRuntime: IJSRuntime; navigationManager: NavigationManager |}
        currentPlaylistItem: (DisplayText * Uri) option
        error: string option
        isCreditsModalVisible: bool
        isPlaying: bool
        presentation: Presentation option
    }

    static member initialize (jsRuntime: IJSRuntime) (navigationManager: NavigationManager) =
        {
            blazorServices = {| jsRuntime = jsRuntime; navigationManager = navigationManager |}
            currentPlaylistItem = None 
            error = None
            isCreditsModalVisible = false
            isPlaying = false 
            presentation = None 
        }

    static member updateModel (message: ProgressiveAudioMessage) (model: ProgressiveAudioModel) =
        match message with
        | GetPlayerManifest -> { model with presentation = None }
        | GotPlayerManifest presentationOption -> { model with presentation = presentationOption }
        | PlayerCreditsClick -> { model with isCreditsModalVisible = not model.isCreditsModalVisible }
        | PlayerError exn -> { model with error = Some exn.Message }
