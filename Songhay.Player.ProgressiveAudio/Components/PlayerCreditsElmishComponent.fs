namespace Songhay.Player.ProgressiveAudio.Components

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models

open Songhay.Player.ProgressiveAudio.Models

type PlayerCreditsElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()

    let modalNode (_: IJSRuntime) (model: ProgressiveAudioModel) dispatch =
        div {
            [
                "modal"
                if model.isCreditsModalVisible then "is-active"
            ] |> CssClasses.toHtmlClassFromList

            div {
                [ "modal-background" ] |> CssClasses.toHtmlClassFromList

                on.click <| fun _ -> ProgressiveAudioMessage.PlayerCreditsClick |> dispatch

            }
            div {
                [ "modal-content" ] |> CssClasses.toHtmlClassFromList
                article {
                    [ "message"; "is-success" ] |> CssClasses.toHtmlClassFromList

                    div {
                        [ "message-header" ] |> CssClasses.toHtmlClassFromList
                        p { text "Credits" }

                        button {
                            [ "delete" ] |> CssClasses.toHtmlClassFromList
                            attr.aria "label" "delete"

                            on.click <| fun _ -> ProgressiveAudioMessage.PlayerCreditsClick |> dispatch
                        }
                    }
                    div {
                        [ "message-body" ] |> CssClasses.toHtmlClassFromList
                        text "Hello world!"
                    }
                }
            }
        }

    let buttonNode (jsRuntime: IJSRuntime) (model: ProgressiveAudioModel) dispatch =
        concat {
            button {
                [ "credits" ] |> CssClasses.toHtmlClassFromList

                on.click <| fun _ -> ProgressiveAudioMessage.PlayerCreditsClick |> dispatch

                span {
                    empty()
                }
            }

            (model, dispatch) ||> modalNode jsRuntime
        }

    static member EComp model dispatch =
        ecomp<PlayerCreditsElmishComponent, _, _> model dispatch { attr.empty() }

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    override this.View model dispatch =
        (model, dispatch) ||> buttonNode this.JSRuntime
