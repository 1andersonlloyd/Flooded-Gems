using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hiddenYPosition;
    public float visibleYPosition;
    public Vector2 displayVector;
    public float moveSpeed = 10f;
    protected Canvas canvas;
    protected RectTransform rectTransform;

  
    public float targetXPosition;
    public float targetYPosition;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        Canvas.ForceUpdateCanvases();
        InitializeValues();
    }

    public void InitializeValues()
    {
        float height = rectTransform.rect.height * rectTransform.lossyScale.y;
        displayVector = new Vector2(0, height / canvas.scaleFactor * 0.4f);

        hiddenYPosition = rectTransform.anchoredPosition.y;
        targetYPosition = hiddenYPosition;
        visibleYPosition = hiddenYPosition + displayVector.y;
    }


    void Update()
    {
        rectTransform.anchoredPosition = Vector2.Lerp(
         rectTransform.anchoredPosition, new Vector2(rectTransform.anchoredPosition.x, targetYPosition), Time.deltaTime * moveSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("hovered item card");
        Show();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Hide();
    }

    public virtual void Hide()
    {
        targetYPosition = hiddenYPosition;
    }

    public virtual void Show()
    {
        targetYPosition = visibleYPosition;
    }


    public void OnClickP()
    {
        Debug.Log("Card Clicked");
    }


}
