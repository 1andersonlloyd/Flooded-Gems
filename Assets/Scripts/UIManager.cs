using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public static Canvas Canvas;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        Canvas = GetComponent<Canvas>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
