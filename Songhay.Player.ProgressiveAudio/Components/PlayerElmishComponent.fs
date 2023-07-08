namespace Songhay.Player.ProgressiveAudio.Components

open Bolero
open Bolero.Html

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass
open Songhay.Modules.Bolero.Visuals.Bulma.Component
open Songhay.Modules.Bolero.Visuals.Bulma.Layout

open Songhay.Player.ProgressiveAudio.Models

type PlayerElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()

    let spinnerContainer =
        bulmaContainer
            ContainerWidthFluid
            (HasClasses (CssClasses [p (All, L6); elementTextAlign AlignCentered]))
                (bulmaLoader (HasClasses <| CssClasses (imageContainer (Square Square128) @ [p (All, L3)])))

    let sectionNode model dispatch =
        section {
            [ "player"; "progressive-audio" ] |> CssClasses.toHtmlClassFromList

            PlayerTitleComponent.BComp <|
                match model.presentation.IsSome with
                    | true ->
                        let (Title t) = model.presentation.Value.title
                        t
                    | _ -> "[ Loading… ]"

            PlayerProseComponent.BComp <|
                match model.presentation.IsSome with
                    | true ->
                        let desc =
                            model.presentation.Value.parts
                            |> List.choose (function | PresentationPart.PresentationDescription s -> Some s | _ -> None)
                            |> List.head
                        desc
                    | _ -> "[ Loading… ]"

            (model, dispatch) ||> PlayerCreditsElmishComponent.EComp

            bulmaModalContainer
                NoCssClasses
                model.presentation.IsNone
                (concat {
                    bulmaModalBackground NoAttr
                    bulmaModalContent NoCssClasses spinnerContainer
                })

        }

    static member EComp model dispatch =
        ecomp<PlayerElmishComponent, _, _> model dispatch { attr.empty() }

    override this.View model dispatch =
        (model, dispatch) ||> sectionNode
