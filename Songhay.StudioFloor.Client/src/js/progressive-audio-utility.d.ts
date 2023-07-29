/// <reference types="blazor__javascript-interop" />
import { WindowAnimation } from 'songhay';
export declare class ProgressiveAudioUtility {
    static getHTMLAudioElement(): HTMLAudioElement | null;
    static playAnimation: WindowAnimation | null;
    static startPlayAnimation(instance: DotNet.DotNetObject): void;
    static stopPlayAnimation(): void;
}
