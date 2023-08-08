import { __awaiter } from "tslib";
import { WindowAnimation } from 'songhay';
export class ProgressiveAudioUtility {
    static getHTMLAudioElement() {
        return window.document.querySelector('#audio-player-container>audio');
    }
    static getPlayPauseButtonElement() {
        return window.document.querySelector('#play-pause-block>button');
    }
    static handleAudioMetadataLoadedAsync(instance) {
        return __awaiter(this, void 0, void 0, function* () {
            const button = ProgressiveAudioUtility.getPlayPauseButtonElement();
            const audio = ProgressiveAudioUtility.getHTMLAudioElement();
            if (button) {
                button.disabled = true;
            }
            yield ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
            if (button) {
                button.disabled = false;
            }
        });
    }
    static invokeDotNetMethodAsync(instance, audio) {
        var _a;
        return __awaiter(this, void 0, void 0, function* () {
            try {
                yield instance.invokeMethodAsync('animateAsync', {
                    animationStatus: (_a = ProgressiveAudioUtility.playAnimation) === null || _a === void 0 ? void 0 : _a.getDiagnosticStatus(),
                    audioCurrentTime: audio === null || audio === void 0 ? void 0 : audio.currentTime,
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
    static startPlayAnimationAsync(instance) {
        return __awaiter(this, void 0, void 0, function* () {
            const button = ProgressiveAudioUtility.getPlayPauseButtonElement();
            const audio = ProgressiveAudioUtility.getHTMLAudioElement();
            if (button) {
                button.disabled = true;
            }
            if (!(audio === null || audio === void 0 ? void 0 : audio.ended) && (audio === null || audio === void 0 ? void 0 : audio.paused) && (audio === null || audio === void 0 ? void 0 : audio.readyState) > 0) {
                yield audio.play();
            }
            console.warn('startPlayAnimationAsync', { ended: audio === null || audio === void 0 ? void 0 : audio.ended, paused: audio === null || audio === void 0 ? void 0 : audio.paused, state: audio === null || audio === void 0 ? void 0 : audio.readyState });
            ProgressiveAudioUtility.playAnimation = WindowAnimation.registerAndGenerate(1, (_) => __awaiter(this, void 0, void 0, function* () {
                if (audio === null || audio === void 0 ? void 0 : audio.ended) {
                    audio.currentTime = 0;
                    yield ProgressiveAudioUtility.stopPlayAnimationAsync(instance);
                    return;
                }
                yield ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
            }));
            WindowAnimation.animate();
            if (button) {
                button.disabled = false;
            }
        });
    }
    static stopPlayAnimationAsync(instance) {
        var _a, _b;
        return __awaiter(this, void 0, void 0, function* () {
            const button = ProgressiveAudioUtility.getPlayPauseButtonElement();
            const audio = ProgressiveAudioUtility.getHTMLAudioElement();
            if (button) {
                button.disabled = true;
            }
            if (audio && !audio.paused && audio.readyState > 0) {
                audio.pause();
            }
            yield ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
            WindowAnimation.cancelAnimation((_b = (_a = ProgressiveAudioUtility.playAnimation) === null || _a === void 0 ? void 0 : _a.id) !== null && _b !== void 0 ? _b : undefined);
            if (button) {
                button.disabled = false;
            }
        });
    }
}
ProgressiveAudioUtility.playAnimation = null;
//# sourceMappingURL=progressive-audio-utility.js.map