namespace Songhay.StudioFloor.Client.Models

open Bolero

type StudioFloorPage =
    | [<EndPoint "/">] ReadMePage
    | [<EndPoint "/audio/">] BRollAudioPage of string
