namespace Songhay.StudioFloor.Client.Models

open System.Net.Http

open Microsoft.JSInterop

open Songhay.Player.ProgressiveAudio.Models

type StudioFloorModel =
    {
        blazorServices: {| httpClient: HttpClient; jsRuntime: IJSRuntime |}
        paModel: ProgressiveAudioModel
        page: StudioFloorPage
        readMeData: string option
    }

    static member initialize (httpClient: HttpClient) (jsRuntime: IJSRuntime) =
        {
            blazorServices = {| httpClient = httpClient; jsRuntime = jsRuntime |}
            paModel = ProgressiveAudioModel.initialize jsRuntime
            page = ReadMePage
            readMeData = None
        }
