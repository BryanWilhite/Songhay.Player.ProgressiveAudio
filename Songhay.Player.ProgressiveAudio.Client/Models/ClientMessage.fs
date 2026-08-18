namespace Songhay.Player.ProgressiveAudio.Client.Models

open Songhay.Player.ProgressiveAudio.Models

type AudioClientMessage =
    | Error of exn
    | ProgressiveAudioMessage of ProgressiveAudioMessage
    | SetPage of AudioClientPage
