namespace Songhay.Player.ProgressiveAudio.Components

open System.Threading.Tasks
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.SvgUtility
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass

open Songhay.Modules.Models
open Songhay.Player.ProgressiveAudio.Models

/// <summary>
/// Defines the player Controls <see cref="ElmishComponent{TModel,TMessage}"/>.
/// </summary>
type PlayerControlsElmishComponent() =
    inherit ElmishComponent<ProgressiveAudioModel, ProgressiveAudioMessage>()

    /// <summary><see cref="HtmlRef"/> for the <c>audio</c> element in the <see cref="playPauseBlock"/></summary>
    let audioElementRef = HtmlRef()

    /// <summary><see cref="HtmlRef"/> for the <c>input[type="range"]</c> element in the <see cref="playPauseBlock"/></summary>
    let inputRangeElementRef = HtmlRef()

    /// <summary>the <c>div.controls</c> element</summary>
    let playPauseBlock model dispatch =

        div {
            [ "controls"; elementIsFlex; AlignCentered.CssClass ] |> CssClasses.toHtmlClassFromList
            attr.id "play-pause-block"

            button {
                on.click (fun _ -> dispatch PlayerPauseOrPlayButtonClickEvent)
                attr.disabled <| not (model.presentationStates.hasState CanPlay)
                svg {
                    "xmlns" => SvgUri
                    "fill" => "currentColor"
                    "preserveAspectRatio" => "xMidYMid meet"
                    "viewBox" => "-45 -47 180 190"
                    cond (model.presentationStates.hasState Playing) <| function
                        | true -> ProgressiveAudioSvgData.Get PAUSE.ToAlphanumeric
                        | false -> ProgressiveAudioSvgData.Get PLAY.ToAlphanumeric
                }
            }

            input {
                m (L, L1) |> CssClasses.toHtmlClass
                on.change (fun _ -> dispatch <| PlayerInputRangeChangeEvent inputRangeElementRef)
                on.input (fun _ -> dispatch PlayerInputRangeInputEvent)
                attr.disabled <| not (model.presentationStates.hasState CanPlay)
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

    /// <summary>the <c>div#audio-player-container</c> element</summary>
    let container model dispatch =

        let uriOption = model.currentPlaylistItem |> Option.map snd

        div {
            attr.id "audio-player-container"
            audio {
                on.loadedmetadata (fun _ -> dispatch PlayerAudioMetadataLoadedEvent)
                on.loadstart (fun _ -> dispatch PlayerAudioLoadStartEvent)
                on.canplay (fun _ -> dispatch PlayerAudioCanPlayEvent)
                on.ended (fun _ -> dispatch PlayerAudioEndedEvent)
                attr.src (if uriOption.IsSome then uriOption.Value else null)
                attr.preload "metadata"
                attr.ref audioElementRef
            }
            (model, dispatch) ||> playPauseBlock
        }

    /// <summary>
    /// The conventional static member that returns
    /// this instance with <see cref="ecomp"/>
    /// </summary>
    /// <param name="elmishComponentHtmlRefPolicy">the Elmish <see cref="HtmlRef"/> <c>dispatch</c> policy</param>
    /// <param name="model">the Elmish model of this domain</param>
    /// <param name="dispatch">the Elmish message dispatcher</param>
    static member EComp (elmishComponentHtmlRefPolicy: ElmishComponentHtmlRefPolicy) model dispatch =
        ecomp<PlayerControlsElmishComponent, _, _> model dispatch
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
            GotPlayerControlsRefs {| audioElementRef = audioElementRef; playerControlsComp = this |},
            model.blazorServices.audioElementRef.IsNone
        )
        |||> this.ElmishComponentHtmlRefPolicy.Evaluate

        (model, dispatch) ||> container

    /// <summary>
    /// The <see cref="JSInvokable"/> member the browser uses to call this instance.
    /// </summary>
    /// <param name="uiData">maps to <see cref="PlayerAnimationTick"/></param>
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

        Task.FromResult <| this.Dispatch tick
