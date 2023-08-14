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

    let buildAudioRootUri (relativeUri: Uri) =
        if relativeUri.IsAbsoluteUri then relativeUri
        else
            let builder = UriBuilder(rxProgressiveAudioRoot)
            builder.Path <- $"{builder.Path}{relativeUri.OriginalString.TrimStart([|'.';'/'|])}"
            builder.Uri

    let getPresentationKey (jsRuntime: IJSRuntime) (navMan: NavigationManager) =

        let uriFragmentOption =
            match (navMan.Uri, UriKind.Absolute) |> Uri |> fun uri -> uri.Fragment with
            | s when s.Length > 0 -> Some s
            | _ -> None

        jsRuntime |> consoleWarnAsync [| nameof uriFragmentOption ; uriFragmentOption |] |> ignore

        let getTypeAndKey (s: string) =
            match s.Split('/') with
            | [| _ ; t ; k |] ->
                if t = "audio" then Some k
                else None
            | _ -> None

        uriFragmentOption >>= (fun s -> s |> getTypeAndKey)

    let getPresentationManifestUri (presentationKey: string ) =
        ($"{rxProgressiveAudioApiRoot}/api/Player/v1/audio/{presentationKey}", UriKind.Absolute) |> Uri

    let getPresentationPlaylistItemUri (presentationKey: string ) (relativePath: string) =
        let segment = relativePath.TrimStart('.', '/')
        ($"{rxProgressiveAudioApiRoot}/api/Player/v1/audio/{presentationKey}/{segment}", UriKind.Absolute) |> Uri

    let getTimeDisplayText secs =
        let minutes = Math.Floor(secs / 60m)
        let seconds = Math.Floor(secs % 60m)

        $"{minutes:``00``}:{seconds:``00``}"

    let toPresentationOption
        (jsRuntime: IJSRuntime)
        (sectionElementRef: HtmlRef option)
        (playListMapper: DisplayText * Uri -> DisplayText * Uri)
        (data: Identifier * Presentation option) =

        let setStyle (elementRef: HtmlRef) (vv: CssVariableAndValue) =
            let p = vv.Pair |> fst
            let v = vv.Pair |> snd
            jsRuntime
                |> setComputedStylePropertyValueAsync elementRef p.Value v.Value
                |> ignore

        option {
            let! presentation = data |> snd
            let! elementRef = sectionElementRef

            presentation.cssVariables |> List.iter (fun vv -> vv |> setStyle elementRef)

            let map list =
                list
                |> List.map playListMapper
                |> Playlist

            let parts =
                presentation.parts
                |> List.map(fun part -> match part with | Playlist list -> list |> map | _ -> part)

            return { presentation with parts = parts }
        }
