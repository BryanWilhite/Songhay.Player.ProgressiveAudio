/// <reference types="blazor__javascript-interop" />
import { WindowAnimation } from 'songhay';
export declare class ProgressiveAudioUtility {
    static playAnimation: WindowAnimation | null;
    static getCurrentAudioElement(): HTMLAudioElement | null;
    static handleAudioMetadataLoadedAsync(instance: DotNet.DotNetObject): Promise<void>;
    static invokeDotNetMethodAsync(instance: DotNet.DotNetObject): Promise<void>;
    static setAudioCurrentTime(input: HTMLInputElement | null): void;
    static startPlayAnimationAsync(instance: DotNet.DotNetObject): Promise<void>;
    static stopPlayAnimationAsync(instance: DotNet.DotNetObject): Promise<void>;
}
