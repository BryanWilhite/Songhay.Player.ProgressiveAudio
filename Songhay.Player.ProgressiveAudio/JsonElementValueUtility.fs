namespace Songhay.Modules

open System

open FsToolkit.ErrorHandling.Operator.Result

open Songhay.Modules.JsonDocumentUtility

module JsonElementValueUtility =

    let tryGetJsonStringValue result =
        result
        |> toResultFromStringElement (fun el -> el.GetString())

    let tryGetJsonBooleanValue result =
        result
        |> toResultFromBooleanElement (fun el -> el.GetBoolean())

    let tryGetJsonBooleanValueFromStringElement result =
        result
        |> tryGetJsonStringValue
        >>= (fun s -> s |> Result.parseBoolean |> Result.mapToJsonException)

    let tryGetJsonDateTimeValue result =
        result
        |> tryGetJsonStringValue
        >>= (fun s -> s |> Result.parseDateTime |> Result.mapToJsonException)

    let tryGetJsonIntValue result =
        result
        |> toResultFromNumericElement (fun el -> el.GetInt32())

    let tryGetJsonIntValueFromStringElement result =
        result
        |> tryGetJsonStringValue
        >>= (fun s -> s |> Result.parseInt32 |> Result.mapToJsonException)

    let tryGetJsonFloatValue result =
        result
        |> toResultFromNumericElement (fun el -> el.GetDouble())

    let tryGetJsonFloatValueFromStringElement result =
        result
        |> tryGetJsonStringValue
        >>= (fun s -> s |> Result.parseDouble |> Result.mapToJsonException)

    let tryGetJsonUriValue (uriKind: UriKind) result =
        result
        |> tryGetJsonStringValue
        >>= (fun s -> s |> Result.parseUri uriKind |> Result.mapToJsonException)
