import { __awaiter } from "tslib";
import { WindowAnimation } from 'songhay';
export class ProgressiveAudioUtility {
    static getHTMLAudioElement() {
        return window.document.querySelector('#audio-player-container>audio');
    }
    static invokeDotNetMethodAsync(instance, audio) {
        var _a;
        return __awaiter(this, void 0, void 0, function* () {
            try {
                yield instance.invokeMethodAsync('animateAsync', {
                    animationStatus: (_a = ProgressiveAudioUtility.playAnimation) === null || _a === void 0 ? void 0 : _a.getDiagnosticStatus(),
                    audioDuration: audio === null || audio === void 0 ? void 0 : audio.duration,
                    audioReadyState: audio === null || audio === void 0 ? void 0 : audio.readyState,
                    isAudioPaused: audio === null || audio === void 0 ? void 0 : audio.paused
                });
            }
            catch (error) {
                console.error({ error });
                WindowAnimation.cancelAnimation();
            }
        });
    }
    static loadAudioTrack(src) {
        const audio = ProgressiveAudioUtility.getHTMLAudioElement();
        audio === null || audio === void 0 ? void 0 : audio.setAttribute('src', src);
        audio === null || audio === void 0 ? void 0 : audio.load();
    }
    static startPlayAnimation(instance) {
        ProgressiveAudioUtility.playAnimation = WindowAnimation.registerAndGenerate(1, (_) => __awaiter(this, void 0, void 0, function* () {
            const audio = ProgressiveAudioUtility.getHTMLAudioElement();
            if ((audio === null || audio === void 0 ? void 0 : audio.paused) && (audio === null || audio === void 0 ? void 0 : audio.readyState) > 0) {
                yield audio.play();
            }
            yield ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
        }));
        WindowAnimation.animate();
    }
    static stopPlayAnimationAsync(instance) {
        var _a, _b;
        return __awaiter(this, void 0, void 0, function* () {
            const audio = ProgressiveAudioUtility.getHTMLAudioElement();
            if (audio && !audio.paused && audio.readyState > 0) {
                audio.pause();
            }
            yield ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
            WindowAnimation.cancelAnimation((_b = (_a = ProgressiveAudioUtility.playAnimation) === null || _a === void 0 ? void 0 : _a.id) !== null && _b !== void 0 ? _b : undefined);
        });
    }
}
ProgressiveAudioUtility.playAnimation = null;
//# sourceMappingURL=progressive-audio-utility.js.map