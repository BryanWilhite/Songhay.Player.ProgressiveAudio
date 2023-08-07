import {WindowAnimation} from 'songhay';

export class ProgressiveAudioUtility {
    static getHTMLAudioElement(): HTMLAudioElement | null {
        return window.document.querySelector('#audio-player-container>audio');
    }

    static playAnimation: WindowAnimation | null = null;

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

    static startPlayAnimation(instance: DotNet.DotNetObject) : void {

        ProgressiveAudioUtility.playAnimation = WindowAnimation.registerAndGenerate(1, async _ => {

            const audio: HTMLAudioElement | null = ProgressiveAudioUtility.getHTMLAudioElement();

            if(audio?.paused && audio?.readyState > 0) { await audio.play(); }

            await ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
        });

        WindowAnimation.animate();
    }

    static async stopPlayAnimationAsync(instance: DotNet.DotNetObject): Promise<void>
    {
        const audio: HTMLAudioElement | null = ProgressiveAudioUtility.getHTMLAudioElement();

        if(audio && !audio.paused && audio.readyState > 0) { audio.pause(); }

        await ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);

        WindowAnimation.cancelAnimation(ProgressiveAudioUtility.playAnimation?.id ?? undefined);
    }
}
