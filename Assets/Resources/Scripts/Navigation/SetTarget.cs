using UnityEngine;
using UnityEngine.AI;

public class SetTarget : MonoBehaviour
{
    [SerializeField]
    private Camera topdownCamera;
     [SerializeField]
     private GameObject target;
     private NavMeshPath path;
     private LineRenderer lineRenderer;

     private bool lineToggle = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        path = new NavMeshPath();
        lineRenderer = transform.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
        {
           lineToggle = !lineToggle;
        }
        if (lineToggle){
            NavMesh.CalculatePath(transform.position, target.transform.position, NavMesh.AllAreas, path);
            lineRenderer.positionCount = path.corners.Length;
            lineRenderer.SetPositions(path.corners);
            lineRenderer.enabled = true;
        }
        
    }
}
