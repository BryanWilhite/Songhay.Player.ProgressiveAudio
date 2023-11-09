import { __awaiter } from "tslib";
import { WindowAnimation } from 'songhay';
export class ProgressiveAudioUtility {
    // noinspection JSUnusedGlobalSymbols
    static getCurrentAudioElement() {
        return window.document.querySelector('#audio-player-container>audio[data-track-is-active=true]');
    }
    // noinspection JSUnusedGlobalSymbols
    static handleAudioMetadataLoadedAsync(instance) {
        return __awaiter(this, void 0, void 0, function* () {
            yield ProgressiveAudioUtility.invokeDotNetMethodAsync(instance);
        });
    }
    static invokeDotNetMethodAsync(instance) {
        var _a;
        return __awaiter(this, void 0, void 0, function* () {
            let data = null;
            const audio = this.getCurrentAudioElement();
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
    static setAudioCurrentTime(input) {
        const audio = this.getCurrentAudioElement();
        if (audio && input) {
            audio.currentTime = parseFloat(input.value);
            audio.dataset.hasSetCurrentTime = 'yes';
        }
        console.info('setAudioCurrentTime', {
            readyState: audio === null || audio === void 0 ? void 0 : audio.readyState,
            paused: audio === null || audio === void 0 ? void 0 : audio.paused,
            currentTime: audio === null || audio === void 0 ? void 0 : audio.currentTime,
            hasSetCurrentTime: audio === null || audio === void 0 ? void 0 : audio.dataset.hasSetCurrentTime,
            inputValue: input === null || input === void 0 ? void 0 : input.value
        });
    }
    // noinspection JSUnusedGlobalSymbols
    static startPlayAnimationAsync(instance) {
        return __awaiter(this, void 0, void 0, function* () {
            const audio = this.getCurrentAudioElement();
            const fps = 1; // frames per second
            if (audio && audio.readyState > 2 && audio.paused) {
                yield audio.play();
            }
            else if (audio && audio.dataset.hasSetCurrentTime === 'yes') {
                yield audio.play();
                audio.dataset.hasSetCurrentTime = 'no';
                console.info('startPlayAnimationAsync', {
                    readyState: audio === null || audio === void 0 ? void 0 : audio.readyState,
                    paused: audio === null || audio === void 0 ? void 0 : audio.paused,
                    currentTime: audio === null || audio === void 0 ? void 0 : audio.currentTime,
                    hasSetCurrentTime: audio === null || audio === void 0 ? void 0 : audio.dataset.hasSetCurrentTime,
                    audio
                });
            }
            else {
                console.error('The audio could not play!', 'startPlayAnimationAsync', {
                    readyState: audio === null || audio === void 0 ? void 0 : audio.readyState,
                    paused: audio === null || audio === void 0 ? void 0 : audio.paused,
                    currentTime: audio === null || audio === void 0 ? void 0 : audio.currentTime,
                    hasSetCurrentTime: audio === null || audio === void 0 ? void 0 : audio.dataset.hasSetCurrentTime,
                    audio
                });
            }
            ProgressiveAudioUtility.playAnimation = WindowAnimation.registerAndGenerate(fps, (_) => __awaiter(this, void 0, void 0, function* () {
                if (audio === null || audio === void 0 ? void 0 : audio.ended) {
                    console.warn('ended?');
                    audio.currentTime = 0;
                    yield ProgressiveAudioUtility.stopPlayAnimationAsync(instance);
                    return;
                }
                yield ProgressiveAudioUtility.invokeDotNetMethodAsync(instance);
            }));
            WindowAnimation.animate();
        });
    }
    static stopPlayAnimationAsync(instance) {
        var _a, _b;
        return __awaiter(this, void 0, void 0, function* () {
            const audio = this.getCurrentAudioElement();
            if (audio && audio.readyState > 0 && !audio.paused) {
                audio.pause();
            }
            yield ProgressiveAudioUtility.invokeDotNetMethodAsync(instance);
            WindowAnimation.cancelAnimation((_b = (_a = ProgressiveAudioUtility.playAnimation) === null || _a === void 0 ? void 0 : _a.id) !== null && _b !== void 0 ? _b : undefined);
            console.info('stopPlayAnimationAsync', {
                readyState: audio === null || audio === void 0 ? void 0 : audio.readyState,
                paused: audio === null || audio === void 0 ? void 0 : audio.paused,
                currentTime: audio === null || audio === void 0 ? void 0 : audio.currentTime,
                hasSetCurrentTime: audio === null || audio === void 0 ? void 0 : audio.dataset.hasSetCurrentTime,
                audio
            });
        });
    }
}
ProgressiveAudioUtility.playAnimation = null;
//# sourceMappingURL=progressive-audio-utility.js.map