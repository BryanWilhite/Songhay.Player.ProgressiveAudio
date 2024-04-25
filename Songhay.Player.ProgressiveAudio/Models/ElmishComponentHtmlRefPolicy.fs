namespace Songhay.Modules.Bolero.Models

/// <summary>
/// defines Elmish <see cref="HtmlRef"/> <c>dispatch</c> policies
/// </summary>
type ElmishComponentHtmlRefPolicy =
    /// <summary> <c>dispatch</c> every time <see cref="ElmishComponent.View"/> is called</summary>
    | DispatchForEveryView
    /// <summary> <c>dispatch</c> once when <see cref="ElmishComponent.View"/> is called</summary>
    | DispatchOnce
    /// <summary> do not <c>dispatch</c> when <see cref="ElmishComponent.View"/> is called</summary>
    | DoNotDispatch
