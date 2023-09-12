namespace Songhay.Player.ProgressiveAudio.Tests

open Xunit
open Xunit.Abstractions
open FsUnit.CustomMatchers
open FsUnit.Xunit

open Songhay.Player.ProgressiveAudio.ProgressiveAudioPresentationUtility

type ProgressiveAudioUtilityTests(outputHelper: ITestOutputHelper) =

    [<Theory>]
    [<InlineData("http://localhost:5000/#default", "default")>]
    [<InlineData("http://localhost:5000/b-roll/audio#default", "default")>]
    [<InlineData("", null)>]
    let ``toUriFragmentOption test`` (location: string, expectedValue: string) =
        let actual = toUriFragmentOption location
        if expectedValue = null then
            actual |> should be (ofCase <@ Option<string>.None @>)
            outputHelper.WriteLine $"{nameof location}: `{location}`"
            outputHelper.WriteLine $"    {nameof expectedValue}: null"
            outputHelper.WriteLine $"    {nameof actual}: {nameof None}"
        else
            actual |> should be (ofCase <@ Option<string>.Some @>)
            actual.Value |> should equal expectedValue
            outputHelper.WriteLine $"{nameof location}: `{location}`"
            outputHelper.WriteLine $"    {nameof expectedValue}: `{expectedValue}`"
            outputHelper.WriteLine $"    {nameof actual}: `{actual.Value}`"
