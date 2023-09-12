namespace Songhay.Player.ProgressiveAudio.Components

open Microsoft.AspNetCore.Components

open Bolero
open Bolero.Html
open Songhay.Modules.Bolero.Models

/// <summary>
/// Defines the player Prose <see cref="Component"/>.
/// </summary>
type PlayerProseComponent() =
    inherit Component()

    /// <summary>
    /// The conventional static member that returns
    /// this instance with <see cref="comp"/>
    /// </summary>
    /// <param name="prose">the Prose <see cref="Parameter"/></param>
    static member BComp (prose: string) =
        comp<PlayerProseComponent> {
            "Prose" => prose
        }

     /// <summary>
     /// The Prose <see cref="Parameter"/>
     /// </summary>
     [<Parameter>]
     member val Prose = "[Prose]" with get, set

    /// <summary>
    /// Overrides <see cref="Component.Render"/>
    /// </summary>
    override this.Render() =
        div {
            [ "prose" ] |> CssClasses.toHtmlClassFromList

            rawHtml this.Prose
        }
