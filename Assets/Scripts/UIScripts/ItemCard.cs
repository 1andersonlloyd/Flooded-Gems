using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemCard : SlidingBar, IPointerEnterHandler, IPointerExitHandler
{
    public override void InitializeValues()
    {
        float height = rectTransform.rect.height * rectTransform.lossyScale.y;
        displayVector = new Vector2(0, height / canvas.scaleFactor * 0.4f);
        base.InitializeValues();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Show();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Hide();
    }
}
