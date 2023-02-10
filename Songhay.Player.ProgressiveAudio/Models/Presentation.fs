namespace Songhay.Player.ProgressiveAudio.Models

open Songhay.Modules.Publications.Models

type Presentation =
    {
        id: Id
        title: Title
        copyright: Copyright
        credits: RoleCredit list
        description: Description
        cssVariables: CssVariable list
        parts: PresentationParts
    }
