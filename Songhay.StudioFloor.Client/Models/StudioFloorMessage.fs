namespace Songhay.StudioFloor.Client.Models

open Songhay.Player.ProgressiveAudio.Models

type StudioFloorMessage =
    | Error of exn
    | GetReadMe | GotReadMe of string
    | ProgressiveAudioMessage of ProgressiveAudioMessage
    | SetTab of StudioFloorTab
