namespace Songhay.Player.ProgressiveAudio.Models

open Songhay.Modules.Publications.Models

type Presentation =
    {
        id: Id
        title: Title
        cssVariables: CssVariable list
        parts: PresentationParts
    }
