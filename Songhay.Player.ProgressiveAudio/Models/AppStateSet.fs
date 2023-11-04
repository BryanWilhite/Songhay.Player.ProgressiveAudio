namespace Songhay.Modules.Models

open System

/// <summary>defines the <see cref="'state"/> collection, naming all states off the app</summary>
type AppStateSet<'state when 'state : comparison> =
    /// <summary>the <see cref="'state"/> collection</summary>
    | AppStates of Set<'state>

    /// <summary>returns an empty collection of <see cref="'state"/></summary>
    static member initialize = AppStates Set.empty<'state>


    /// <summary>adds the specified <see cref="'state"/></summary>
    member this.addState state = AppStates <| this.states.Add(state)

    /// <summary>adds the specified list of <see cref="'state"/></summary>
    member this.addStates ([<ParamArray>]states :'state[]) =
        let oldStates = this.states |> Array.ofSeq
        let newSet = (states |> Array.append oldStates) |> Set.ofArray

        AppStates newSet

    /// <summary>returns <c>true</c> when the specified <see cref="'state"/> is collected</summary>
    member this.hasState state = this.states.Contains(state)

    /// <summary>removes the specified <see cref="'state"/></summary>
    member this.removeState state = AppStates <| this.states.Remove(state)

    /// <summary>removes the specified list of <see cref="'state"/></summary>
    member this.removeStates ([<ParamArray>]states :'state[]) =
        let newSet = (states |> Set.ofArray) |> Set.difference this.states

        AppStates newSet

    /// <summary>returns the underlying <see cref="Set{T}"/></summary>
    member this.states: Set<'state> = let (AppStates set) = this in set

    /// <summary>toggles the specified <see cref="'state"/></summary>
    member this.toggleState state =
        if this.hasState state then this.removeState state
        else this.addState state
