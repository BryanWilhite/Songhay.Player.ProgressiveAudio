namespace Songhay.Player.ProgressiveAudio

open System
open Bolero
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Option

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models
open Songhay.Modules.Bolero.JsRuntimeUtility

open Songhay.Player.ProgressiveAudio.ProgressiveAudioScalars

module ProgressiveAudioUtility =

    let internal toUriFragmentOption (location: string) =
        if String.IsNullOrWhiteSpace location then None
        else
            match (location, UriKind.Absolute) |> Uri |> fun uri -> uri.Fragment with
            | s when s.Length > 0 -> Some <| s.TrimStart '#'
            | _ -> None

    let buildAudioRootUri (relativeUri: Uri) =
        if relativeUri.IsAbsoluteUri then relativeUri
        else
            let builder = UriBuilder(rxProgressiveAudioRoot)
            builder.Path <- $"{builder.Path}{relativeUri.OriginalString.TrimStart([|'.';'/'|])}"
            builder.Uri

    let getConventionalCssProperties (presentationKey: string) =
        let bgImgUrl = $"url({rxProgressiveAudioRoot}{presentationKey}/jpg/background.jpg)"
        let buttonImgUrl = $"url({rxAkyinkyinSvgDataUrl})"

        [
            CssVariableAndValue (CssVariable.fromInput "rx-player-width", CssValue "800px")
            CssVariableAndValue (CssVariable.fromInput "rx-player-height", CssValue "600px")
            CssVariableAndValue (CssVariable.fromInput "rx-player-background-image", CssValue bgImgUrl)
            CssVariableAndValue (CssVariable.fromInput "rx-player-credits-button-background-image", CssValue buttonImgUrl)
        ]

    let getPresentationKey (_: IJSRuntime) (navMan: NavigationManager) =

        // jsRuntime |> consoleWarnAsync [| nameof uriFragmentOption ; (navMan.Uri |> uriFragmentOption) |] |> ignore

        navMan.Uri |> toUriFragmentOption

    let getPresentationManifestUri (presentationKey: string ) =
        ($"{rxProgressiveAudioRoot}{presentationKey}/{presentationKey}_presentation.json", UriKind.Absolute) |> Uri

    let getPresentationPlaylistItemUri (presentationKey: string ) (relativePath: string) =
        let segment = relativePath.TrimStart('.', '/')
        ($"{rxProgressiveAudioApiRoot}/api/Player/v1/audio/{presentationKey}/{segment}", UriKind.Absolute) |> Uri

    let getTimeDisplayText secs =
        let minutes = Math.Floor(secs / 60m)
        let seconds = Math.Floor(secs % 60m)

        $"{minutes:``00``}:{seconds:``00``}"

    let mapCssPair (vv: CssVariableAndValue) =
        let cssVariable = vv.Pair |> fst
        let cssValue = vv.Pair |> snd
        let propertyValue =
            match cssVariable.Value with
            | name when name.EndsWith "color" -> cssValue.Value.ToLowerInvariant().Replace("0x", "#")
            | _ -> cssValue.Value

        cssVariable.Value, propertyValue

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
            presentation.cssVariables
            |> List.iter
                    (
                        fun vv ->
                            let name, value = vv |> mapCssPair

                            jsRuntime
                                |> setComputedStylePropertyValueAsync elementRef name value
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
