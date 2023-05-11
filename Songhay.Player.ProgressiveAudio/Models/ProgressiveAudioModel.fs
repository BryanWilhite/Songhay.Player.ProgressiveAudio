namespace Songhay.Player.ProgressiveAudio.Models

open Songhay.Modules.Publications.Models

type ProgressiveAudioModel =
    {
        error: string option
        isCreditsModalVisible: bool
        isLoadingModalVisible: bool
        presentation: Presentation option
    }

    static member initialize =
        {
            error = None
            isCreditsModalVisible = false
            isLoadingModalVisible = true 
            presentation = None 
        }

    static member updateModel (message: ProgressiveAudioMessage) (model: ProgressiveAudioModel) =
        match message with
        | GetPlayerManifest -> { model with presentation = None }
        | GotPlayerManifest presentationOption -> { model with presentation = presentationOption }
        | PlayerCreditsClick -> { model with isCreditsModalVisible = not model.isCreditsModalVisible }
        | PlayerError exn -> { model with error = Some exn.Message }
