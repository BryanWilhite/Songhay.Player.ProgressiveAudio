namespace Songhay.Player.ProgressiveAudio.Components

open Bolero
open Bolero.Html

open Songhay.Player.ProgressiveAudio.Models

type PlayerElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()

    override this.View model dispatch =
        section {
            empty()
        }
