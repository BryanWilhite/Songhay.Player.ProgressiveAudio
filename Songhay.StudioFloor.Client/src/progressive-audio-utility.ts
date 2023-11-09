import { WindowAnimation } from 'songhay';

export class ProgressiveAudioUtility {

    static playAnimation: WindowAnimation | null = null;

    // noinspection JSUnusedGlobalSymbols
    static getCurrentAudioElement() : HTMLAudioElement | null {
        return window.document.querySelector('#audio-player-container>audio[data-track-is-active=true]');
    }

    // noinspection JSUnusedGlobalSymbols
    static async handleAudioMetadataLoadedAsync(instance: DotNet.DotNetObject) : Promise<void> {
        await ProgressiveAudioUtility.invokeDotNetMethodAsync(instance);
    }

    static async invokeDotNetMethodAsync(instance: DotNet.DotNetObject) : Promise<void> {
        let data : {} | null = null;

        const audio = this.getCurrentAudioElement();

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

    static setAudioCurrentTime(input: HTMLInputElement | null) : void {
        const audio = this.getCurrentAudioElement();

        if(audio && input) {
            audio.currentTime = parseFloat(input.value);
            audio.dataset.hasSetCurrentTime = 'yes';
        }
        console.info('setAudioCurrentTime',
            {
                readyState: audio?.readyState,
                paused: audio?.paused,
                currentTime: audio?.currentTime,
                hasSetCurrentTime: audio?.dataset.hasSetCurrentTime,
                inputValue: input?.value
            }
        );
    }

    // noinspection JSUnusedGlobalSymbols
    static async startPlayAnimationAsync(instance: DotNet.DotNetObject) : Promise<void> {
        const audio = this.getCurrentAudioElement();
        const fps: number = 1; // frames per second

        if(audio && audio.readyState > 2 && audio.paused) {
            await audio.play();
        }
        else if (audio && audio.dataset.hasSetCurrentTime === 'yes') {
            await audio.play();
            audio.dataset.hasSetCurrentTime = 'no';
            console.info('startPlayAnimationAsync',
                {
                    readyState: audio?.readyState,
                    paused: audio?.paused,
                    currentTime: audio?.currentTime,
                    hasSetCurrentTime: audio?.dataset.hasSetCurrentTime,
                    audio
                }
            );
        }
        else {
            console.error('The audio could not play!',
                'startPlayAnimationAsync',
                {
                    readyState: audio?.readyState,
                    paused: audio?.paused,
                    currentTime: audio?.currentTime,
                    hasSetCurrentTime: audio?.dataset.hasSetCurrentTime,
                    audio
                }
            );
        }

        ProgressiveAudioUtility.playAnimation = WindowAnimation.registerAndGenerate(fps, async _ => {

            if(audio?.ended) { console.warn('ended?');
                audio.currentTime = 0;

                await ProgressiveAudioUtility.stopPlayAnimationAsync(instance);

                return;
            }

            await ProgressiveAudioUtility.invokeDotNetMethodAsync(instance);
        });

        WindowAnimation.animate();
    }

    static async stopPlayAnimationAsync(instance: DotNet.DotNetObject): Promise<void> {
        const audio = this.getCurrentAudioElement();

        if(audio && audio.readyState > 0 && !audio.paused) { audio.pause(); }

        await ProgressiveAudioUtility.invokeDotNetMethodAsync(instance);

        WindowAnimation.cancelAnimation(ProgressiveAudioUtility.playAnimation?.id ?? undefined);

        console.info('stopPlayAnimationAsync',
            {
                readyState: audio?.readyState,
                paused: audio?.paused,
                currentTime: audio?.currentTime,
                hasSetCurrentTime: audio?.dataset.hasSetCurrentTime,
                audio
            }
        );
    }
}
