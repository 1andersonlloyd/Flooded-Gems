using UnityEngine;

public class ButtonBar : SlidingBar
{

    public override void InitializeValues()
    {
        RectTransform rect = GetComponent<RectTransform>();
        float width = rect.rect.width * rect.lossyScale.y;
        displayVector = new Vector2(-width / canvas.scaleFactor * 1.5f, 0);

        base.InitializeValues();
    }



}
