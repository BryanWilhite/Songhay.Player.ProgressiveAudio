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

type PresentationParts =
    | CopyRights of Copyright list
    | Credits of RoleCredit list
    | Description of Description
    | Pages of string list
    | Playlist of (Title * Uri) list
    | Stream of StreamSegment list
