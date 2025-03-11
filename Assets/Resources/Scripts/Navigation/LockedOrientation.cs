using UnityEngine;

public class LockedOrientation : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // Keep the environment locked
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}

