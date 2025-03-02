using UnityEngine;
using UnityEngine.InputSystem; // For new Input System

public class RotateCharacter : MonoBehaviour
{
    public float rotationSpeed = 100f;
    private Vector2 lastTouchPosition;

    void Update()
    {
        Vector2 rotationDelta = Vector2.zero;

        // Handle Touch Input (Mobile)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            rotationDelta = Touchscreen.current.primaryTouch.delta.ReadValue();
        }

        // Handle Mouse Input (for Editor Testing)
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            rotationDelta = Mouse.current.delta.ReadValue();
        }

        // Rotate on Y-axis (horizontal swipe)
        float rotateY = rotationDelta.x * rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, -rotateY, Space.World);
    }
}
