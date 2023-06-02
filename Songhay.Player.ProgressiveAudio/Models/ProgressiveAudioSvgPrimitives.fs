namespace Songhay.Player.ProgressiveAudio.Models

open Bolero.Html

open Songhay.Modules.Bolero.SvgUtility

open Songhay.Modules.Models

type ProgressiveAudioSvgKeys =
    | PLAY
    | PAUSE

    member this.Value = this.ToString().ToLowerInvariant()

    member this.ToAlphanumeric = Alphanumeric this.Value

type ProgressiveAudioSvgData() =

    static let Collection =
        [
            PLAY.ToAlphanumeric,
            elt "symbol" {
                attr.id PLAY.Value
                polygonNode "24,0 96,48 24,96"
            }
            PAUSE.ToAlphanumeric,
            elt "symbol" {
                attr.id PAUSE.Value
                polygonNode "12,0 36,0 36,96 12,96"
                polygonNode "48,0 72,0 72,96 48,96"
            }
        ] |> dict

    static member Get (id: Identifier) = Collection[id]

    static member HasKey (id: Identifier) = Collection.ContainsKey(id)
