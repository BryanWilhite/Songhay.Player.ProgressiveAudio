namespace Songhay.Player.ProgressiveAudio.Models

open Songhay.Modules.Publications.Models

type ProgressiveAudioModel =
    {
        error: string option
        presentation: Presentation option
    }

    static member initialize =
        {
            error = None
            presentation = None 
        }

    static member updateModel (message: ProgressiveAudioMessage) (model: ProgressiveAudioModel) =
        match message with
        | PlayerError exn -> { model with error = Some exn.Message }
        | GetPlayerManifest -> { model with presentation = None }
        | GotPlayerManifest presentationOption -> { model with presentation = presentationOption }
