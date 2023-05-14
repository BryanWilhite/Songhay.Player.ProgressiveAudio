namespace Songhay.Player.ProgressiveAudio.Components

open Microsoft.AspNetCore.Components

open Bolero
open Bolero.Html
open Songhay.Modules.Bolero.Models

type PlayerProseComponent() =
    inherit Component()

    static member BComp (title: string) =
        comp<PlayerProseComponent> {
            "Prose" => title
        }

     [<Parameter>]
     member val Prose = "[Prose]" with get, set
    override this.Render() =
        div {
            [ "prose" ] |> CssClasses.toHtmlClassFromList

            rawHtml this.Prose
        }
