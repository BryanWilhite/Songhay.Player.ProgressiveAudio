namespace Songhay.Player.ProgressiveAudio.Components

open Bolero
open Bolero.Html

open Microsoft.AspNetCore.Components
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
    let sectionNode elmishComponentHtmlRefPolicy model dispatch =
        section {
            [ "player"; "progressive-audio" ] |> CssClasses.toHtmlClassFromList

            $"data-{CanPlay.CssValue}" => if model.presentationStates.hasState CanPlay then "yes" else "no"
            $"data-{CreditsModalVisible.CssValue}" => if model.presentationStates.hasState CreditsModalVisible then "yes" else "no"
            $"data-{LoadingAfterPlaylistIsClicked.CssValue}" => if model.presentationStates.hasState LoadingAfterPlaylistIsClicked then "yes" else "no"
            $"data-{Playing.CssValue}" => if model.presentationStates.hasState Playing then "yes" else "no"
            $"data-{SeekingAfterSliderDrag.CssValue}" => if model.presentationStates.hasState SeekingAfterSliderDrag then "yes" else "no"

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

            (model, dispatch) ||> PlayerControlsElmishComponent.EComp elmishComponentHtmlRefPolicy

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
    /// <param name="elmishComponentHtmlRefPolicy">the Elmish <see cref="HtmlRef"/> <c>dispatch</c> policy</param>
    /// <param name="model">the Elmish model of this domain</param>
    /// <param name="dispatch">the Elmish message dispatcher</param>
    static member EComp (elmishComponentHtmlRefPolicy: ElmishComponentHtmlRefPolicy) model dispatch =
        ecomp<PlayerElmishComponent, _, _> model dispatch
            {
                "ElmishComponentHtmlRefPolicy" => elmishComponentHtmlRefPolicy
            }

    /// <summary>
    /// The conventional Elmish <see cref="HtmlRef"/> <c>dispatch</c> policy
    /// </summary>
     [<Parameter>]
     member val ElmishComponentHtmlRefPolicy = DispatchConditionally with get, set

    /// <summary>
    /// Overrides <see cref="ElmishComponent.View"/>
    /// </summary>
    /// <param name="model">the Elmish model of this domain</param>
    /// <param name="dispatch">the Elmish message dispatcher</param>
    override this.View model dispatch =
        (
            dispatch,
            GotPlayerSection sectionElementRef,
            model.blazorServices.sectionElementRef.IsNone
        )
        |||> this.ElmishComponentHtmlRefPolicy.Evaluate

        (model, dispatch) ||> sectionNode this.ElmishComponentHtmlRefPolicy
