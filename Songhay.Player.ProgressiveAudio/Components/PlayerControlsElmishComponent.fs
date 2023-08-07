namespace Songhay.Player.ProgressiveAudio.Components

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.JsRuntimeUtility
open Songhay.Modules.Bolero.SvgUtility
open Songhay.Modules.Bolero.Visuals.BodyElement
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass

open Songhay.Modules.Models
open Songhay.Player.ProgressiveAudio.Models

type PlayerControlsElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()

    let playPauseBlock model dispatch =
        div {
            [ "controls"; elementIsFlex; AlignCentered.CssClass ] |> CssClasses.toHtmlClassFromList
            attr.id "play-pause-block"
            buttonElement
                NoCssClasses
                (fun _ -> dispatch PlayPauseControlClick)
                (svg {
                    "xmlns" => SvgUri
                    "fill" => "currentColor"
                    "preserveAspectRatio" => "xMidYMid meet"
                    "viewBox" => "-45 -47 180 190"
                    cond model.isPlaying <| function
                        | true -> ProgressiveAudioSvgData.Get PAUSE.ToAlphanumeric
                        | false -> ProgressiveAudioSvgData.Get PLAY.ToAlphanumeric
                })

            input {
                m (L, L1) |> CssClasses.toHtmlClass
                attr.id "play-pause-range"
                attr.``type`` "range"
                attr.value 0
            }
            span {
                [ elementIsFlex; elementFlexWrap NoWrap; elementFlexContentAlignment Center; m (L, L1) ] |> CssClasses.toHtmlClassFromList
                output {
                    [ fontSize Size6; elementFlexSelfAlignment Center ] |> CssClasses.toHtmlClassFromList
                    attr.id "play-pause-progress-output"
                    attr.``for`` "play-pause-range"
                    text model.playingProgressDisplay
                }
                span {
                    [ fontSize Size7; elementFlexSelfAlignment Center ] |> CssClasses.toHtmlClassFromList
                    text "/"
                }
                output {
                    [ fontSize Size7; elementFlexSelfAlignment Center ] |> CssClasses.toHtmlClassFromList
                    attr.id "play-pause-duration-output"
                    attr.``for`` "play-pause-range"
                    text model.playingDurationDisplay
                }
            }
        }

    let container model dispatch =

        let uriOption = model.currentPlaylistItem |> Option.map snd

        concat {
            div {
                attr.id "audio-player-container"
                audio {
                    attr.src (if uriOption.IsSome then uriOption.Value else null)
                    attr.preload "metadata"
                }
                (model, dispatch) ||> playPauseBlock
            }
        }

    static member EComp model dispatch =
        ecomp<PlayerControlsElmishComponent, _, _> model dispatch { attr.empty() }

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    override this.View model dispatch =

        if model.blazorServices.playerControlsRef.IsNone then
            dispatch <| ProgressiveAudioMessage.GotPlayerControlsRef this
        else
            ()

        (model, dispatch) ||> container

    [<JSInvokable>]
    member this.animateAsync(uiData: {|
                                       animationStatus: string option
                                       audioCurrentTime: double option
                                       audioDuration: double option
                                       audioReadyState: int option
                                       isAudioPaused: bool option |}) =

        this.Dispatch PlayerAnimationTick

        this.JSRuntime |> consoleInfoAsync [| uiData |]
