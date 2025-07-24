namespace Songhay.Player.ProgressiveAudio

open System
open Bolero
open Microsoft.JSInterop

open FsToolkit.ErrorHandling

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models
open Songhay.Modules.Bolero.JsRuntimeUtility

open Songhay.Player.ProgressiveAudio.ProgressiveAudioScalars

/// <summary>
/// Shared utilities for Progressive Audio Presentations
/// </summary>
module ProgressiveAudioPresentationUtility =

    /// <summary>
    /// Builds an absolute <see cref="Uri"/>
    /// from the conventional relative <see cref="Uri"/>.
    /// </summary>
    /// <param name="relativeUri">the conventional relative <see cref="Uri"/></param>
    let buildAudioRootUri (relativeUri: Uri) =
        if relativeUri.IsAbsoluteUri then relativeUri
        else
            let builder = UriBuilder(rxProgressiveAudioRoot)
            builder.Path <- $"{builder.Path}{relativeUri.OriginalString.TrimStart([|'.';'/'|])}"
            builder.Uri

    /// <summary>
    /// Gets the conventional custom CSS properties for a <c>800×600</c> <see cref="Presentation"/>.
    /// </summary>
    /// <param name="presentationKey">the Presentation key</param>
    let getConventionalCssProperties (presentationKey: string) =
        let bgImgUrl = $"url({rxProgressiveAudioRoot}{presentationKey}/jpg/background.jpg)"
        let buttonImgUrl = $"url({rxAkyinkyinSvgDataUrl})"

        [
            CssCustomPropertyAndValue (CssCustomProperty.fromInput "rx-player-width", CssValue "800px")
            CssCustomPropertyAndValue (CssCustomProperty.fromInput "rx-player-height", CssValue "600px")
            CssCustomPropertyAndValue (CssCustomProperty.fromInput "rx-player-background-image", CssValue bgImgUrl)
            CssCustomPropertyAndValue (CssCustomProperty.fromInput "rx-player-credits-button-background-image", CssValue buttonImgUrl)
        ]

    /// <summary>
    /// Gets the conventional presentation manifest <see cref="Uri"/> from the Presentation key.
    /// </summary>
    /// <param name="presentationKey">the Presentation key</param>
    let getPresentationManifestUri (presentationKey: string ) =
        ($"{rxProgressiveAudioRoot}{presentationKey}/{presentationKey}_presentation.json", UriKind.Absolute) |> Uri

    /// <summary>
    /// Gets the playlist item <see cref="Uri"/>
    /// with the specified key and relative path.
    /// </summary>
    /// <param name="presentationKey">the Presentation key</param>
    /// <param name="relativePath">the relative path to the playlist item</param>
    let getPresentationPlaylistItemUri (presentationKey: string ) (relativePath: string) =
        let segment = relativePath.TrimStart('.', '/')
        ($"{rxProgressiveAudioApiRoot}/api/Player/v1/audio/{presentationKey}/{segment}", UriKind.Absolute) |> Uri

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
    /// <param name="playListMapper">maps the relative playlist paths to absolute paths</param>
    /// <param name="data"><see cref="Presentation"/> data pair</param>
    let toPresentationOption
        (jsRuntime: IJSRuntime)
        (sectionElementRef: HtmlRef option)
        (playListMapper: DisplayText * Uri -> DisplayText * Uri)
        (data: Identifier * Presentation option) =

        option {
            let key = data |> fst
            let! presentation = data |> snd
            let! elementRef = sectionElementRef

            getConventionalCssProperties(key.StringValue) @
            presentation.cssCustomPropertiesAndValues
            |> List.iter
                    (
                        fun vv ->
                            let n, v = vv.Pair

                            jsRuntime
                                |> setComputedStylePropertyValueAsync elementRef n.Value v.Value
                                |> ignore
                    )

            let map list =
                list
                |> List.map playListMapper
                |> Playlist

            let parts =
                presentation.parts
                |> List.map(fun part -> match part with | Playlist list -> list |> map | _ -> part)

            return { presentation with parts = parts }
        }
