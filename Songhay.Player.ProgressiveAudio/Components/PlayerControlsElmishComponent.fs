namespace Songhay.Player.ProgressiveAudio.Components

open System.Threading.Tasks
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.SvgUtility
open Songhay.Modules.Bolero.Visuals.BodyElement
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass

open Songhay.Modules.Models
open Songhay.Player.ProgressiveAudio.Models

type PlayerControlsElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()

    let audioElementRef = HtmlRef()

    let inputRangeElementRef = HtmlRef()

    let playPauseBlock model dispatch =
        div {
            [ "controls"; elementIsFlex; AlignCentered.CssClass ] |> CssClasses.toHtmlClassFromList
            attr.id "play-pause-block"
            buttonElement
                NoCssClasses
                (fun _ -> dispatch PlayPauseButtonClickEvent)
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
                on.change (fun _ -> dispatch <| PlayPauseChangeEvent inputRangeElementRef)
                on.input (fun _ -> dispatch PlayPauseInputEvent)
                attr.id "play-pause-range"
                attr.``type`` "range"
                attr.max model.playingDuration
                attr.value $"{model.playingCurrentTime}"
                attr.ref inputRangeElementRef
            }
            span {
                [ elementIsFlex; elementFlexWrap NoWrap; elementFlexContentAlignment Center; m (L, L1) ] |> CssClasses.toHtmlClassFromList
                output {
                    [ fontSize Size6; elementFlexSelfAlignment Center ] |> CssClasses.toHtmlClassFromList
                    attr.id "play-pause-progress-output"
                    attr.``for`` "play-pause-range"
                    text model.playingCurrentTimeDisplay
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
                    on.loadedmetadata (fun _ -> dispatch PlayAudioMetadataLoadedEvent)
                    on.ended (fun _ -> dispatch PlayAudioEndedEvent)
                    attr.src (if uriOption.IsSome then uriOption.Value else null)
                    attr.preload "metadata"
                    attr.ref audioElementRef
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
            dispatch <| ProgressiveAudioMessage.GotPlayerControlsRef {|
                                                                       audioElementRef = audioElementRef
                                                                       playerControlsRef = this
                                                                    |}
        else
            ()

        (model, dispatch) ||> container

    [<JSInvokable>]
    member this.animateAsync(uiData: {|
                                       animationStatus: string option
                                       audioCurrentTime: decimal option
                                       audioDuration: decimal option
                                       audioReadyState: int option
                                       isAudioPaused: bool option |}) =

        let tick =
            PlayerAnimationTick {
                audioCurrentTime = uiData.audioCurrentTime.Value
                audioDuration = uiData.audioDuration.Value
                audioReadyState = uiData.audioReadyState.Value
            }

        let result = this.Dispatch tick

        //this.JSRuntime |> Songhay.Modules.Bolero.JsRuntimeUtility.consoleInfoAsync [| uiData |]
        Task.FromResult result
