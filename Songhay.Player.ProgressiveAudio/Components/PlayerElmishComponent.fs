namespace Songhay.Player.ProgressiveAudio.Components

open Elmish
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Player.ProgressiveAudio.Models

type PlayerElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()

    let sectionNode (_: IJSRuntime) model dispatch =
        section {
            [ "player"; "progressive-audio" ] |> CssClasses.toHtmlClassFromList

            (model, dispatch) ||> PlayerCreditsElmishComponent.EComp
        }

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    static member EComp model dispatch =
        ecomp<PlayerElmishComponent, _, _> model dispatch { attr.empty() }

    static member Update model dispatch =
        match model with
        | _ -> model, Cmd.none

    override this.View model dispatch =
        (model, dispatch) ||> sectionNode this.JSRuntime
