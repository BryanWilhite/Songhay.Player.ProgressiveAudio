namespace Songhay.StudioFloor.Client.Components

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma.Component
open Songhay.Modules.Bolero.Visuals.Bulma.Element
open Songhay.Modules.Bolero.Visuals.Bulma.Layout

open Songhay.Player.ProgressiveAudio.Components
open Songhay.StudioFloor.Client.Models

type TabsElmishComponent() =
    inherit ElmishComponent<StudioFloorModel, StudioFloorMessage>()

    static member EComp model dispatch =
        ecomp<TabsElmishComponent, _, _> model dispatch { attr.empty() }

    override this.ShouldRender(oldModel, newModel) =
        oldModel.tab <> newModel.tab
        || oldModel.readMeData <> newModel.readMeData
        || oldModel.paModel.error <> newModel.paModel.error
        || oldModel.paModel.isPlaying <> newModel.paModel.isPlaying
        || oldModel.paModel.playingDuration <> newModel.paModel.playingDuration
        || oldModel.paModel.playingProgress <> newModel.paModel.playingProgress
        || oldModel.paModel.presentation <> newModel.paModel.presentation

    override this.View model dispatch =
        concat {
            let tabs = [
                (text "README", ReadMeTab)
                (text "Progressive Audio Player", PlayerTab)
            ]

            bulmaTabs
                (HasClasses <| CssClasses [ ColorEmpty.BackgroundCssClassLight; "is-toggle"; "is-fullwidth"; SizeLarge.CssClass ])
                (fun pg -> model.tab = pg)
                (fun pg _ -> SetTab pg |> dispatch)
                tabs

            cond model.tab <| function
            | ReadMeTab ->
                if model.readMeData.IsNone then
                    text "loading…"
                else
                    bulmaContainer
                        ContainerWidthFluid
                        NoCssClasses
                        (bulmaNotification
                            (HasClasses <| CssClasses [ "is-info" ])
                            (rawHtml model.readMeData.Value))
            | PlayerTab ->
                bulmaContainer
                    ContainerWidthFluid
                    NoCssClasses
                    (PlayerElmishComponent.EComp model.paModel (ProgressiveAudioMessage >> dispatch))
        }
