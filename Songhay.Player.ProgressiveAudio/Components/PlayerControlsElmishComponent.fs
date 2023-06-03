namespace Songhay.Player.ProgressiveAudio.Components

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Bolero
open Bolero.Html

open Songhay.Player.ProgressiveAudio.Models

type PlayerControlsElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()

    let playPauseBlock model dispatch =
        div {
            attr.id "play-pause-block"
        }

    let container model dispatch =

        let uriOption = model.currentPlaylistItem |> Option.map (fun pair -> pair |> snd)

        concat {
            ProgressiveAudioSvgData.SvgBlock
            div {
                attr.id "audio-player-container"
                cond uriOption.IsSome <| function
                    | true -> audio { attr.src uriOption.Value; attr.preload "metadata" }
                    | false -> empty()
                (model, dispatch) ||> playPauseBlock
            }
        }

    static member EComp model dispatch =
        ecomp<PlayerControlsElmishComponent, _, _> model dispatch { attr.empty() }

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    override this.View model dispatch =
        (model, dispatch) ||> container
