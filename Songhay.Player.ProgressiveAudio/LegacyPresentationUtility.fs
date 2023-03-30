namespace Songhay.Player.ProgressiveAudio

module LegacyPresentationUtility =

    open System
    open System.Linq
    open System.Collections.Generic
    open System.Text.Json
    open System.Text.RegularExpressions

    open FsToolkit.ErrorHandling

    open Songhay.Modules.Models
    open Songhay.Modules.JsonDocumentUtility
    open Songhay.Modules.Publications.Models

    open Songhay.Player.ProgressiveAudio.Models

    let toPresentationCopyrights (elementResult: Result<JsonElement, JsonException>) =
        elementResult

    let toPresentationCssVariables (elementResult: Result<JsonElement, JsonException>) =
        let declarations = List<CssVariableAndValue>()
        let rec processProperty (prefix: string) (p: JsonProperty) =
            match p.Value.ValueKind with
            | JsonValueKind.Object ->
                p.Value.EnumerateObject().ToArray()
                |> Array.iter (fun el -> ($"{prefix}{p.Name}-", el) ||> processProperty)
                ()
            | JsonValueKind.String ->
                match p.Name with
                | "@title" | "@uri" | "@version" -> ()
                | _ ->
                    let cssVal =
                        match p.Name with
                        | "@x" | "@y" | "@marginBottom" | "@marginTop"
                        | "@width" | "@height" -> $"{p.Value.GetString()}px"
                        | "@opacity" -> $"{p.Value.GetString()}%%"
                        | _ -> p.Value.GetString()
                        |> CssValue
                    let cssVar = $"{prefix}{p.Name.TrimStart('@')}" |> CssVariable.fromInput
                    declarations.Add((cssVar, cssVal) |> CssVariableAndValue)
                    ()
            | _ -> ()

        elementResult
        |> toResultFromJsonElement
            (fun kind -> kind = JsonValueKind.Object)
            (fun el -> el.EnumerateObject().ToArray())
        |> Result.map
            (
                fun jsonProperties ->
                    jsonProperties |> Array.iter (fun el -> ("rx-player-", el) ||> processProperty)
                    declarations |> List.ofSeq
            )

    let tryGetPresentationElementResult (json: string) =
        json
        |> tryGetRootElement
        |> Result.bind (tryGetProperty <| nameof(Presentation))

    let tryGetPresentationIdResult presentationElementResult =
        presentationElementResult
        |> Result.bind (tryGetProperty "@ClientId")

    let tryGetPresentationTitleResult presentationElementResult =
        presentationElementResult
        |> Result.bind (tryGetProperty <| nameof(Title))

    let tryGetPresentationDescriptionResult presentationElementResult =
        presentationElementResult
        |> Result.bind (tryGetProperty <| nameof(Description))
        |> Result.bind (tryGetProperty "#text")

    let tryGetPresentationCreditsResult presentationElementResult =
        presentationElementResult
        |> Result.bind (tryGetProperty <| nameof(Credits))
        |> Result.bind (tryGetProperty "#text")

    let tryGetLayoutMetadataResult presentationElementResult =
        presentationElementResult
        |> Result.bind (tryGetProperty "LayoutMetadata")

    let tryGetCopyrightNameResult presentationElementResult =
            presentationElementResult
            |> Result.bind (tryGetProperty <| nameof(Copyright))
            |> Result.bind (tryGetProperty "@Name")
    let tryGetCopyrightYearResult presentationElementResult =
            presentationElementResult
            |> Result.bind (tryGetProperty <| nameof(Copyright))
            |> Result.bind (tryGetProperty "@Year")

    let tryGetPlaylistRootResult presentationElementResult =
        presentationElementResult
        |> Result.bind (tryGetProperty "ItemGroup")
        |> Result.bind (tryGetProperty "Item")
