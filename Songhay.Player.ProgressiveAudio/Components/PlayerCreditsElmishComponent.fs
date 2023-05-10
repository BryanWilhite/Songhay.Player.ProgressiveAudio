namespace Songhay.Player.ProgressiveAudio.Components

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Bolero
open Bolero.Html

open Songhay.Modules.StringUtility
open Songhay.Modules.Bolero.Models

open Songhay.Modules.Publications.Models
open Songhay.Player.ProgressiveAudio.Models

type PlayerCreditsElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()

    let modalNode (_: IJSRuntime) (model: ProgressiveAudioModel) dispatch =
        let creditItemsNode =
            if model.presentation.IsSome then
                let credits =
                    model.presentation.Value.parts
                    |> List.choose (function | PresentationPart.Credits l -> Some l | _ -> None)
                forEach credits <| fun l -> forEach l <| fun c ->
                    li {
                        (nameof RoleCredit |> toKabobCase).Value |> CssClasses.toHtmlClass
                        span {
                            nameof c.name |> CssClasses.toHtmlClass
                            text c.name
                        }
                        span {
                            nameof c.role |> CssClasses.toHtmlClass
                            text c.role
                        }
                    }
            else
                text "[missing!]"

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
                        ul {
                            creditItemsNode
                        }
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
