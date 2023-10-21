namespace Songhay.StudioFloor.Client.Components

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma.Element
open Songhay.Modules.Bolero.Visuals.Bulma.Layout

open Songhay.Player.ProgressiveAudio.Components

open Songhay.StudioFloor.Client
open Songhay.StudioFloor.Client.Models

type TabsElmishComponent() =
    inherit ElmishComponent<StudioFloorModel, StudioFloorMessage>()

    let getTabs (moreClasses: CssClassesOrEmpty) isActiveGetter (nodePair: (Node * 'page) list) =
            div {
                CssClasses [ "tabs" ] |> moreClasses.ToHtmlClassAttribute

                ul {
                    forEach nodePair <| fun (node, pg) ->
                    li {
                        attr.``class`` (if (isActiveGetter pg) then "is-active" else null)

                        node
                    }
                }
            }

    static member EComp model dispatch =
        ecomp<TabsElmishComponent, _, _> model dispatch { attr.empty() }

    override this.ShouldRender(oldModel, newModel) =
        oldModel.page <> newModel.page
        || oldModel.readMeData <> newModel.readMeData
        || oldModel.paModel <> newModel.paModel

    override this.View model dispatch =
        concat {
            let tabPairs = [
                ( a { ElmishRoutes.router.HRef ReadMePage; text "README" }, ReadMePage )
                ( a { ElmishRoutes.router.HRef <| BRollAudioPage "default"; text "Progressive Audio Player" }, BRollAudioPage "default" )
            ]

            getTabs
                (HasClasses <| CssClasses [ ColorEmpty.BackgroundCssClassLight; "is-toggle"; "is-fullwidth"; SizeLarge.CssClass ])
                (fun pg -> model.page = pg)
                tabPairs

            cond model.page <| function
            | ReadMePage ->
                if model.readMeData.IsNone then
                    text "loading…"
                else
                    bulmaContainer
                        ContainerWidthFluid
                        NoCssClasses
                        (bulmaNotification
                            (HasClasses <| CssClasses [ "is-info" ])
                            (rawHtml model.readMeData.Value))
            | BRollAudioPage _ ->
                bulmaContainer
                    ContainerWidthFluid
                    NoCssClasses
                    (PlayerElmishComponent.EComp model.paModel (ProgressiveAudioMessage >> dispatch))
        }
