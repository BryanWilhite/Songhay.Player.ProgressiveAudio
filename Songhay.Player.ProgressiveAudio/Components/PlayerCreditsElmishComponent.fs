namespace Songhay.Player.ProgressiveAudio.Components

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open FsToolkit.ErrorHandling

open Bolero
open Bolero.Html

open Songhay.Modules.Models
open Songhay.Modules.StringUtility
open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass
open Songhay.Modules.Bolero.Visuals.Bulma.Component
open Songhay.Modules.Publications.Models
open Songhay.Player.ProgressiveAudio.Models

type PlayerCreditsElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()

    let modalNode (model: ProgressiveAudioModel) dispatch =
        let creditItemsNode =
            model.presentationCredits
            |> Option.either
                (fun roleCredits ->
                    forEach roleCredits <| fun roleCredit ->
                        li {
                            [
                                (nameof RoleCredit |> toKabobCase).Value
                                elementIsFlex
                                elementFlexDirection Row
                                elementFlexItemsAlignment Start
                                elementFlexJustifyContent SpaceBetween
                            ] |> CssClasses.toHtmlClassFromList
                            span {
                                [ nameof roleCredit.name; fontSize Size3 ] |> CssClasses.toHtmlClassFromList
                                text roleCredit.name
                            }
                            span {
                                [ nameof roleCredit.role ] |> CssClasses.toHtmlClassFromList
                                text roleCredit.role
                            }
                        }
                )
                (fun () -> text "[missing!]")

        let modalNode =
            article {
                [ "message"; "is-success" ] |> CssClasses.toHtmlClassFromList

                div {
                    [ "message-header" ] |> CssClasses.toHtmlClassFromList
                    Html.p { text "Credits" }

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

        bulmaModalContainer
            NoCssClasses
            model.isCreditsModalVisible
            (concat {
                bulmaModalBackground
                    (HasAttr (on.click <| fun _ -> ProgressiveAudioMessage.PlayerCreditsClick |> dispatch))
                bulmaModalContent NoCssClasses modalNode
            })

    let buttonNode model dispatch =
        concat {
            button {
                [ "credits" ] |> CssClasses.toHtmlClassFromList

                on.click <| fun _ -> ProgressiveAudioMessage.PlayerCreditsClick |> dispatch

                span {
                    empty()
                }
            }

            (model, dispatch) ||> modalNode
        }

    static member EComp model dispatch =
        ecomp<PlayerCreditsElmishComponent, _, _> model dispatch { attr.empty() }

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    override this.View model dispatch =
        (model, dispatch) ||> buttonNode
