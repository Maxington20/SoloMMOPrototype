using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatBreakdownTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string title;
    private string details;

    private void Awake()
    {
        Graphic graphic = GetComponent<Graphic>();
        if (graphic != null)
        {
            graphic.raycastTarget = true;
        }
    }

    public void SetTooltip(string newTitle, string newDetails)
    {
        title = newTitle;
        details = newDetails;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        StatBreakdownTooltipUI.Instance?.Show(title, details);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StatBreakdownTooltipUI.Instance?.Hide();
    }
}