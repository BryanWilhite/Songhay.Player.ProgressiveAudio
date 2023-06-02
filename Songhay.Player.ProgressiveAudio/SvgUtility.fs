namespace Songhay.Modules.Bolero

open System.Collections.Generic

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero
open Songhay.Modules.Models

module SvgUtility =

    let polygonNode (points: string) =
        elt "polygon" {
            "points" => points
        }

    let toSvgBlock (svgDictionary: IDictionary<Identifier, Node>) =
        svg {
            "xmlns" => SvgUtility.SvgUri
            attr.style "display: none;"
            forEach svgDictionary <| fun pair -> pair.Value
        }

    let toSvgUse (id: Identifier) =
        elt "use" {
            attr.id id.StringValue
            "xlink:href" => $"#{id.StringValue}"
        }
