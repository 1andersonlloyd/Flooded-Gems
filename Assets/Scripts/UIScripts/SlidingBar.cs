using UnityEngine;

public class SlidingBar : MonoBehaviour
{

    public bool isHiddenByDefault = true;
    protected Vector2 hiddenPosition;
    protected Vector2 visiblePosition;
    public Vector2 displayVector;
    public float moveSpeed = 10f;
    protected Canvas canvas;

    protected RectTransform rectTransform;
    protected Vector2 targetPosition;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        Canvas.ForceUpdateCanvases();
        InitializeValues();
    }
    public virtual void InitializeValues()
    {
        if (isHiddenByDefault)
        {
            hiddenPosition = rectTransform.anchoredPosition;
            targetPosition = hiddenPosition;
            visiblePosition = hiddenPosition + displayVector;
        }
        else
        {
            visiblePosition = rectTransform.anchoredPosition;
            targetPosition = visiblePosition;
            hiddenPosition = visiblePosition - displayVector;
        }
    }

    // Update is called once per frame
    void Update()
    {
        rectTransform.anchoredPosition = Vector2.Lerp(
         rectTransform.anchoredPosition, targetPosition, Time.deltaTime * moveSpeed);
    }

    public virtual void Hide()
    {
        targetPosition = hiddenPosition;
    }

    public virtual void Show()
    {
        targetPosition = visiblePosition;
    }
}
