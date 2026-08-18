namespace Songhay.Player.ProgressiveAudio.Client.Models

open Bolero

type AudioClientPage =
    | [<EndPoint "/">] NoContentPage
    | [<EndPoint "/{key}">] BRollAudioPage of key: string
