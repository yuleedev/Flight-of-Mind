using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneDragger : MonoBehaviour
{
    public Camera cam;
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
            transform.position = mousePos;
            TrailMakingManager.Instance.Draw(mousePos);
        }
    }
}