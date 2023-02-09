namespace Songhay.Player.ProgressiveAudio.Models

open Songhay.Modules.Publications.Models

type Presentation =
    {
        id: Id
        title: Title
        cssVariables: CssVariable list
        description: Description
        credits: RoleCredit list
        copyright: Copyright
        playlist: Playlist option
        introUri: IntroUri option
    }
