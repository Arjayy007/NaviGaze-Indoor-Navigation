using UnityEngine;

public class LockCameraPosition : MonoBehaviour
{
    public Transform navMeshCenter; // Assign the center of your 3D campus (NavMesh)

    void Update()
    {
        if (navMeshCenter != null)
        {
            // Lock the camera to the center of the NavMesh
            transform.position = navMeshCenter.position;
        }

        // Allow rotation (user can look around)
    }
}
