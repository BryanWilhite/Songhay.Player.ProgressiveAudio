namespace Songhay.Player.ProgressiveAudio.Components

open Microsoft.AspNetCore.Components

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass
open Songhay.Modules.Models

type PlayerTitleComponent() =
    inherit Component()

    static member BComp (title: string) =
        comp<PlayerTitleComponent> {
            "Title" => title
        }

     [<Parameter>]
     member val Title = "[Title]" with get, set

    override this.Render() =
        h2 {
            [
                "title"
                elementFontWeight Normal
                fontSize Size5
                p (T, L2)
                p (L, L2)
            ] |> CssClasses.toHtmlClassFromList

            rawHtml this.Title
        }
