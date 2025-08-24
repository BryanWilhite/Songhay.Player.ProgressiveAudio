namespace Songhay.Player.ProgressiveAudio

open System
open Bolero
open Microsoft.JSInterop

open FsToolkit.ErrorHandling

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models
open Songhay.Modules.Bolero.JsRuntimeUtility
open Songhay.Modules.Bolero.SvgUtility

/// <summary>
/// Shared utilities for Progressive Audio Presentations
/// </summary>
module ProgressiveAudioPresentationUtility =

    /// <summary>
    /// Gets the conventional custom CSS properties for a <c>800×600</c> <see cref="Presentation"/>.
    /// </summary>
    /// <param name="bgImgUri">the Presentation background image URI</param>
    let getConventionalCssProperties (bgImgUri: Uri) =
        let cssUrlForBgImg = $"url({bgImgUri.OriginalString})"
        let cssUrlForButtonBgSvg = $"url({rxAkyinkyinSvgDataUrl})"

        [
            CssCustomPropertyAndValue (CssCustomProperty.fromInput "rx-player-width", CssValue "800px")
            CssCustomPropertyAndValue (CssCustomProperty.fromInput "rx-player-height", CssValue "600px")
            CssCustomPropertyAndValue (CssCustomProperty.fromInput "rx-player-background-image", CssValue cssUrlForBgImg)
            CssCustomPropertyAndValue (CssCustomProperty.fromInput "rx-player-credits-button-background-image", CssValue cssUrlForButtonBgSvg)
        ]

    /// <summary>
    /// Gets the time display text
    /// from the specified <see cref="decimal"/> in seconds.
    /// </summary>
    /// <param name="secs">the specified <see cref="decimal"/> in seconds</param>
    let getTimeDisplayText secs =
        let minutes = Math.Floor(secs / 60m)
        let seconds = Math.Floor(secs % 60m)

        $"{minutes:``00``}:{seconds:``00``}"

    /// <summary>
    /// Maps the specified <see cref="Presentation"/> data
    /// for the current browser.
    /// </summary>
    /// <param name="jsRuntime">the <see cref="IJSRuntime"/></param>
    /// <param name="sectionElementRef">the <see cref="HtmlRef"/> targeted for <see cref="setComputedStylePropertyValueAsync"/></param>
    /// <param name="bgImgUriOption">the Presentation background image URI</param>
    /// <param name="playListChooser">maps the relative playlist paths to absolute paths</param>
    /// <param name="data"><see cref="Presentation"/> data pair</param>
    let toPresentationOption
        (jsRuntime: IJSRuntime)
        (sectionElementRef: HtmlRef option)
        (bgImgUriOption: Uri option)
        (playListChooser: DisplayText * Uri -> (DisplayText * Uri) option)
        (data: Identifier * Presentation option) =

        option {
            let! presentation = data |> snd
            let! elementRef = sectionElementRef
            let! bgImgUri = bgImgUriOption

            getConventionalCssProperties(bgImgUri) @
            presentation.cssCustomPropertiesAndValues
            |> List.iter
                    (
                        fun vv ->
                            let n, v = vv.Pair

                            jsRuntime
                                |> setComputedStylePropertyValueAsync elementRef n.Value v.Value
                                |> ignore
                    )

            let parts =
                presentation.parts
                |> List.choose(fun part ->
                    match part with
                    | Playlist list -> list |> List.choose playListChooser |> Playlist |> Some
                    | _ -> Some part
                    )

            return { presentation with parts = parts }
        }
