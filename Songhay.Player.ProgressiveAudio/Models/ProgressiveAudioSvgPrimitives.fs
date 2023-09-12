namespace Songhay.Player.ProgressiveAudio.Models

open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.SvgElement

open Songhay.Modules.Models

/// <summary>
/// Defines the keys to identify SVG visuals.
/// </summary>
type ProgressiveAudioSvgKeys =
    | PLAY
    | PAUSE

    /// <summary>The <see cref="string"/> value of this instance (in lower case)</summary>
    member this.Value = this.ToString().ToLowerInvariant()

    /// <summary>converts this instance into an <see cref="Identifier"/></summary>
    member this.ToAlphanumeric = Alphanumeric this.Value

/// <summary>
/// Centralizes a collection of the SVG visuals of this domain.
/// </summary>
type ProgressiveAudioSvgData() =

    /// <summary>the collection of the SVG visuals</summary>
    static let Collection =
        [
            PLAY.ToAlphanumeric,
            polygonNode NoAttr "24,0 96,48 24,96"

            PAUSE.ToAlphanumeric,
            concat {
                polygonNode NoAttr "12,0 36,0 36,96 12,96"
                polygonNode NoAttr "48,0 72,0 72,96 48,96"
            }
        ] |> dict

    /// <summary>gets an SVG <see cref="Node"/> with the specified <see cref="Identifier"/></summary>
    static member Get (id: Identifier) = Collection[id]

    /// <summary>returns <c>true</c> when the specified <see cref="Identifier"/> maps to an SVG visual</summary>
    static member HasKey (id: Identifier) = Collection.ContainsKey(id)
