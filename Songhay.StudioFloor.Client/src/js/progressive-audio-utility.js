import { __awaiter } from "tslib";
import { WindowAnimation } from 'songhay';
export class ProgressiveAudioUtility {
    // noinspection JSUnusedGlobalSymbols
    static handleAudioMetadataLoadedAsync(instance, audio) {
        return __awaiter(this, void 0, void 0, function* () {
            yield ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
        });
    }
    static invokeDotNetMethodAsync(instance, audio) {
        var _a;
        return __awaiter(this, void 0, void 0, function* () {
            let data = null;
            try {
                data = {
                    animationStatus: (_a = ProgressiveAudioUtility.playAnimation) === null || _a === void 0 ? void 0 : _a.getDiagnosticStatus(),
                    audioCurrentTime: audio === null || audio === void 0 ? void 0 : audio.currentTime,
                    audioDuration: audio === null || audio === void 0 ? void 0 : audio.duration,
                    audioReadyState: audio === null || audio === void 0 ? void 0 : audio.readyState,
                    isAudioPaused: audio === null || audio === void 0 ? void 0 : audio.paused
                };
                yield (instance === null || instance === void 0 ? void 0 : instance.invokeMethodAsync('animateAsync', data));
            }
            catch (error) {
                console.error({ error, instance, data });
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
    static startPlayAnimation(instance, audio) {
        const fps = 1; // frames per second
        ProgressiveAudioUtility.playAnimation = WindowAnimation.registerAndGenerate(fps, (_) => __awaiter(this, void 0, void 0, function* () {
            if (audio === null || audio === void 0 ? void 0 : audio.ended) {
                audio.currentTime = 0;
                yield ProgressiveAudioUtility.stopPlayAnimationAsync(instance, audio);
                return;
            }
            yield ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
        }));
        WindowAnimation.animate();
    }
    static stopPlayAnimationAsync(instance, audio) {
        var _a, _b;
        return __awaiter(this, void 0, void 0, function* () {
            if (audio && audio.readyState > 0 && !audio.paused) {
                audio.pause();
            }
            yield ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
            WindowAnimation.cancelAnimation((_b = (_a = ProgressiveAudioUtility.playAnimation) === null || _a === void 0 ? void 0 : _a.id) !== null && _b !== void 0 ? _b : undefined);
        });
    }
}
ProgressiveAudioUtility.playAnimation = null;
//# sourceMappingURL=progressive-audio-utility.js.map