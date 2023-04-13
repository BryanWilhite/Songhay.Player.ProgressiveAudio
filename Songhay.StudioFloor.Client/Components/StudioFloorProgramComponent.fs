namespace Songhay.StudioFloor.Client.Components

open System
open System.Net
open System.Net.Http
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open FsToolkit.ErrorHandling
open Elmish
open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma.Element
open Songhay.Modules.Bolero.Visuals.Bulma.Layout

open Songhay.Player.ProgressiveAudio.Components
open Songhay.Player.ProgressiveAudio.Models
open Songhay.StudioFloor.Client
open Songhay.StudioFloor.Client.Models

type StudioFloorProgramComponent() =
    inherit ProgramComponent<StudioFloorModel, StudioFloorMessage>()

    let update (jsRuntime: IJSRuntime) (client: HttpClient) message model =
        match message with
        | Error _ -> model, Cmd.none
        | GetReadMe ->
            let success (result: Result<string, HttpStatusCode>) =
                let data = result |> Result.valueOr (fun code -> $"The expected README data is not here. [error code: {code}]")
                StudioFloorMessage.GotReadMe data
            let failure ex = ((jsRuntime |> Some), ex) ||> ClientUtility.passFailureToConsole |> StudioFloorMessage.Error
            let uri = ("./README.html", UriKind.Relative) |> Uri
            let cmd = Cmd.OfAsync.either ClientUtility.Remote.tryDownloadToStringAsync (client, uri)  success failure
            model, cmd
        | GotReadMe data ->
            let m = { model with readMeData = (data |> Some) }
            m, Cmd.none
        | StudioFloorMessage.SetTab tab ->
            let m = { model with tab = tab }
            match tab with
            | _ -> m, Cmd.none
        | StudioFloorMessage.ProgressiveAudioMessage playerMessage ->
            ClientUtility.updatePlayer jsRuntime client playerMessage model

    let view model dispatch =
        let tabs = [
            ("README", ReadMeTab)
            ("Progressive Audio Player", PlayerTab)
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
            | PlayerTab ->
                bulmaContainer
                    ContainerWidthFluid
                    NoCssClasses
                    (PlayerElmishComponent.EComp model.playerModel (ProgressiveAudioMessage >> dispatch))
        }

    [<Inject>]
    member val HttpClient = Unchecked.defaultof<HttpClient> with get, set

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    override this.Program =
        let initModel = {
            playerModel = ProgressiveAudioModel.initialize
            tab = ReadMeTab
            readMeData = None
        }
        let init = (fun _ -> initModel, Cmd.ofMsg StudioFloorMessage.GetReadMe)
        let update = update this.JSRuntime this.HttpClient
        Program.mkProgram init update view