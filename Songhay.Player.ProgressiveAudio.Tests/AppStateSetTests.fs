module Songhay.Modules.Models.Tests.AppStateSetTests

open Xunit

open Songhay.Modules.Models

type MyAppState = | StateOne | StateTwo | StateThree

[<Fact>]
let ``hasState test`` () =

    let actual = AppStateSet<MyAppState>.initialize.addStates(StateOne, StateThree)

    actual.hasState StateOne |> Assert.True
    actual.hasState StateTwo |> Assert.False
    actual.hasState StateThree |> Assert.True

[<Fact>]
let ``removeStates test`` () =

    let actual = AppStateSet<MyAppState>
                     .initialize
                     .addStates(StateOne, StateTwo, StateThree)
                     .removeStates(StateTwo, StateThree)

    actual.hasState StateOne |> Assert.True
    actual.hasState StateTwo |> Assert.False
    actual.hasState StateThree |> Assert.False


[<Fact>]
let ``toggleState test`` () =

    let actual = AppStateSet<MyAppState>
                     .initialize
                     .addStates(StateOne, StateTwo, StateThree)
                     .toggleState(StateTwo)

    actual.hasState StateTwo |> Assert.False

    Assert.Equal(2, actual.states.Count)

    let actual = actual.toggleState(StateTwo)

    actual.hasState StateTwo |> Assert.True

    Assert.Equal(3, actual.states.Count)
