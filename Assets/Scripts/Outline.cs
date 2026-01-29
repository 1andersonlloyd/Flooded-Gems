using UnityEngine;

public class Outline : MonoBehaviour
{
    public PolygonCollider2D polyCollider;
    public LineRenderer lineRenderer;
    public Color outlineColor = Color.white;
    public float outlineWidth = 0.3f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer.positionCount = polyCollider.points.Length + 1;
        Vector2[] points = polyCollider.points;

        for (int i = 0; i < points.Length; i++)
        {
            // Convert local points to world points
            Vector3 worldPoint = transform.TransformPoint(points[i]);
            lineRenderer.SetPosition(i, worldPoint);
        }
        // Close the loop
        lineRenderer.SetPosition(points.Length, transform.TransformPoint(points[0]));

        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = true;

        // Configure line appearance here (color, width, material)
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.pink;
        lineRenderer.endColor = Color.pink;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
