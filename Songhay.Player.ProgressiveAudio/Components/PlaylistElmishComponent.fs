namespace Songhay.Player.ProgressiveAudio.Components

open Bolero
open Bolero.Html

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Publications.Models
open Songhay.Player.ProgressiveAudio.Models

type PlaylistElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()
    let container model dispatch =
        let itemsOption =
            model.presentation |> Option.map (
                    fun p ->
                        p.parts
                        |> List.choose (function | PresentationPart.Playlist pl -> pl |> Some | _ -> None)
                        |> List.head
                )
        ol {
            [ "panel"; "track-list" ] |> CssClasses.toHtmlClassFromList
            attr.id "playlist"
            cond itemsOption.IsSome <| function
                | true ->
                    forEach itemsOption.Value <| fun (txt, uri) ->
                        li {
                            "panel-block" |> CssClasses.toHtmlClass
                            span {
                                [ "panel-icon"; "is-size-3" ] |> CssClasses.toHtmlClassFromList
                                text "⬤"
                            }
                            a {
                                "data-src" => if uri.IsAbsoluteUri then uri.AbsoluteUri else uri.OriginalString
                                text txt.Value
                            }
                        }
                | false ->
                    li { text "[ Loading… ]" }
        }

    static member EComp model dispatch =
        ecomp<PlaylistElmishComponent, _, _> model dispatch { attr.empty() }

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    override this.View model dispatch =
        (model, dispatch) ||> container