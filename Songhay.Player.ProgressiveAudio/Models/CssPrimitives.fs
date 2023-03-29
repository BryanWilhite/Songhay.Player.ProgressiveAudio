namespace Songhay.Player.ProgressiveAudio.Models

open Songhay.Modules.StringUtility

type CssVariable =
    | CssVariable of string

    static member fromInput (input: string) =
        match input.TrimStart('-') |> toKabobCase with
        | Some s -> CssVariable $"--{s}"
        | None -> CssVariable "--?"

    member this.toCssDeclaration (cssVariableValue: string) = $"{this.Value}: {cssVariableValue};"

    member this.toCssPropertyValue = $"var({this.Value})"

    member this.toCssPropertyValueWithFallback (fallback: string) = $"var({this.Value}, {fallback})"

    member this.Value = let (CssVariable v) = this in v

    override this.ToString() = this.Value

type CssVariables =
    | CssVariables of CssVariable list

    static member fromInput (input: string list) = input |> List.map CssVariable.fromInput
