namespace Songhay.StudioFloor.Client.Models

open Songhay.Player.ProgressiveAudio.Models

type StudioFloorModel = {
    playerModel: ProgressiveAudioModel
    readMeData: string option
    tab: StudioFloorTab
}
