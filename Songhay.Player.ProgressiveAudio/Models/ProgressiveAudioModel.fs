namespace Songhay.Player.ProgressiveAudio.Models

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Songhay.Modules.Publications.Models

type ProgressiveAudioModel =
    {
        blazorServices: {| jsRuntime: IJSRuntime; navigationManager: NavigationManager |}
        error: string option
        isCreditsModalVisible: bool
        presentation: Presentation option
    }

    static member initialize (jsRuntime: IJSRuntime) (navigationManager: NavigationManager) =
        {
            blazorServices = {| jsRuntime = jsRuntime; navigationManager = navigationManager |}
            error = None
            isCreditsModalVisible = false
            presentation = None 
        }

    static member updateModel (message: ProgressiveAudioMessage) (model: ProgressiveAudioModel) =
        match message with
        | GetPlayerManifest -> { model with presentation = None }
        | GotPlayerManifest presentationOption -> { model with presentation = presentationOption }
        | PlayerCreditsClick -> { model with isCreditsModalVisible = not model.isCreditsModalVisible }
        | PlayerError exn -> { model with error = Some exn.Message }
