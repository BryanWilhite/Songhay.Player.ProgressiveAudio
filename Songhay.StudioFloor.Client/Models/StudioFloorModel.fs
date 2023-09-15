namespace Songhay.StudioFloor.Client.Models

open System.Net.Http

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Songhay.Player.ProgressiveAudio.Models

type StudioFloorModel =
    {
        blazorServices: {| httpClient: HttpClient; jsRuntime: IJSRuntime; navigationManager: NavigationManager |}
        paModel: ProgressiveAudioModel
        page: StudioFloorPage
        tab: StudioFloorTab
        readMeData: string option
    }

    static member initialize (httpClient: HttpClient) (jsRuntime: IJSRuntime) (navigationManager: NavigationManager) =
        {
            blazorServices = {| httpClient = httpClient; jsRuntime = jsRuntime; navigationManager = navigationManager |}
            paModel = ProgressiveAudioModel.initialize jsRuntime navigationManager
            page = ReadMePage
            tab = ReadMeTab
            readMeData = None
        }
