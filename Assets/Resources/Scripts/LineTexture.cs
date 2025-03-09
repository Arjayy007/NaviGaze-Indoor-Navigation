using UnityEngine;

public class LineTexture : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float scrollSpeed = 1f; // Adjust speed as needed

    private Material lineMaterial;

    void Start()
    {
        // Get the material from the Line Renderer
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
            lineMaterial = lineRenderer.material;
    }

    void Update()
    {
        if (lineMaterial != null)
        {
            // Scroll the texture horizontally
            Vector2 offset = lineMaterial.mainTextureOffset;
            offset.x += Time.deltaTime * scrollSpeed;
            lineMaterial.mainTextureOffset = offset;
        }
    }
}
