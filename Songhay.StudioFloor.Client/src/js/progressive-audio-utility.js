import { __awaiter } from "tslib";
import { WindowAnimation } from 'songhay';
export class ProgressiveAudioUtility {
    // noinspection JSUnusedGlobalSymbols
    static handleAudioMetadataLoadedAsync(instance, button, audio) {
        return __awaiter(this, void 0, void 0, function* () {
            ProgressiveAudioUtility.toggleElementEnabled(button);
            yield ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
            ProgressiveAudioUtility.toggleElementEnabled(button);
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
    // noinspection JSUnusedGlobalSymbols
    static loadAudioTrack(audio, src) {
        audio === null || audio === void 0 ? void 0 : audio.setAttribute('src', src);
        audio === null || audio === void 0 ? void 0 : audio.load();
    }
    static setAudioCurrentTime(input, audio) {
        if (audio && input) {
            audio.currentTime = parseFloat(input.value);
        }
    }
    // noinspection JSUnusedGlobalSymbols
    static startPlayAnimation(instance, button, audio) {
        const fps = 1; // frames per second
        const readyStatePollFreq = 250; // milliseconds
        ProgressiveAudioUtility.toggleElementEnabled(button);
        const timeId = window.setTimeout(() => __awaiter(this, void 0, void 0, function* () {
            // poll faster than animation ticks until `readyState` changes:
            if (!(audio === null || audio === void 0 ? void 0 : audio.ended) && (audio === null || audio === void 0 ? void 0 : audio.paused) && (audio === null || audio === void 0 ? void 0 : audio.readyState) > 0) {
                window.clearTimeout(timeId);
                yield audio.play();
            }
        }), readyStatePollFreq);
        ProgressiveAudioUtility.playAnimation = WindowAnimation.registerAndGenerate(fps, (_) => __awaiter(this, void 0, void 0, function* () {
            if (audio === null || audio === void 0 ? void 0 : audio.ended) {
                audio.currentTime = 0;
                yield ProgressiveAudioUtility.stopPlayAnimationAsync(instance, button, audio);
                return;
            }
            yield ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
        }));
        WindowAnimation.animate();
        ProgressiveAudioUtility.toggleElementEnabled(button);
    }
    static stopPlayAnimationAsync(instance, button, audio) {
        var _a, _b;
        return __awaiter(this, void 0, void 0, function* () {
            ProgressiveAudioUtility.toggleElementEnabled(button);
            if (audio && !audio.paused && audio.readyState > 0) {
                audio.pause();
            }
            yield ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
            WindowAnimation.cancelAnimation((_b = (_a = ProgressiveAudioUtility.playAnimation) === null || _a === void 0 ? void 0 : _a.id) !== null && _b !== void 0 ? _b : undefined);
            ProgressiveAudioUtility.toggleElementEnabled(button);
        });
    }
    static toggleElementEnabled(element) {
        if (!element) {
            return;
        }
        element.toggleAttribute('disabled');
    }
}
ProgressiveAudioUtility.playAnimation = null;
//# sourceMappingURL=progressive-audio-utility.js.map