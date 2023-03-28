namespace Songhay.Player.ProgressiveAudio.Models

open System

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models

type Copyright =
    {
        year: DisplayText
        name: DisplayText
    }

    override this.ToString() = $"©{this.year.Value} {this.name.Value}"

type Description =
    | Description of DisplayText

    override this.ToString() = match this with Description dt -> dt.Value

type RoleCredit =
    {
        role: DisplayText
        name: DisplayText
    }

    override this.ToString() = $"{nameof(this.role)}{this.role.Value}; {nameof(this.name)}{this.name.Value}"

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
        | _ -> this.ToString()

    member this.StringValues =
        match this with
        | CopyRights l -> l |> List.map (fun i -> i.ToString())
        | Pages l -> l
        | _ -> [this.ToString()]
