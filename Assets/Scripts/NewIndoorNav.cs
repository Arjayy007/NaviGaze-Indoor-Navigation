using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using System.Collections;

public class NewIndoorNav : MonoBehaviour
{
    [SerializeField] private Transform player; // AR camera representing the player
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private GameObject trackedImagePrefab;
    [SerializeField] private LineRenderer line; // Line Renderer for navigation path
    [SerializeField] private TMP_Dropdown dropdown; // Destination selector
    [SerializeField] private GameObject infoPanel; // UI panel for arrival confirmation
    [SerializeField] private TMP_Text startingPoint;
    [SerializeField] private TMP_Text destinationPoint;
    [SerializeField] private TMP_Text estimatedDistance;
    [SerializeField] private TMP_Text estimatedTime;
    [SerializeField] private GameObject estimatedArrivalTimeAndDistancePanel;
    [SerializeField] private GameObject navigationPanel; // panel sa taas ng screen 
    [SerializeField] private TMP_Text destinationRoom; // sa navigation panel sa taas
    [SerializeField] private Button closeButton; // sa navigation panel sa taas
    [SerializeField] private TMP_Text arrivedText;
    public GameObject slideUpPanel;
    public ClassNavigationManager classNavigationManager;

    [SerializeField] private GameObject character; // Assign your character in the Inspector
    private NavMeshAgent characterAgent;
    private Animator characterAnimator;


    private List<GameObject> navigationTargets = new List<GameObject>(); // List of all target locations
    private NavMeshPath navMeshPath;

    private bool isQRCodeScanned = false;
    private bool hasSavedToDatabase = false;
    private string selectedRole;

    private void Start()
{
    navMeshPath = new NavMeshPath(); 
    Screen.sleepTimeout = SleepTimeout.NeverSleep;

    navigationTargets = GameObject.FindGameObjectsWithTag("Target").ToList();
    PopulateDropdown();

    dropdown.onValueChanged.AddListener(OnDropdownValueChanged);

    characterAgent = character.GetComponent<NavMeshAgent>();
    characterAnimator = character.GetComponent<Animator>();
    characterAnimator.Play("Breathing");
    selectedRole = PlayerPrefs.GetString("SelectedRole", "");

    // ✅ Set custom XR Origin start position
    Transform xrOrigin = player.transform.parent;
    if (xrOrigin != null)
    {
        xrOrigin.position = new Vector3(0f, 0.87f, 1.50f); // your desired starting position
        Debug.Log("XR Origin manually positioned at (0, 0.87, 1.68)");
    }
}

    private void Update()
{
 
}


    private void OnEnable()
    {
        if (m_TrackedImageManager != null)
            m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        if (m_TrackedImageManager != null)
            m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added.Concat(args.updated))
        {
            string qrCodeName = trackedImage.referenceImage.name;

            // Directly update position and navigation
            SetPlayerPositionFromQRCode(qrCodeName);
        }
    }


private void SetPlayerPositionFromQRCode(string qrCodeName)
{
    GameObject targetCube = navigationTargets.FirstOrDefault(target => target.name == qrCodeName);

    if (targetCube != null)
    {
        Vector3 targetPosition = targetCube.transform.position;

        // Get XR Origin (parent of Camera Offset)
        Transform xrOrigin = player.transform.parent; 
        if (xrOrigin != null)
        {
            // Move XR Origin instead of just the camera
            xrOrigin.position = targetPosition;

            // Preserve camera's original upright rotation
            Quaternion targetRotation = Quaternion.Euler(0, targetCube.transform.eulerAngles.y, 0);
            xrOrigin.rotation = targetRotation;

            Debug.Log($"XR Origin repositioned to {qrCodeName} at {targetPosition}");
        }

            characterAgent.Warp(targetPosition);
            characterAgent.ResetPath();

            // Update navigation and history
            if (targetCube.name == destinationPoint.text)
        {
            if (!hasSavedToDatabase)
            {
                    if (selectedRole == "Student" || selectedRole == "Professor")
                    {   
                        GetEstimatedArrival();
                        arrivedText.text = "Arrived";
                        classNavigationManager.CheckForClassNavigation(startingPoint.text, destinationPoint.text);
                        hasSavedToDatabase = true;
                    }else{
                        GetEstimatedArrival();
                        arrivedText.text = "Arrived";
                    }
                }
        }
        else
        {
            startingPoint.text = qrCodeName;
            UpdateLineRenderer();

            if (!isQRCodeScanned)
            {
                openHistory();
                isQRCodeScanned = true;
            }
        }
    }
    else
    {
        Debug.LogWarning($"No matching target found for QR Code: {qrCodeName}");
    }
}




    private void PopulateDropdown()
    {
        dropdown.options.Clear();

        // Add default option first
        dropdown.options.Add(new TMP_Dropdown.OptionData("-Select Destination"));

        // Sort targets alphabetically
        navigationTargets = navigationTargets.OrderBy(target => target.name).ToList();

        // Add actual target destinations
        foreach (var target in navigationTargets)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(target.name));
        }

        dropdown.RefreshShownValue();

        // Set default value to "-Select Destination"
        dropdown.value = 0;
        dropdown.captionText.text = dropdown.options[0].text;

        // Ensure no navigation line is drawn initially
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        if (dropdown.value == 0)
        {
            // If "-Select Destination" is chosen, don't render the line
            line.positionCount = 0;
            return;
        }

        string selectedTargetName = dropdown.options[dropdown.value].text;
        GameObject selectedTarget = navigationTargets.FirstOrDefault(target => target.name == selectedTargetName);

        if (selectedTarget != null)
        {
            // Always calculate from the latest player position
            NavMesh.CalculatePath(player.position, selectedTarget.transform.position, NavMesh.AllAreas, navMeshPath);

            if (navMeshPath.status == NavMeshPathStatus.PathComplete)
            {
                line.positionCount = navMeshPath.corners.Length;
                line.SetPositions(navMeshPath.corners);
            }
            else
            {
                line.positionCount = 0;
            }
        }
        else
        {
            Debug.LogWarning($"No valid target found for {selectedTargetName}");
            line.positionCount = 0;
        }
    }


    private void OnDropdownValueChanged(int index)
    {
        character.SetActive(true);
        UpdateLineRenderer(); // Changing target updates only the destination
        destinationPoint.text = dropdown.options[dropdown.value].text;
        destinationRoom.text = dropdown.options[dropdown.value].text;
    }
    public void CloseHistoryPanel()
    {
        GetEstimatedArrival();
        destinationRoom.text = dropdown.options[dropdown.value].text;
        ToggleHistoryPanel(false);
        estimatedArrivalTimeAndDistancePanel.SetActive(true);
        dropdown.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
        navigationPanel.SetActive(true);
         MoveCharacterToDestination();
    }

    public (float distance, float time) GetEstimatedArrival()
    {
        float totalDistance = 0f;
        float walkingSpeed = 1.4f; // Average walking speed in meters per second

        for (int i = 1; i < navMeshPath.corners.Length; i++)
        {
            totalDistance += Vector3.Distance(navMeshPath.corners[i - 1], navMeshPath.corners[i]);
        }

        float calculateEstimatedTime = totalDistance / walkingSpeed;

        // Convert to whole numbers
        int roundedDistance = Mathf.RoundToInt(totalDistance);
        int roundedTime = Mathf.RoundToInt(calculateEstimatedTime);

        // Update UI with whole numbers
        estimatedDistance.text = $"{roundedDistance} meters";
        estimatedTime.text = $"{roundedTime} seconds";

        return (roundedDistance, roundedTime);
    }


    public void openHistory()
    {
        ToggleHistoryPanel(true);
    }
    private void ToggleHistoryPanel(bool open)
    {
        Animator animator = slideUpPanel.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("open", open);
        }
    }

    public void CancelNavigation()
    {
        // Reset the dropdown to default
        dropdown.value = 0;
        dropdown.captionText.text = dropdown.options[0].text;

        // Clear the line renderer
        line.positionCount = 0;

        // Hide navigation-related UI panels
        navigationPanel.SetActive(false);
        estimatedArrivalTimeAndDistancePanel.SetActive(false);
        slideUpPanel.SetActive(false);
        infoPanel.SetActive(false);
        dropdown.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);

        // Reset estimated distance and time display
        estimatedDistance.text = "";
        estimatedTime.text = "";

        // Reset QR scan flags
        isQRCodeScanned = false;
        hasSavedToDatabase = false;

        // Clear starting and destination texts
        startingPoint.text = "";
        destinationPoint.text = "";
        destinationRoom.text = "";
        arrivedText.text = "Navigating";

        // Reset character movement
        if (characterAgent != null)
        {
            characterAgent.ResetPath(); // Stop the NavMeshAgent
            characterAgent.Warp(player.position); // Move character back to player�s position
        }

        // Reset character animation to idle
        if (characterAnimator != null)
        {
            characterAnimator.ResetTrigger("StartWalking");
            characterAnimator.Play("Breathing"); // Idle animation
        }

        // Reset QR tracking (if needed)
        if (m_TrackedImageManager != null)
        {
            foreach (var trackedImage in m_TrackedImageManager.trackables)
            {
                trackedImage.gameObject.SetActive(false); // Hide tracked images
            }
        }

        Debug.Log("Navigation has been fully reset!");
    }

    //testingggggggggg
    private void MoveCharacterToDestination()
{
    if (dropdown.value == 0)
    {
        Debug.LogWarning("No destination selected!");
        return;
    }

    string selectedTargetName = dropdown.options[dropdown.value].text;
    GameObject destinationCube = navigationTargets.FirstOrDefault(target => target.name == selectedTargetName);

    if (destinationCube != null)
    {
        characterAgent.SetDestination(destinationCube.transform.position);
        characterAnimator.SetTrigger("StartWalking");

        FaceCameraToPathDirection(); // 👈 Face the camera to the direction of the line
        StartCoroutine(checkIfCharacterArrived());
    }
    else
    {
        Debug.LogWarning("Destination cube not found!");
    }
}

private void FaceCameraToPathDirection()
{
    if (navMeshPath.corners.Length >= 2)
    {
        // Use characterAgent position as the starting point
        Vector3 direction = navMeshPath.corners[1] - characterAgent.transform.position;
        direction.y = 0; // Flatten the direction

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            // Rotate the XR Origin so the camera faces the path
            Transform xrOrigin = Camera.main.transform.parent; // Or player.transform.parent if player is the camera
            if (xrOrigin != null)
            {
                xrOrigin.rotation = lookRotation;
                Debug.Log("Camera rotated to face path direction: " + lookRotation.eulerAngles);
            }

            Debug.DrawLine(characterAgent.transform.position, navMeshPath.corners[1], Color.green, 3f);
        }
    }
}


    private IEnumerator checkIfCharacterArrived() {
        yield return new WaitUntil(() => characterAgent.remainingDistance <= characterAgent.stoppingDistance && !characterAgent.pathPending);
        characterAnimator.ResetTrigger("StartWalking");
        characterAnimator.Play("Breathing");
    }

}