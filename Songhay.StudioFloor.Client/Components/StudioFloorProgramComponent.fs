namespace Songhay.StudioFloor.Client.Components

open System
open Microsoft.AspNetCore.Components

open Elmish
open Bolero

open Songhay.Player.ProgressiveAudio.Models
open Songhay.StudioFloor.Client
open Songhay.StudioFloor.Client.Models

module pcu = ProgramComponentUtility

type StudioFloorProgramComponent() =
    inherit ProgramComponent<StudioFloorModel, StudioFloorMessage>()

    let update message model =

        match message with
        | Error _ ->
            model, Cmd.none
        | GetReadMe ->
            let cmd = pcu.getCommandForGetReadMe model
            model, cmd
        | GotReadMe data ->
            let m = { model with readMeData = data |> Some }
            m, Cmd.none
        | SetPage page ->
            let m = { model with page = page }
            let cmd = pcu.getCommandForSetPage page
            m, cmd
        | StudioFloorMessage.ProgressiveAudioMessage paMessage ->
            let m = { model with paModel = ProgressiveAudioModel.updateModel paMessage model.paModel }
            let cmd = pcu.getCommandForProgressiveAudio model paMessage
            m, cmd

    let view model dispatch =
        TabsElmishComponent.EComp model dispatch

    [<Inject>]
    member val ServiceProvider = Unchecked.defaultof<IServiceProvider> with get, set

    override this.Program =
        let m = StudioFloorModel.initialize this.ServiceProvider
        let cmd = Cmd.ofMsg StudioFloorMessage.GetReadMe

        Program.mkProgram (fun _ -> m, cmd) update view
        |> Program.withRouter ElmishRoutes.router
