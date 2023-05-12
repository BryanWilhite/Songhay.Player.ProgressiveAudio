namespace Songhay.Player.ProgressiveAudio.Components

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Visuals.Bulma
open Songhay.Modules.Models
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

    let sectionNode (_: IJSRuntime) model dispatch =
        section {
            [ "player"; "progressive-audio" ] |> CssClasses.toHtmlClassFromList

            (model, dispatch) ||> PlayerCreditsElmishComponent.EComp

            bulmaModalContainer
                NoCssClasses
                NoAttr
                false
                model.presentation.IsNone
                spinnerContainer
        }

    static member EComp model dispatch =
        ecomp<PlayerElmishComponent, _, _> model dispatch { attr.empty() }

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    override this.View model dispatch =
        (model, dispatch) ||> sectionNode this.JSRuntime
