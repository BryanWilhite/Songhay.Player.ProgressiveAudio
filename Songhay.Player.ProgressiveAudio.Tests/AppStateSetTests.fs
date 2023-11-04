module Songhay.Modules.Models.Tests.AppStateSetTests

open Xunit
open FsUnit.Xunit

open Songhay.Modules.Models

type MyAppState = | StateOne | StateTwo | StateThree

[<Fact>]
let ``hasState test`` () =

    let actual = AppStateSet<MyAppState>.initialize.addStates(StateOne, StateThree)

    actual.hasState StateOne |> should be True
    actual.hasState StateTwo |> should be False
    actual.hasState StateThree |> should be True

[<Fact>]
let ``removeStates test`` () =

    let actual = AppStateSet<MyAppState>
                     .initialize
                     .addStates(StateOne, StateTwo, StateThree)
                     .removeStates(StateTwo, StateThree)

    actual.hasState StateOne |> should be True
    actual.hasState StateTwo |> should be False
    actual.hasState StateThree |> should be False


[<Fact>]
let ``toggleState test`` () =

    let actual = AppStateSet<MyAppState>
                     .initialize
                     .addStates(StateOne, StateTwo, StateThree)
                     .toggleState(StateTwo)

    actual.hasState StateTwo |> should be False

    actual.states.Count |> should equal 2

    let actual = actual.toggleState(StateTwo)

    actual.hasState StateTwo |> should be True

    actual.states.Count |> should equal 3
