namespace Songhay.Player.ProgressiveAudio.Components

open Microsoft.AspNetCore.Components

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass
open Songhay.Modules.Models

/// <summary>
/// Defines the player Title <see cref="Component"/>.
/// </summary>
type PlayerTitleComponent() =
    inherit Component()

    /// <summary>
    /// The conventional static member that returns
    /// this instance with <see cref="comp"/>
    /// </summary>
    /// <param name="title">the Title <see cref="Parameter"/></param>
    static member BComp (title: string) =
        comp<PlayerTitleComponent> {
            "Title" => title
        }

     /// <summary>
     /// The Title <see cref="Parameter"/>
     /// </summary>
     [<Parameter>]
     member val Title = "[Title]" with get, set

    /// <summary>
    /// Overrides <see cref="Component.Render"/>
    /// </summary>
    override this.Render() =
        h2 {
            [
                "title"
                elementFontWeight Medium
                fontSize Size5
                p (T, L4)
                p (L, L4)
            ] |> CssClasses.toHtmlClassFromList

            rawHtml this.Title
        }
