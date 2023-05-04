namespace Songhay.Player.ProgressiveAudio.Components

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma
open Songhay.Player.ProgressiveAudio.Models

type PlayerCreditsElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()

    let buttonNode (_: IJSRuntime) model dispatch =
        button {
            [ "credits" ] |> CssClasses.toHtmlClassFromList

            span {
                empty()
            }
        }

    static member EComp model dispatch =
        ecomp<PlayerCreditsElmishComponent, _, _> model dispatch { attr.empty() }

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    override this.View model dispatch =
        (model, dispatch) ||> buttonNode this.JSRuntime
