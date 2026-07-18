using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneDragger : MonoBehaviour
{
    public Camera cam;

    [Header("Bounds")]
    public Collider2D playArea;
    public float padding = 0f;

    Collider2D col;
    bool drawing;

    void Start()
    {
        col = GetComponent<Collider2D>();
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        Vector3 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (col.OverlapPoint(mousePos))
            {
                drawing = true;
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            drawing = false;
        }

        if (drawing)
        {
            Vector3 clamped = ClampToBounds(mousePos);
            transform.position = clamped;
            TrailMakingManager.Instance.Draw(clamped);
        }
    }

    Vector3 ClampToBounds(Vector3 p)
    {
        Bounds area;
        if (playArea != null)
        {
            area = playArea.bounds;
        }
        else
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            Vector3 c = cam.transform.position;
            area = new Bounds(new Vector3(c.x, c.y, 0f), new Vector3(halfW * 2f, halfH * 2f, 0f));
        }

        Vector3 ext = col != null ? col.bounds.extents : Vector3.zero;

        float minX = area.min.x + ext.x + padding;
        float maxX = area.max.x - ext.x - padding;
        float minY = area.min.y + ext.y + padding;
        float maxY = area.max.y - ext.y - padding;

        if (minX > maxX) minX = maxX = area.center.x;
        if (minY > maxY) minY = maxY = area.center.y;

        p.x = Mathf.Clamp(p.x, minX, maxX);
        p.y = Mathf.Clamp(p.y, minY, maxY);
        return p;
    }
}
