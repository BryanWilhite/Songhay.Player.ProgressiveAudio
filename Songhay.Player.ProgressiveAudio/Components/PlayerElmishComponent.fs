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

/// <summary>
/// Defines the top-level player <see cref="ElmishComponent{TModel,TMessage}"/>.
/// </summary>
type PlayerElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()

    /// <summary><see cref="HtmlRef"/> for the <c>section</c> element in the <see cref="sectionNode"/></summary>
    let sectionElementRef = HtmlRef()

    /// <summary>
    /// a <see cref="bulmaContainer"/> with <see cref="bulmaLoader"/>
    /// </summary>
    let spinnerContainer =
        bulmaContainer
            ContainerWidthFluid
            (HasClasses (CssClasses [p (All, L6); elementTextAlign AlignCentered]))
                (bulmaLoader (HasClasses <| CssClasses (imageContainer (Square Square128) @ [p (All, L3)])))

    /// <summary>the <c>section.player</c> element</summary>
    let sectionNode model dispatch =
        section {
            [ "player"; "progressive-audio" ] |> CssClasses.toHtmlClassFromList
            attr.ref sectionElementRef

            PlayerTitleComponent.BComp <|
                match model.presentation.IsSome with
                    | true ->
                        let (Title t) = model.presentation.Value.title
                        t
                    | _ -> "[ Loading… ]"

            PlayerProseComponent.BComp <|
                match model.presentationDescription.IsSome with
                    | true -> model.presentationDescription.Value
                    | _ -> "[ Loading… ]"

            (model, dispatch) ||> PlaylistElmishComponent.EComp

            (model, dispatch) ||> PlayerControlsElmishComponent.EComp

            (model, dispatch) ||> PlayerCreditsElmishComponent.EComp

            bulmaModalContainer
                NoCssClasses
                model.presentation.IsNone
                (concat {
                    bulmaModalBackground NoAttr
                    bulmaModalContent NoCssClasses spinnerContainer
                })

        }

    /// <summary>
    /// The conventional static member that returns
    /// this instance with <see cref="ecomp"/>
    /// </summary>
    /// <param name="model">the Elmish model of this domain</param>
    /// <param name="dispatch">the Elmish message dispatcher</param>
    static member EComp model dispatch =
        ecomp<PlayerElmishComponent, _, _> model dispatch { attr.empty() }

    /// <summary>
    /// Overrides <see cref="ElmishComponent.View"/>
    /// </summary>
    /// <param name="model">the Elmish model of this domain</param>
    /// <param name="dispatch">the Elmish message dispatcher</param>
    override this.View model dispatch =
        if model.blazorServices.sectionElementRef.IsNone then
            dispatch <| GotPlayerSection sectionElementRef
        else
            ()

        (model, dispatch) ||> sectionNode
