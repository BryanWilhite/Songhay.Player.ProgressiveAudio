# Songhay.Player.ProgressiveAudio

## Bolero Components, Elmish types and JSON handlers for the b-roll player for progressive audio 🎤 content

The main concerns of this work are the utilities, components and models of the b-roll player for progressive audio, including:

- `ProgressiveAudioUtility` ([src](https://github.com/BryanWilhite/Songhay.Player.ProgressiveAudio/blob/main/Songhay.Player.ProgressiveAudio/ProgressiveAudioUtility.fs))
- `PlayerElmishComponent` ([src](https://github.com/BryanWilhite/Songhay.Player.ProgressiveAudio/blob/main/Songhay.Player.ProgressiveAudio/Components/PlayerElmishComponent.fs))
- `ProgressiveAudioModel` ([src](https://github.com/BryanWilhite/Songhay.Player.ProgressiveAudio/blob/main/Songhay.Player.ProgressiveAudio/Models/ProgressiveAudioModel.fs))

## the conventional Studio Floor

The conventional Studio Floor of this Solution is `Songhay.StudioFloor.Client` (see [directory](https://github.com/BryanWilhite/Songhay.Player.ProgressiveAudio/blob/main/Songhay.StudioFloor.Client)) which is here to demonstrate the functionality of the b-roll player for progressive audio:

![the b-roll player for progressive audio](https://raw.githubusercontent.com/BryanWilhite/Songhay.Player.ProgressiveAudio/main/.github/bitmaps/screenshot.png)

The installation of the Studio floor started with this Bolero-ish command:

```bash
dotnet new bolero-app -s=false
```

But the default file layout was changed by hand mostly to take the `src` [directory](https://github.com/BryanWilhite/Songhay.Player.ProgressiveAudio/blob/main/Songhay.StudioFloor.Client/src) back for a Typescript and SCSS pipeline.

### running the Studio Floor

From the Studio Floor [directory](https://github.com/BryanWilhite/Songhay.Player.ProgressiveAudio/blob/main/Songhay.StudioFloor.Client) run:

```bash
dotnet run
```

the screenshot above was the browser location `http://localhost:5000/audio/default` where `default` is the Presentation key that the player uses to load content.

@[BryanWilhite](https://twitter.com/BryanWilhite)
