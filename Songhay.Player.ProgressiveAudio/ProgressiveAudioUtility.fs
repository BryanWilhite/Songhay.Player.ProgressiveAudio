namespace Songhay.Player.ProgressiveAudio

open System

open FsToolkit.ErrorHandling

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models

open Songhay.Player.ProgressiveAudio.ProgressiveAudioScalars

module ProgressiveAudioUtility =

    let buildAudioRootUri (relativeUri: Uri) =
        if relativeUri.IsAbsoluteUri then relativeUri
        else
            let builder = UriBuilder(rxProgressiveAudioRoot)
            builder.Path <- $"{builder.Path}{relativeUri.OriginalString.TrimStart([|'.';'/'|])}"
            builder.Uri

    let getTimeDisplayText secs =
        let minutes = Math.Floor(secs / 60m)
        let seconds = Math.Floor(secs % 60m)

        $"{minutes:``00``}:{seconds:``00``}"

    let toPresentationOption
        (data: Presentation option)
        (playListMapper: DisplayText * Uri -> DisplayText * Uri) =

        option {
            let! presentation = data

            let map list =
                list
                |> List.map playListMapper
                |> Playlist

            let parts =
                presentation.parts
                |> List.map(fun part -> match part with | Playlist list -> list |> map | _ -> part)

            return { presentation with parts = parts }
        }
