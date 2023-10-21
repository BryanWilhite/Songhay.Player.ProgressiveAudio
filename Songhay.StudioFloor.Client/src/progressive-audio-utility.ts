import { WindowAnimation } from 'songhay';

export class ProgressiveAudioUtility {

    static playAnimation: WindowAnimation | null = null;

    // noinspection JSUnusedGlobalSymbols
    static async handleAudioMetadataLoadedAsync(instance: DotNet.DotNetObject, audio: HTMLAudioElement | null) : Promise<void> {
        await ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
    }

    static async invokeDotNetMethodAsync(instance: DotNet.DotNetObject, audio: HTMLAudioElement | null) : Promise<void> {
        let data : {} | null = null;

        try {
            data = {
                animationStatus: ProgressiveAudioUtility.playAnimation?.getDiagnosticStatus(),
                audioCurrentTime: audio?.currentTime,
                audioDuration: audio?.duration,
                audioReadyState: audio?.readyState,
                isAudioPaused: audio?.paused
            };

            await instance?.invokeMethodAsync(
                'animateAsync',
                data
            );
        } catch (error) {
            console.error({ error, instance, data });
            WindowAnimation.cancelAnimation();
        }
    }

    // noinspection JSUnusedGlobalSymbols
    static loadAudioTrack(audio: HTMLAudioElement | null, src: string) : void {
        audio?.setAttribute('src', src);
        audio?.load();
    }

    static setAudioCurrentTime(input: HTMLInputElement | null, audio: HTMLAudioElement | null) : void {
        if(audio && input) { audio.currentTime = parseFloat(input.value); }
    }

    // noinspection JSUnusedGlobalSymbols
    static async startPlayAnimationAsync(instance: DotNet.DotNetObject, audio: HTMLAudioElement | null) : Promise<void> {
        const fps: number = 1; // frames per second

        if(audio && audio.readyState > 2 && audio.paused) { await audio.play(); }
        else { console.error("The audio could not play!", {audio}); }

        ProgressiveAudioUtility.playAnimation = WindowAnimation.registerAndGenerate(fps, async _ => {

            if(audio?.ended) {
                audio.currentTime = 0;

                await ProgressiveAudioUtility.stopPlayAnimationAsync(instance, audio);

                return;
            }

            await ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
        });

        WindowAnimation.animate();
    }

    static async stopPlayAnimationAsync(instance: DotNet.DotNetObject, audio: HTMLAudioElement | null): Promise<void> {

        if(audio && audio.readyState > 0 && !audio.paused) { audio.pause(); }

        await ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);

        WindowAnimation.cancelAnimation(ProgressiveAudioUtility.playAnimation?.id ?? undefined);
    }
}
