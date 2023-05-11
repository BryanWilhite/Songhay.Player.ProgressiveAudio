namespace Songhay.StudioFloor.Client.Models

open Songhay.Player.ProgressiveAudio.Models

type StudioFloorModel =
    {
        tab: StudioFloorTab
        readMeData: string option
        paModel: ProgressiveAudioModel
    }

    static member initialize =
        {
            tab = ReadMeTab
            readMeData = None
            paModel = ProgressiveAudioModel.initialize
        }
