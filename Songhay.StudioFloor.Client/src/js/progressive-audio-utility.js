import { __awaiter } from "tslib";
import { WindowAnimation } from 'songhay';
export class ProgressiveAudioUtility {
    static getHTMLAudioElement() {
        return window.document.querySelector('#audio-player-container>audio');
    }
    static startPlayAnimation(instance) {
        ProgressiveAudioUtility.playAnimation = WindowAnimation.registerAndGenerate(1, (_) => __awaiter(this, void 0, void 0, function* () {
            var _a;
            const audio = ProgressiveAudioUtility.getHTMLAudioElement();
            if (audio === null || audio === void 0 ? void 0 : audio.paused) {
                yield (audio === null || audio === void 0 ? void 0 : audio.play());
            }
            try {
                yield instance.invokeMethodAsync('animateAsync', {
                    animationStatus: (_a = ProgressiveAudioUtility.playAnimation) === null || _a === void 0 ? void 0 : _a.getDiagnosticStatus(),
                    audioDuration: audio === null || audio === void 0 ? void 0 : audio.duration,
                    isAudioPaused: audio === null || audio === void 0 ? void 0 : audio.paused
                });
            }
            catch (error) {
                console.error({ error });
                WindowAnimation.cancelAnimation();
            }
        }));
        WindowAnimation.animate();
    }
    static stopPlayAnimation() {
        var _a, _b;
        const audio = ProgressiveAudioUtility.getHTMLAudioElement();
        audio === null || audio === void 0 ? void 0 : audio.pause();
        WindowAnimation.cancelAnimation((_b = (_a = ProgressiveAudioUtility.playAnimation) === null || _a === void 0 ? void 0 : _a.id) !== null && _b !== void 0 ? _b : undefined);
    }
}
ProgressiveAudioUtility.playAnimation = null;
//# sourceMappingURL=progressive-audio-utility.js.map