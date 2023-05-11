namespace Songhay.Modules.Bolero.Visuals.Bulma

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma

module Component =

    /// <summary>
    /// “A classic modal overlay, in which you can include any content you want…”
    /// </summary>
    /// <remarks>
    /// 📖 https://bulma.io/documentation/components/modal/
    /// </remarks>
    let bulmaModalContainer
        (moreClasses: CssClassesOrEmpty)
        (modalBackgroundAttributes: HtmlAttributeOrEmpty)
        (isModalCloseVisible: bool)
        (isModalVisible: bool)
        (contentNode: Node) =
        div {
            CssClasses [ "modal"; if isModalVisible then CssClass.elementIsActive ] |> moreClasses.ToHtmlClassAttribute

            div {
                [ "modal-background" ] |> CssClasses.toHtmlClassFromList

                modalBackgroundAttributes.Value
            }
            div {
                [ "modal-content" ] |> CssClasses.toHtmlClassFromList

                contentNode

            }
            cond isModalCloseVisible <| function
                | true ->
                    button {
                        [ "modal-close"; "is-large" ] |> CssClasses.toHtmlClassFromList
                    }
                | false ->
                    empty()
        }
