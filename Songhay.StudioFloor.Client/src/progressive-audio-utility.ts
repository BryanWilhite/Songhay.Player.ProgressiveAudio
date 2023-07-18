import { WindowAnimation } from 'songhay';

export class ProgressiveAudioUtility {
    static playAnimation: WindowAnimation | null = null;
    static startPlayAnimation(instance: DotNet.DotNetObject) : void {

        console.warn({instance});

        ProgressiveAudioUtility.playAnimation = WindowAnimation.registerAndGenerate(1, async _ => {
            try {
                await instance.invokeMethodAsync('startAsync', null);
                console.info(ProgressiveAudioUtility.playAnimation?.getDiagnosticStatus());
            } catch (error) {
                console.error({error});
                WindowAnimation.cancelAnimation();
            }
        });

        WindowAnimation.animate();
    }

    static stopPlayAnimation(): void
    {
        WindowAnimation.cancelAnimation(ProgressiveAudioUtility.playAnimation?.id ?? undefined);
    }
}
