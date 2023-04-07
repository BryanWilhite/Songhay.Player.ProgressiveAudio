namespace Songhay.Modules

open System
open System.Text.Json

module Result =

    let private getFormatException (typeName: string) = FormatException $"The expected {typeName} value is not here."

    let mapToJsonException (r: Result<_,_>) =
        r
        |> Result.mapError (fun ex -> JsonException("See inner exception.", ex))

    let parseBoolean (s: string) =
        match s |> Boolean.TryParse with
        | true, b -> b |> Ok
        | _ -> nameof(Boolean) |> getFormatException |> Error

    let parseDateTime (s: string) =
        match s |> DateTime.TryParse with
        | true, dt -> dt |> Ok
        | _ -> nameof(DateTime) |> getFormatException |> Error

    let parseDouble (s: string) =
        match s |> Double.TryParse with
        | true, f -> Ok f
        | _ -> nameof(Double) |> getFormatException |> Error

    let parseInt32 (s: string) =
        match s |> Int32.TryParse with
        | true, i -> Ok i
        | _ -> nameof(Int32) |> getFormatException |> Error

    let parseUri (uriKind: UriKind) (s: string) =
        match Uri.TryCreate(s, uriKind) with
        | true, uri -> Ok uri
        | _ -> nameof(Uri) |> getFormatException |> Error
