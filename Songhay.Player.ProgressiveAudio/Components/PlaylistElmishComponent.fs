namespace Songhay.Player.ProgressiveAudio.Components

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass
open Songhay.Player.ProgressiveAudio.Models

/// <summary>
/// Defines the player Playlist <see cref="ElmishComponent{TModel,TMessage}"/>.
/// </summary>
type PlaylistElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()

    /// <summary>the <c>ol#playlist</c> element</summary>
    let container (model: ProgressiveAudioModel) dispatch =
        ol {
            [ panel; "track-list" ] |> CssClasses.toHtmlClassFromList
            attr.id "playlist"
            cond model.presentationPlayList.IsSome <| function
                | true ->
                    forEach model.presentationPlayList.Value <| fun (txt, uri) ->
                        li {
                            [
                                "panel-block"
                                if model.currentPlaylistItem = Some (txt, uri) then "is-active"
                                else ()
                            ] |> CssClasses.toHtmlClassFromList
                            span {
                                [ "panel-icon"; fontSize Size5 ] |> CssClasses.toHtmlClassFromList
                                text "⬤"
                            }
                            a {
                                [
                                    fontSize Size7
                                    if not model.canPlay then "anchor-disabled"
                                ] |> CssClasses.toHtmlClassFromList

                                on.click (fun _ -> dispatch <| PlaylistClick (txt, uri))
                                DomElementEvent.Click.PreventDefault

                                attr.href "#"

                                text txt.Value
                            }
                        }
                | false ->
                    li { text "[ Loading… ]" }
        }

    /// <summary>
    /// The conventional static member that returns
    /// this instance with <see cref="ecomp"/>
    /// </summary>
    /// <param name="model">the Elmish model of this domain</param>
    /// <param name="dispatch">the Elmish message dispatcher</param>
    static member EComp model dispatch =
        ecomp<PlaylistElmishComponent, _, _> model dispatch { attr.empty() }

    /// <summary>
    /// Overrides <see cref="ElmishComponent.View"/>
    /// </summary>
    /// <param name="model">the Elmish model of this domain</param>
    /// <param name="dispatch">the Elmish message dispatcher</param>
    override this.View model dispatch =
        (model, dispatch) ||> container
