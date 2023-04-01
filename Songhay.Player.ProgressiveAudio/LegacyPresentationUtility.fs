namespace Songhay.Player.ProgressiveAudio

module LegacyPresentationUtility =

    open System
    open System.Data
    open System.Linq
    open System.Collections.Generic
    open System.Text.Json
    open System.Text.RegularExpressions

    open FsToolkit.ErrorHandling

    open Songhay.Modules.Models
    open Songhay.Modules.Publications.Models
    open Songhay.Modules.JsonDocumentUtility

    open Songhay.Player.ProgressiveAudio.Models

    let toPresentationCopyrights
        (nameElementResult: Result<JsonElement, JsonException>)
        (yearElementResult: Result<JsonElement, JsonException>) =

        let nameResult = nameElementResult |> JsonElementValue.tryGetJsonStringValue
        let yearResult = yearElementResult |> JsonElementValue.tryGetJsonIntValueFromStringElement

        [
            nameResult
            yearResult
        ]
        |> List.sequenceResultM
        |> Result.bind (
            fun _ ->
            let nameOption = (nameResult |> Result.valueOr raise).StringValue
            let yearOption = (yearResult |> Result.valueOr raise).IntValue
            let options = [|
                nameOption.IsSome
                yearOption.IsSome
            |]
            match options |> Array.forall id with
            | true ->
                [
                    {
                        name = nameOption.Value
                        year = yearOption.Value
                    }
                ]
                |> CopyRights |> Ok
            | false -> Error <| JsonException "The expected option values are not here."
        )

    let toPresentationCreditsResult (elementResult: Result<JsonElement, JsonException>) =

        let rx = Regex("<div>([^<>]+)<strong>([^<>]+)<\/strong><\/div>", RegexOptions.Compiled)
        let matchesResult =
            elementResult
            |> toResultFromStringElement (fun el -> el.GetString())
            |> Result.map (fun html -> html |> rx.Matches)
            |> Result.bind (fun matches ->
                if matches.Count > 0 then Ok matches
                else Error <| JsonException "The expected HTML format is not here.")

        let processMatches (creditsMatch: Match) =
            let getRole (group: Group) = Regex.Replace(group.Value, " by[ , ]. . . . . . . ", String.Empty)

            match creditsMatch.Groups |> List.ofSeq with
            | [_; r; n] -> Ok { role = r |> getRole; name = n.Value }
            | _ -> Error <| JsonException ("See inner exception.", DataException $"The expected {nameof(Regex)} group data is not here.")

        matchesResult
        |> Result.bind (
            fun matches ->
                matches
                |> Seq.map processMatches
                |> List.ofSeq
                |> List.sequenceResultM
                |> Result.map (fun l -> l  |> Credits)
            )

    let toPresentationCssVariablesResult (elementResult: Result<JsonElement, JsonException>) =
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

    let toPresentationPlaylistResult (elementResult: Result<JsonElement, JsonException>) =

        let toPlaylistItem el =
            let titleResult = el |> tryGetProperty "#text" |> JsonElementValue.tryGetJsonStringValue
            let uriResult = el |> tryGetProperty "@Uri" |> JsonElementValue.tryGetJsonUriValue UriKind.Relative

            [
                titleResult
                uriResult
            ]
            |> List.sequenceResultM
            |> Result.bind
                (
                    fun _ ->
                        let titleOpt = (titleResult |> Result.valueOr raise).StringValue
                        let uriOpt = (uriResult |> Result.valueOr raise).UriValue
                        let options = [|
                            titleOpt.IsSome
                            uriOpt.IsSome
                        |]
                        match options |> Array.forall id with
                        | true -> Ok (titleOpt.Value |> DisplayText, uriOpt.Value)
                        | false -> Error <| JsonException "The expected option values are not here."
                )

        elementResult
            |> toResultFromJsonElement
                (fun kind -> kind = JsonValueKind.Array)
                (fun el -> el.EnumerateArray().ToArray())
            |> Result.bind (
                    fun a ->
                        a
                        |> List.ofSeq
                        |> List.map toPlaylistItem
                        |> List.sequenceResultM
                        |> Result.map (fun l -> l |> Playlist)
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

    let tryGetPresentation (json: string) =
        let presentationElementResult = json |> tryGetPresentationElementResult

        let idResult =
            presentationElementResult
            |> tryGetPresentationIdResult
            |> toResultFromStringElement (fun el -> el.GetString() |> Identifier.fromString |> Id)
        let titleResult =
            presentationElementResult
            |> tryGetPresentationTitleResult
            |> toResultFromStringElement (fun el -> el.GetString() |> Title)
        let cssVariablesResult =
            presentationElementResult
            |> tryGetLayoutMetadataResult
            |> toPresentationCssVariablesResult
        let descriptionResult =
            presentationElementResult
            |> tryGetPresentationDescriptionResult
            |> toResultFromStringElement (fun el -> el.GetString() |> PresentationDescription)
        let creditsResult =
            presentationElementResult
            |> tryGetPresentationCreditsResult
            |> toPresentationCreditsResult
        let copyRightNameResult = presentationElementResult |> tryGetCopyrightNameResult
        let copyRightYearResult = presentationElementResult |> tryGetCopyrightYearResult
        let copyRightsResult = (copyRightNameResult, copyRightYearResult) ||> toPresentationCopyrights
        let playlistResult =
            presentationElementResult
            |> tryGetPlaylistRootResult
            |> toPresentationPlaylistResult

        [
            idResult |> Result.mapToUnit
            titleResult |> Result.mapToUnit
            cssVariablesResult |> Result.mapToUnit
            descriptionResult |> Result.mapToUnit
            creditsResult |> Result.mapToUnit
            copyRightsResult |> Result.mapToUnit
            playlistResult |> Result.mapToUnit
        ]
        |> List.sequenceResultM
        |> Result.map
            (
                fun _ ->
                    {
                        id = idResult |> Result.valueOr raise
                        title = titleResult |> Result.valueOr raise
                        cssVariables = cssVariablesResult |> Result.valueOr raise
                        parts = [
                            descriptionResult |> Result.valueOr raise
                            creditsResult |> Result.valueOr raise
                            copyRightsResult |> Result.valueOr raise
                            playlistResult |> Result.valueOr raise
                        ]
                    }
            )
