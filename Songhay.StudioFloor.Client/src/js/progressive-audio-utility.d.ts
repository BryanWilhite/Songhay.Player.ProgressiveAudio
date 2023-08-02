/// <reference types="blazor__javascript-interop" />
import { WindowAnimation } from 'songhay';
export declare class ProgressiveAudioUtility {
    static getHTMLAudioElement(): HTMLAudioElement | null;
    static playAnimation: WindowAnimation | null;
    static invokeDotNetMethodAsync(instance: DotNet.DotNetObject, audio: HTMLAudioElement | null): Promise<void>;
    static loadAudioTrackAsync(instance: DotNet.DotNetObject): Promise<void>;
    static startPlayAnimation(instance: DotNet.DotNetObject): void;
    static stopPlayAnimationAsync(instance: DotNet.DotNetObject): Promise<void>;
}
