using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    Vector3 dragOrigin;
    bool isDragging;
    public GameObject map;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Application.targetFrameRate = 60; // or 144, 30, etc.
        transform.position = new Vector3(0, 0, -10);
    }

    // Update is called once per frame
    void Update()
    {
        // Panning
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            dragOrigin = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            isDragging = true;
        }
        if (Mouse.current.middleButton.isPressed && isDragging)
        {
            Vector3 currentPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector3 difference = dragOrigin - currentPos;
            transform.position += difference;
        }
        if (Mouse.current.middleButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -20, 20), Mathf.Clamp(transform.position.y, -10, 10), -10);

        // Zooming
        if (Mouse.current.scroll.ReadValue().y > 0)
        {
            Camera.main.orthographicSize -= 1.0f;
        }
        if (Mouse.current.scroll.ReadValue().y < 0)
        {
            Camera.main.orthographicSize += 1.0f;
        }
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3.5f, 20f);

    }
}
