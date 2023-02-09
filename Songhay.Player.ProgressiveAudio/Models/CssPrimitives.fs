namespace Songhay.Player.ProgressiveAudio.Models

open Songhay.Modules.Publications.Models

type CssValue = | CssValue of string

type CssVariable = Name * CssValue
