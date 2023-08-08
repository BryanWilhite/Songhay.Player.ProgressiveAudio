import {WindowAnimation} from 'songhay';

export class ProgressiveAudioUtility {
    static getHTMLAudioElement(): HTMLAudioElement | null {
        return window.document.querySelector('#audio-player-container>audio');
    }

    static getPlayPauseButtonElement(): HTMLButtonElement | null {
        return window.document.querySelector('#play-pause-block>button');
    }

    static playAnimation: WindowAnimation | null = null;

    static async handleAudioMetadataLoadedAsync(instance: DotNet.DotNetObject) : Promise<void> {
        const button: HTMLButtonElement | null = ProgressiveAudioUtility.getPlayPauseButtonElement();
        const audio: HTMLAudioElement | null = ProgressiveAudioUtility.getHTMLAudioElement();

        if(button) { button.disabled = true; }

        await ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);

        if(button) { button.disabled = false; }
    }

    static async invokeDotNetMethodAsync(instance: DotNet.DotNetObject, audio: HTMLAudioElement | null) : Promise<void> {
        try {
            await instance.invokeMethodAsync(
                'animateAsync',
                {
                    animationStatus: ProgressiveAudioUtility.playAnimation?.getDiagnosticStatus(),
                    audioCurrentTime: audio?.currentTime,
                    audioDuration: audio?.duration,
                    audioReadyState: audio?.readyState,
                    isAudioPaused: audio?.paused
                }
            );
        } catch (error) {
            console.error({error});
            WindowAnimation.cancelAnimation();
        }
    }

    static loadAudioTrack(src: string) : void {
        const audio: HTMLAudioElement | null = ProgressiveAudioUtility.getHTMLAudioElement();

        audio?.setAttribute('src', src);
        audio?.load();
    }

    static async startPlayAnimationAsync(instance: DotNet.DotNetObject) : Promise<void> {
        const button: HTMLButtonElement | null = ProgressiveAudioUtility.getPlayPauseButtonElement();
        const audio: HTMLAudioElement | null = ProgressiveAudioUtility.getHTMLAudioElement();

        if(button) { button.disabled = true; }

        if(!audio?.ended && audio?.paused && audio?.readyState > 0) { await audio.play(); }

        ProgressiveAudioUtility.playAnimation = WindowAnimation.registerAndGenerate(1, async _ => {

            if(audio?.ended) {
                audio.currentTime = 0;

                await ProgressiveAudioUtility.stopPlayAnimationAsync(instance);

                return;
            }

            await ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
        });

        WindowAnimation.animate();

        if(button) { button.disabled = false; }
    }

    static async stopPlayAnimationAsync(instance: DotNet.DotNetObject): Promise<void>
    {
        const button: HTMLButtonElement | null = ProgressiveAudioUtility.getPlayPauseButtonElement();
        const audio: HTMLAudioElement | null = ProgressiveAudioUtility.getHTMLAudioElement();

        if(button) { button.disabled = true; }
        if(audio && !audio.paused && audio.readyState > 0) { audio.pause(); }

        await ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);

        WindowAnimation.cancelAnimation(ProgressiveAudioUtility.playAnimation?.id ?? undefined);

        if(button) { button.disabled = false; }
    }
}
