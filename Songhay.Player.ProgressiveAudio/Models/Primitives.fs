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

    static member tryGetJsonBooleanValue result =
        result |> toResultFromBooleanElement (fun el -> el.GetBoolean() |> JsonBooleanValue)

    static member tryGetJsonDateTimeValue result =
        result
        |> toResultFromBooleanElement (fun el -> el.GetString())
        |> Result.bind (fun s ->
            match s |> DateTime.TryParse with
            | true, dt -> Ok <| JsonDateTimeValue dt
            | false, _ -> Error <| JsonException $"The expected {nameof(DateTime)} value is not here.")

    static member tryGetJsonIntValue result =
        result |> toResultFromBooleanElement (fun el -> el.GetInt32() |> JsonIntValue)

    static member tryGetJsonFloatValue result =
        result |> toResultFromBooleanElement (fun el -> el.GetDouble() |> JsonFloatValue)

    static member tryGetJsonStringValue result =
        result |> toResultFromBooleanElement (fun el -> el.GetString() |> JsonStringValue)

    static member tryGetJsonUriValue (uriKind: UriKind) result =
        result |> toResultFromBooleanElement (fun el -> el.GetString())
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
