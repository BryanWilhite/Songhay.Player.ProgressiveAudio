namespace Songhay.Player.ProgressiveAudio

module LegacyPresentationUtility =

    open System.Text.Json
    open System.Text.RegularExpressions

    open Songhay.Modules.Models
    open Songhay.Modules.JsonDocumentUtility

    open Songhay.Player.ProgressiveAudio.Models

    let tryGetPresentationElementResult (json: string) =
        json
        |> tryGetRootElement
        |> Result.bind (tryGetProperty <| nameof(Presentation))
