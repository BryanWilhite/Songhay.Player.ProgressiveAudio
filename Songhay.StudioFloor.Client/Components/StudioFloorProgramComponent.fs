namespace Songhay.StudioFloor.Client.Components

open System
open System.Net
open System.Net.Http
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open FsToolkit.ErrorHandling
open Elmish
open Bolero

open Songhay.Player.ProgressiveAudio.Models
open Songhay.StudioFloor.Client
open Songhay.StudioFloor.Client.Models

type StudioFloorProgramComponent() =
    inherit ProgramComponent<StudioFloorModel, StudioFloorMessage>()

    let update (jsRuntime: IJSRuntime) (client: HttpClient) (navMan: NavigationManager) message model =

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
            | PlayerTab ->
                let msg = ProgressiveAudioMessage.GetPlayerManifest |> StudioFloorMessage.ProgressiveAudioMessage
                m, Cmd.ofMsg msg
            | _ -> m, Cmd.none
        | StudioFloorMessage.ProgressiveAudioMessage playerMessage ->
            ClientUtility.updatePlayer jsRuntime client navMan playerMessage model

    let view model dispatch =
        TabsElmishComponent.EComp model dispatch

    [<Inject>]
    member val HttpClient = Unchecked.defaultof<HttpClient> with get, set

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    [<Inject>]
    member val NavigationManager = Unchecked.defaultof<NavigationManager> with get, set

    override this.Program =
        let initModel = {
            playerModel = ProgressiveAudioModel.initialize
            tab = ReadMeTab
            readMeData = None
        }
        let init = (fun _ -> initModel, Cmd.ofMsg StudioFloorMessage.GetReadMe)
        let update = update this.JSRuntime this.HttpClient this.NavigationManager
        Program.mkProgram init update view
