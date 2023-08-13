import { WindowAnimation } from 'songhay';

export class ProgressiveAudioUtility {
    static getHTMLAudioElement(): HTMLAudioElement | null {
        return window.document.querySelector('#audio-player-container>audio');
    }

    static getPlayPauseButtonElement(): HTMLButtonElement | null {
        return window.document.querySelector('#play-pause-block>button');
    }

    static playAnimation: WindowAnimation | null = null;

    // noinspection JSUnusedGlobalSymbols
    static async handleAudioMetadataLoadedAsync(instance: DotNet.DotNetObject) : Promise<void> {
        const button: HTMLButtonElement | null = ProgressiveAudioUtility.getPlayPauseButtonElement();
        const audio: HTMLAudioElement | null = ProgressiveAudioUtility.getHTMLAudioElement();

        ProgressiveAudioUtility.toggleElementEnabled(button);

        await ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);

        ProgressiveAudioUtility.toggleElementEnabled(button);
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

    // noinspection JSUnusedGlobalSymbols
    static loadAudioTrack(audio: HTMLAudioElement | null, src: string) : void {
        audio?.setAttribute('src', src);
        audio?.load();
    }

    static setAudioCurrentTime(input: HTMLInputElement | null) : void {
        const audio: HTMLAudioElement | null = ProgressiveAudioUtility.getHTMLAudioElement();

        if(audio && input) { audio.currentTime = parseFloat(input.value); }
    }

    // noinspection JSUnusedGlobalSymbols
    static startPlayAnimation(instance: DotNet.DotNetObject) : void {
        const button: HTMLButtonElement | null = ProgressiveAudioUtility.getPlayPauseButtonElement();
        const audio: HTMLAudioElement | null = ProgressiveAudioUtility.getHTMLAudioElement();
        const fps: number = 1; // frames per second
        const readyStatePollFreq: number = 250; // milliseconds

        ProgressiveAudioUtility.toggleElementEnabled(button);

        const timeId = window.setTimeout(async () => {
            // poll faster than animation ticks until `readyState` changes:
            if(!audio?.ended && audio?.paused && audio?.readyState > 0) {
                window.clearTimeout(timeId);

                await audio.play();
            }
        }, readyStatePollFreq);

        ProgressiveAudioUtility.playAnimation = WindowAnimation.registerAndGenerate(fps, async _ => {

            if(audio?.ended) {
                audio.currentTime = 0;

                await ProgressiveAudioUtility.stopPlayAnimationAsync(instance);

                return;
            }

            await ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
        });

        WindowAnimation.animate();

        ProgressiveAudioUtility.toggleElementEnabled(button);
    }

    static async stopPlayAnimationAsync(instance: DotNet.DotNetObject): Promise<void> {
        const button: HTMLButtonElement | null = ProgressiveAudioUtility.getPlayPauseButtonElement();
        const audio: HTMLAudioElement | null = ProgressiveAudioUtility.getHTMLAudioElement();

        ProgressiveAudioUtility.toggleElementEnabled(button);

        if(audio && !audio.paused && audio.readyState > 0) { audio.pause(); }

        await ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);

        WindowAnimation.cancelAnimation(ProgressiveAudioUtility.playAnimation?.id ?? undefined);

        ProgressiveAudioUtility.toggleElementEnabled(button);
    }

    static toggleElementEnabled(element: HTMLElement | null): void {
        if (!element) { return; }

        element.toggleAttribute('disabled');
    }
}
