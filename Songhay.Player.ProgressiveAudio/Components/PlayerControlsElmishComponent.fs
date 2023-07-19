namespace Songhay.Player.ProgressiveAudio.Components

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Bolero
open Bolero.Html

open Songhay.Modules.Models
open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass

open Songhay.Player.ProgressiveAudio.Models

type PlayerControlsElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()

    let playPauseBlock model dispatch =
        div {
            [ elementIsFlex; AlignCentered.CssClass; CssClass.p (L, L2) ] |> CssClasses.toHtmlClassFromList
            attr.id "play-pause-block"
            button {
                svg {
                    attr.width 16
                    attr.height 16
                    cond model.isPlaying <| function
                        | true -> ProgressiveAudioSvgData.Get PLAY.ToAlphanumeric
                        | false -> ProgressiveAudioSvgData.Get PAUSE.ToAlphanumeric
                }
            }
            input {
                attr.id "play-pause-range"
                attr.``type`` "range"
                attr.value 0
            }
        }

    let container model dispatch =

        let uriOption = model.currentPlaylistItem |> Option.map (fun pair -> pair |> snd)

        concat {
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
