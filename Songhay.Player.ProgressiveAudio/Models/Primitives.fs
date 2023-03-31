namespace Songhay.Modules.Models

open System
open System.Text.Json
open Songhay.Modules.JsonDocumentUtility

type JsonElementValue =
    | JsonBooleanValue of bool
    | JsonDateTimeValue of DateTime
    | JsonIntValue of int
    | JsonFloatValue of double
    | JsonStringValue of string
    | JsonUriValue of Uri

     static member private getEx (typeName: string) = Error <| JsonException $"The expected {typeName} value is not here."

    static member tryGetJsonBooleanValue result =
        result |> toResultFromBooleanElement (fun el -> el.GetBoolean() |> JsonBooleanValue)

    static member tryGetJsonBooleanValueFromStringElement result =
        result
        |> toResultFromStringElement (fun el -> el.GetString())
        |> Result.bind (fun s ->
                match s |> Boolean.TryParse with
                | true, b -> b |> JsonBooleanValue |> Ok
                | false, _ -> nameof(Boolean) |> JsonElementValue.getEx
            )

    static member tryGetJsonDateTimeValue result =
        result
        |> toResultFromStringElement (fun el -> el.GetString())
        |> Result.bind (fun s ->
            match s |> DateTime.TryParse with
            | true, dt -> Ok <| JsonDateTimeValue dt
            | false, _ -> nameof(DateTime) |> JsonElementValue.getEx
        )

    static member tryGetJsonIntValue result =
        result |> toResultFromNumericElement (fun el -> el.GetInt32() |> JsonIntValue)

    static member tryGetJsonIntValueFromStringElement result =
        result
        |> toResultFromStringElement (fun el -> el.GetString())
        |> Result.bind (fun s ->
                match s |> Int32.TryParse with
                | true, d -> d |> JsonIntValue |> Ok
                | false, _ -> nameof(Int32) |> JsonElementValue.getEx
            )

    static member tryGetJsonFloatValue result =
        result |> toResultFromNumericElement (fun el -> el.GetDouble() |> JsonFloatValue)

    static member tryGetJsonFloatValueFromStringElement result =
        result
        |> toResultFromStringElement (fun el -> el.GetString())
        |> Result.bind (fun s ->
                match s |> Double.TryParse with
                | true, d -> d |> JsonFloatValue |> Ok
                | false, _ -> nameof(Double) |> JsonElementValue.getEx
            )
    static member tryGetJsonStringValue result =
        result |> toResultFromStringElement (fun el -> el.GetString() |> JsonStringValue)

    static member tryGetJsonUriValue (uriKind: UriKind) result =
        result |> toResultFromStringElement (fun el -> el.GetString())
        |> Result.bind (fun s ->
            match Uri.TryCreate(s, uriKind) with
            | true, uri -> Ok <| JsonUriValue uri
            | false, _ -> Error <| JsonException $"The expected {nameof(Uri)} value is not here.")

    member this.BooleanValue =
        match this with
        | JsonBooleanValue b -> Some b
        | _ -> None

    member this.DateTimeValue =
        match this with
        | JsonDateTimeValue b -> Some b
        | _ -> None

    member this.IntValue =
        match this with
        | JsonIntValue i -> Some i
        | _ -> None

    member this.FloatValue =
        match this with
        | JsonFloatValue f -> Some f
        | _ -> None

    member this.StringValue =
        match this with
        | JsonStringValue s -> Some s
        | _ -> None

    member this.UriValue =
        match this with
        | JsonUriValue s -> Some s
        | _ -> None
