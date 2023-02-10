namespace Songhay.Player.ProgressiveAudio.Models

open System

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models

type Copyright = | Copyright of DisplayText

type Description = | Description of DisplayText

type IntroUri = | IntroUri of Uri

type PresentationParts =
    | Pages of string list
    | Playlist of (Title * Uri) list

type RoleCredit =
    {
        role: DisplayText
        name: DisplayText
    }
