namespace Songhay.Modules.Bolero.Models

/// <summary>
/// defines Elmish <see cref="HtmlRef"/> <c>dispatch</c> policies
/// </summary>
type ElmishComponentHtmlRefPolicy =
    /// <summary> <c>dispatch</c> when <see cref="ElmishComponent.View"/> is called</summary>
    | DispatchForEveryView
    /// <summary> <c>dispatch</c> conditionally when <see cref="ElmishComponent.View"/> is called</summary>
    /// <remarks>use a <c>when</c> guard to call <c>dispatch</c> conditionally in a <c>match</c> expression</remarks>
    | DispatchConditionally
    /// <summary> do not <c>dispatch</c> when <see cref="ElmishComponent.View"/> is called</summary>
    | DoNotDispatch
