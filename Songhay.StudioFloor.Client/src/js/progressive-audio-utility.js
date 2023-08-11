import { __awaiter } from "tslib";
import { WindowAnimation } from 'songhay';
export class ProgressiveAudioUtility {
    static getHTMLAudioElement() {
        return window.document.querySelector('#audio-player-container>audio');
    }
    static getPlayPauseButtonElement() {
        return window.document.querySelector('#play-pause-block>button');
    }
    static getPlayPauseInputElement() {
        return window.document.querySelector('#play-pause-block>input');
    }
    // noinspection JSUnusedGlobalSymbols
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
    // noinspection JSUnusedGlobalSymbols
    static loadAudioTrack(src) {
        const audio = ProgressiveAudioUtility.getHTMLAudioElement();
        audio === null || audio === void 0 ? void 0 : audio.setAttribute('src', src);
        audio === null || audio === void 0 ? void 0 : audio.load();
    }
    // noinspection JSUnusedGlobalSymbols
    static startPlayAnimation(instance) {
        const button = ProgressiveAudioUtility.getPlayPauseButtonElement();
        const audio = ProgressiveAudioUtility.getHTMLAudioElement();
        const fps = 1; // frames per second
        const readyStatePollFreq = 250; // milliseconds
        if (button) {
            button.disabled = true;
        }
        if (!ProgressiveAudioUtility.isInputEventingApplied) {
            const input = ProgressiveAudioUtility.getPlayPauseInputElement();
            input === null || input === void 0 ? void 0 : input.addEventListener('change', () => {
                const audio = ProgressiveAudioUtility.getHTMLAudioElement();
                if (audio) {
                    console.warn({ input });
                    audio.currentTime = parseFloat(input === null || input === void 0 ? void 0 : input.value);
                }
            });
        }
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
                yield ProgressiveAudioUtility.stopPlayAnimationAsync(instance);
                return;
            }
            yield ProgressiveAudioUtility.invokeDotNetMethodAsync(instance, audio);
        }));
        WindowAnimation.animate();
        if (button) {
            button.disabled = false;
        }
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
ProgressiveAudioUtility.isInputEventingApplied = false;
ProgressiveAudioUtility.playAnimation = null;
//# sourceMappingURL=progressive-audio-utility.js.map