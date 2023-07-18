import { __awaiter } from "tslib";
import { WindowAnimation } from 'songhay';
export class ProgressiveAudioUtility {
    static startPlayAnimation(instance) {
        console.warn({ instance });
        ProgressiveAudioUtility.playAnimation = WindowAnimation.registerAndGenerate(1, (_) => __awaiter(this, void 0, void 0, function* () {
            var _a;
            try {
                yield instance.invokeMethodAsync('startAsync', null);
                console.info((_a = ProgressiveAudioUtility.playAnimation) === null || _a === void 0 ? void 0 : _a.getDiagnosticStatus());
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
        WindowAnimation.cancelAnimation((_b = (_a = ProgressiveAudioUtility.playAnimation) === null || _a === void 0 ? void 0 : _a.id) !== null && _b !== void 0 ? _b : undefined);
    }
}
ProgressiveAudioUtility.playAnimation = null;
//# sourceMappingURL=progressive-audio-utility.js.map