namespace Songhay.Player.ProgressiveAudio.Models

open System

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models

type Copyright = | Copyright of DisplayText

type Description = | Description of DisplayText

type IntroUri = | IntroUri of Uri

type RoleCredit =
    {
        role: DisplayText
        name: DisplayText
    }

type StreamSegment =
    {
        id: Id
        thumbnailUri: Uri
    }

type PresentationPart =
    | CopyRights of Copyright list
    | Credits of RoleCredit list
    | PresentationDescription of Description
    | Pages of string list
    | Playlist of (Title * Uri) list
    | Stream of StreamSegment list

    member this.StringValue =
        match this with
        | PresentationDescription (Description dt) -> dt.Value
        | _ -> this.ToString()

    member this.StringValues =
        match this with
        | CopyRights l -> l |> List.map (fun (Copyright dt) -> dt.Value)
        | Pages l -> l
        | PresentationDescription (Description dt) -> [dt.Value]
        | _ -> [this.ToString()]
