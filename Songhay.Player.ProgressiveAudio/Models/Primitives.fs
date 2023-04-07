namespace Songhay.Modules

module JsonElementValueUtility =

    open System
    open System.Text.Json
    open Songhay.Modules.JsonDocumentUtility

    let private getEx (typeName: string) = Error <| JsonException $"The expected {typeName} value is not here."

    let tryGetJsonBooleanValue result =
        result |> toResultFromBooleanElement (fun el -> el.GetBoolean())

    let tryGetJsonBooleanValueFromStringElement result =
        result
        |> toResultFromStringElement (fun el -> el.GetString())
        |> Result.bind (fun s ->
                match s |> Boolean.TryParse with
                | true, b -> b |> Ok
                | false, _ -> nameof(Boolean) |> getEx
            )

    let tryGetJsonDateTimeValue result =
        result
        |> toResultFromStringElement (fun el -> el.GetString())
        |> Result.bind (fun s ->
            match s |> DateTime.TryParse with
            | true, dt -> Ok dt
            | false, _ -> nameof(DateTime) |> getEx
        )

    let tryGetJsonIntValue result =
        result |> toResultFromNumericElement (fun el -> el.GetInt32())

    let tryGetJsonIntValueFromStringElement result =
        result
        |> toResultFromStringElement (fun el -> el.GetString())
        |> Result.bind (fun s ->
                match s |> Int32.TryParse with
                | true, d -> d |> Ok
                | false, _ -> nameof(Int32) |> getEx
            )

    let tryGetJsonFloatValue result =
        result |> toResultFromNumericElement (fun el -> el.GetDouble())

    let tryGetJsonFloatValueFromStringElement result =
        result
        |> toResultFromStringElement (fun el -> el.GetString())
        |> Result.bind (fun s ->
                match s |> Double.TryParse with
                | true, d -> d |> Ok
                | false, _ -> nameof(Double) |> getEx
            )
    let tryGetJsonStringValue result =
        result |> toResultFromStringElement (fun el -> el.GetString())

    let tryGetJsonUriValue (uriKind: UriKind) result =
        result |> toResultFromStringElement (fun el -> el.GetString())
        |> Result.bind (fun s ->
                match Uri.TryCreate(s, uriKind) with
                | true, uri -> Ok <| uri
                | false, _ -> nameof(Uri) |> getEx
            )
