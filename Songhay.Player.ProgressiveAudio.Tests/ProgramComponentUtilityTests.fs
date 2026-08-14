namespace Songhay.Player.ProgressiveAudio.Tests

open System
open Microsoft.Extensions.Logging
open Xunit
open Xunit.Abstractions

open FsToolkit.ErrorHandling

open Songhay.Modules
open Songhay.Modules.Models

type ProgramComponentUtilityTests(outputHelper: ITestOutputHelper) =

    /// <summary>
    /// An <c>option</c> wrapper for <see cref="RestApiMetadata.ToUriResultFromClaim"/>.
    /// </summary>
    member this.GetUriResultFromClaim(metaOption: RestApiMetadata option, key: string, logger: ILogger, [<ParamArray>] args: string[]) =
        metaOption
        |> Option.either
            (
                fun restApiMetadata ->
                    restApiMetadata.ToUriResultFromClaim(key, args)
                    |> Result.teeError logger.LogException
            )
            (fun () -> Error <| exn $"The expected {nameof RestApiMetadata} is not here.")

    [<Theory>]
    [<InlineData("PlayerApi", "cdn-route-for-manifest", "default")>]
    member this.``tryDownloadToStringAsync request test (async)`` (metaKey: string, claimSetKey: string, presentationKey: string) =
        let metaOption =
                    metaKey
                    |> RestApiMetadata.fromConfiguration studioFloorConfiguration
                    |> RestApiMetadata.toRestApiMetadataOption nullLogger.LogException

        let uriResult = this.GetUriResultFromClaim(metaOption, claimSetKey, nullLogger, presentationKey)

        uriResult
        |> Result.tee (fun uri -> outputHelper.WriteLine $"{uri}")
        |> Result.teeError (fun exn -> outputHelper.WriteLine $"ERROR: {exn.Message}")
        |> _.IsOk |> Assert.True
