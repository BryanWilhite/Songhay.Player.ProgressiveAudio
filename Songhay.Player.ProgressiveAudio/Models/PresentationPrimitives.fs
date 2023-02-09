namespace Songhay.Player.ProgressiveAudio.Models

open System

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models

type Copyright = | Copyright of DisplayText

type Description = | Description of DisplayText

type Playlist = | Playlist of (Title * Uri) list

type RoleCredit =
    {
        role: DisplayText
        name: DisplayText
    }

type IntroUri = | IntroUri of Uri
