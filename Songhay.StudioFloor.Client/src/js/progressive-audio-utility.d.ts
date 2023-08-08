/// <reference types="blazor__javascript-interop" />
import { WindowAnimation } from 'songhay';
export declare class ProgressiveAudioUtility {
    static getHTMLAudioElement(): HTMLAudioElement | null;
    static getPlayPauseButtonElement(): HTMLButtonElement | null;
    static playAnimation: WindowAnimation | null;
    static handleAudioMetadataLoadedAsync(instance: DotNet.DotNetObject): Promise<void>;
    static invokeDotNetMethodAsync(instance: DotNet.DotNetObject, audio: HTMLAudioElement | null): Promise<void>;
    static loadAudioTrack(src: string): void;
    static startPlayAnimationAsync(instance: DotNet.DotNetObject): Promise<void>;
    static stopPlayAnimationAsync(instance: DotNet.DotNetObject): Promise<void>;
}
