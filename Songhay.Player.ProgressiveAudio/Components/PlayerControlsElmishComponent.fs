namespace Songhay.Player.ProgressiveAudio.Components

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.SvgUtility
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass

open Songhay.Player.ProgressiveAudio.Models

type PlayerControlsElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()

    let playPauseBlock model dispatch =
        div {
            [ "controls"; elementIsFlex; AlignCentered.CssClass ] |> CssClasses.toHtmlClassFromList
            attr.id "play-pause-block"
            button {
                svg {
                    "xmlns" => SvgUri
                    "fill" => "currentColor"
                    "preserveAspectRatio" => "xMidYMid meet"
                    "viewBox" => "-45 -47 180 190"
                    cond model.isPlaying <| function
                        | true -> ProgressiveAudioSvgData.Get PAUSE.ToAlphanumeric
                        | false -> ProgressiveAudioSvgData.Get PLAY.ToAlphanumeric
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
