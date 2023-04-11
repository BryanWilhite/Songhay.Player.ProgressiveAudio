module Songhay.StudioFloor.Client.ElmishProgram

open System
open System.Net
open System.Net.Http
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open Elmish
open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma.Element
open Songhay.Modules.Bolero.Visuals.Bulma.Layout
open Songhay.StudioFloor.Client.ElmishTypes

type StudioFloorProgramComponent() =
    inherit ProgramComponent<Model, Message>()

    let update (jsRuntime: IJSRuntime) (client: HttpClient) message model =
        match message with
        | Error _ -> model, Cmd.none
        | GetReadMe ->
            let success (result: Result<string, HttpStatusCode>) =
                let data = result |> Result.valueOr (fun code -> $"The expected README data is not here. [error code: {code}]")
                Message.GotReadMe data
            let failure ex = ((jsRuntime |> Some), ex) ||> ClientUtility.passFailureToConsole |> Message.Error
            let uri = ("./README.html", UriKind.Relative) |> Uri
            let cmd = Cmd.OfAsync.either ClientUtility.Remote.tryDownloadToStringAsync (client, uri)  success failure
            model, cmd
        | GotReadMe data ->
            let m = { model with readMeData = (data |> Some) }
            m, Cmd.none
        | Message.SetTab tab ->
            let m = { model with tab = tab }
            match tab with
            | _ -> m, Cmd.none

    let view model dispatch =
        let tabs = [
            ("README", ReadMeTab)
        ]

        concat {
            div {
                [
                    "tabs";
                    "has-background-grey-light";
                    "is-toggle";
                    "is-fullwidth";
                    "is-large"
                ] |> CssClasses.toHtmlClassFromList

                ul {
                    forEach tabs <| fun (label, pg) ->
                    li {
                        a {
                            attr.href "#"
                            DomElementEvent.Click.PreventDefault
                            on.click (fun _ -> SetTab pg |> dispatch)
                            text label
                        }
                    }
                }
            }

            cond model.tab <| function
            | ReadMeTab ->
                if model.readMeData.IsNone then
                    text "loading…"
                else
                    bulmaContainer
                        ContainerWidthFluid
                        NoCssClasses
                        (bulmaNotification
                            (HasClasses <| CssClasses [ "is-info" ])
                            (rawHtml model.readMeData.Value))
        }

    [<Inject>]
    member val HttpClient = Unchecked.defaultof<HttpClient> with get, set

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    override this.Program =
        let initModel = {
            tab = ReadMeTab
            readMeData = None
        }
        let init = (fun _ -> initModel, Cmd.ofMsg Message.GetReadMe)
        let update = update this.JSRuntime this.HttpClient
        Program.mkProgram init update view
