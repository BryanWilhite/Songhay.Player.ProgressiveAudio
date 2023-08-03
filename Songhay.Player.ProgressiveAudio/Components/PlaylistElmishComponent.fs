namespace Songhay.Player.ProgressiveAudio.Components

open Bolero
open Bolero.Html

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.JsRuntimeUtility
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass
open Songhay.Player.ProgressiveAudio.Models

type PlaylistElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()

    let container (model: ProgressiveAudioModel) dispatch =
        ol {
            [ "panel"; "track-list" ] |> CssClasses.toHtmlClassFromList
            attr.id "playlist"
            cond model.presentationPlayList.IsSome <| function
                | true ->
                    forEach model.presentationPlayList.Value <| fun (txt, uri) ->
                        li {
                            "panel-block" |> CssClasses.toHtmlClass
                            span {
                                [ "panel-icon"; fontSize Size3 ] |> CssClasses.toHtmlClassFromList
                                text "⬤"
                            }
                            a {
                                attr.href "#"
                                on.async.click (
                                    fun _ ->
                                        let qualifiedName = $"{rx}.ProgressiveAudioUtility.loadAudioTrack"
                                        model.blazorServices
                                            .jsRuntime.InvokeVoidAsync(qualifiedName, (uri |> ProgressiveAudioModel.buildAudioRootUri).AbsoluteUri)
                                            .AsTask()
                                        |> Async.AwaitTask

                                )
                                DomElementEvent.Click.PreventDefault
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
