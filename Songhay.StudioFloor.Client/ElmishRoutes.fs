namespace Songhay.StudioFloor.Client

open Bolero

open Songhay.StudioFloor.Client.Models

module ElmishRoutes =

    let router = Router.infer SetPage (_.page)
