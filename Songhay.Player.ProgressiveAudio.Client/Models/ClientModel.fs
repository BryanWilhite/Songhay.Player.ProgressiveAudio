namespace Songhay.Player.ProgressiveAudio.Client.Models

open System

open Songhay.Player.ProgressiveAudio.Models

type AudioClientModel =
    {
        page: AudioClientPage
        paModel: ProgressiveAudioModel
    }

    static member initialize (serviceProvider: IServiceProvider) =
        {
            page = NoContentPage
            paModel = ProgressiveAudioModel.initialize serviceProvider
        }
