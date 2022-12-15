namespace Songhay.StudioFloor.Client.ElmishTypes

type Tab =
    | ReadMeTab

type Model = {
    readMeData: string option
    tab: Tab
}

type Message =
    | Error of exn
    | GetReadMe | GotReadMe of string
    | SetTab of Tab
