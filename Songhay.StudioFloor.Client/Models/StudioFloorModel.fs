namespace Songhay.StudioFloor.Client.Models

open System

open Songhay.Player.ProgressiveAudio.Models

type StudioFloorModel =
    {
        paModel: ProgressiveAudioModel
        page: StudioFloorPage
        readMeData: string option
    }

    static member initialize (serviceProvider: IServiceProvider) =
        {
            paModel = ProgressiveAudioModel.initialize serviceProvider
            page = ReadMePage
            readMeData = None
        }
