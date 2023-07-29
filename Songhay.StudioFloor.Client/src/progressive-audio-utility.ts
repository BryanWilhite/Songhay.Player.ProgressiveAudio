import {WindowAnimation} from 'songhay';

export class ProgressiveAudioUtility {
    static getHTMLAudioElement(): HTMLAudioElement | null {
        return window.document.querySelector('#audio-player-container>audio');
    }

    static playAnimation: WindowAnimation | null = null;

    static startPlayAnimation(instance: DotNet.DotNetObject) : void {

        ProgressiveAudioUtility.playAnimation = WindowAnimation.registerAndGenerate(1, async _ => {

            const audio: HTMLAudioElement | null = ProgressiveAudioUtility.getHTMLAudioElement();

            if(audio?.paused) { await audio?.play(); }

            try {
                await instance.invokeMethodAsync(
                    'animateAsync',
                    {
                        animationStatus: ProgressiveAudioUtility.playAnimation?.getDiagnosticStatus(),
                        audioDuration: audio?.duration,
                        isAudioPaused: audio?.paused
                    }
                );
            } catch (error) {
                console.error({error});
                WindowAnimation.cancelAnimation();
            }
        });

        WindowAnimation.animate();
    }

    static stopPlayAnimation(): void
    {
        const audio: HTMLAudioElement | null = ProgressiveAudioUtility.getHTMLAudioElement();

        audio?.pause();

        WindowAnimation.cancelAnimation(ProgressiveAudioUtility.playAnimation?.id ?? undefined);
    }
}
