using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using TMPro; // Import TextMeshPro namespace
using UnityEngine.XR.ARFoundation;

public class NewIndoorNav : MonoBehaviour {
    [SerializeField] private Transform player; // Reference to the player (e.g., AR camera)
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private GameObject trackedImagePrefab;
    [SerializeField] private LineRenderer line; // Line Renderer component
    [SerializeField] private TMP_Dropdown dropdown; // TextMeshPro Dropdown for selecting targets
    [SerializeField] private GameObject infoPanel; // Reference to the UI panel
    [SerializeField] private GameObject ETAPanel; // Reference to the UI panel
    [SerializeField] private SlideUpAnimation slideUpAnimation;
    [SerializeField] private TMP_Text startingpoint;
    [SerializeField] private TMP_Text destination;
    [SerializeField] private TMP_Text estimatedTimeText; // TextMeshPro text for estimated time
    [SerializeField] private TMP_Text distanceText;



    private List<GameObject> navigationTargets = new List<GameObject>(); // All target cubes
    private NavMeshSurface navMeshSurface;
    private NavMeshPath navMeshPath;
    private GameObject navigationBase;
    private bool playerHasScanned = false; // Track if the player has scanned a QR code
    private CoinManager coinManager;    // Reference to the CoinManager script
    private bool hasSetStartingPoint = false;
    private float distanceTraveled = 0f;  // Track the distance the player has moved
    private float startTime = 0f;          // Track the time when the navigation starts
    

    private void Start() {
        navMeshPath = new NavMeshPath();

        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Find all target cubes in the scene
        navigationTargets = GameObject.FindGameObjectsWithTag("Target").ToList();

        if (navigationTargets.Count == 0) {
            Debug.LogWarning("No targets found with the 'Target' tag!");
        }

        // Populate the dropdown with the names of the target cubes
        PopulateDropdown();

        // Listen to dropdown selection changes
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        
        coinManager = FindAnyObjectByType<CoinManager>();
    }

    private void Update() {
        // Only update navigation if the player has scanned a QR and a valid target is selected
        if (playerHasScanned && navigationTargets.Count > 0 && dropdown.options.Count > 0 && dropdown.value != 0) {
            GameObject selectedTarget = navigationTargets.FirstOrDefault(
                target => target.name == dropdown.options[dropdown.value].text
            );

            if (selectedTarget != null) {
                NavMesh.CalculatePath(player.position, selectedTarget.transform.position, NavMesh.AllAreas, navMeshPath);

                if (navMeshPath.status == NavMeshPathStatus.PathComplete) {
                    line.positionCount = navMeshPath.corners.Length;
                    line.SetPositions(navMeshPath.corners);
                } else {
                    line.positionCount = 0; // Clear the line if no valid path
                }
            }
        } else {
            line.positionCount = 0; // Hide line if no valid selection
        }
    }

    private void OnEnable() {
        if (m_TrackedImageManager != null)
            m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable() {
        if (m_TrackedImageManager != null)
            m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args) {
        foreach (var newImage in args.added) {
            navigationBase = Instantiate(trackedImagePrefab);
            navMeshSurface = navigationBase.GetComponentInChildren<NavMeshSurface>();
        }

        foreach (var updatedImage in args.updated) {
            if (navigationBase != null) {
                navigationBase.transform.SetPositionAndRotation(
                    updatedImage.transform.position,
                    Quaternion.Euler(0, updatedImage.transform.rotation.eulerAngles.y, 0)
                );

                // **Set Player Position Based on QR Code**
                SetPlayerPositionFromQRCode(updatedImage.referenceImage.name);
            }
        }
    }

    private void SetPlayerPositionFromQRCode(string qrCodeName) {

        if (hasSetStartingPoint) return;

        GameObject targetCube = navigationTargets.FirstOrDefault(target => target.name == qrCodeName);
    
        if (targetCube != null) {
            player.position = targetCube.transform.position;
            playerHasScanned = true; // Mark that the player has scanned a QR code
            Debug.Log($"Recentered player to {qrCodeName}");

            // Set the starting point text
            startingpoint.text = qrCodeName;
            hasSetStartingPoint = true;
            destination.text = dropdown.options[dropdown.value].text;
            if (slideUpAnimation != null) {
                slideUpAnimation.SlideUp();
            }

            // Get the currently selected target from the dropdown
            string selectedTargetName = dropdown.options[dropdown.value].text;

            // If scanned QR matches the selected target, show panel
            if (qrCodeName == selectedTargetName) {
                infoPanel.SetActive(true);  // Show panel
                line.positionCount = 0;     // Hide LineRenderer (navigation complete)
                Debug.Log($"Arrived at {qrCodeName}. Navigation complete!");

                 // Call AddCoinsToUser from CoinManager
                if (coinManager != null)
                {
                    coinManager.AddCoinsToUser(50, "onTimeReward");
                }
            } else {
                // If QR is not the destination, update path normally
                UpdateLineRenderer();
            }
        } else {
            Debug.LogWarning($"No matching target cube found for QR Code: {qrCodeName}");
        }
    }

    private void PopulateDropdown() {
        dropdown.options.Clear();

        // Add default option
        dropdown.options.Add(new TMP_Dropdown.OptionData("-Select Destination-"));

        // Sort navigationTargets alphabetically by their names
        navigationTargets = navigationTargets.OrderBy(target => target.name).ToList();

        foreach (var target in navigationTargets) {
            if (target != null) {
                dropdown.options.Add(new TMP_Dropdown.OptionData(target.name));
                Debug.Log($"Added to dropdown: {target.name}");
            }
        }

        dropdown.RefreshShownValue();
        dropdown.value = 0; // Default to "-Select Destination-"
        dropdown.captionText.text = dropdown.options[0].text;

        Debug.Log("Dropdown initialized with '-Select Destination-' as default.");
    }

    private void UpdateLineRenderer() {
        if (dropdown.value == 0 || !playerHasScanned) { 
            // If "-Select Destination-" is selected or player hasn't scanned a QR, hide the line
            line.positionCount = 0;
            return;
        }

        string selectedTargetName = dropdown.options[dropdown.value].text;
        GameObject selectedTarget = navigationTargets.FirstOrDefault(target => target.name == selectedTargetName);

        if (selectedTarget != null) {
            Debug.Log($"Selected target: {selectedTargetName}");
            NavMesh.CalculatePath(player.position, selectedTarget.transform.position, NavMesh.AllAreas, navMeshPath);

            if (navMeshPath.status == NavMeshPathStatus.PathComplete) {
                line.positionCount = navMeshPath.corners.Length;
                line.SetPositions(navMeshPath.corners);

                // EstimateDistanceAndTime();
            } else {
                Debug.LogWarning("No valid path found to the selected target!");
                line.positionCount = 0;

                /*
                if (distanceText != null)
                distanceText.text = "Distance: -- m";

                if (estimatedTimeText != null)
                estimatedTimeText.text = "ETA: -- sec";

                */

            }
        } else {
            Debug.LogWarning($"No valid target found with the name: {selectedTargetName}");
            line.positionCount = 0;
        }
    }

    private void OnDropdownValueChanged(int index) {
        // Prevent rendering the line if the user selects "-Select Destination-"
        if (index == 0 || !playerHasScanned) {
            line.positionCount = 0;
            Debug.Log("Navigation line hidden because '-Select Destination-' is selected or player has not scanned a QR code.");
        } else {
            UpdateLineRenderer();
        }

    }

/*
    public void CancelNavigation() {
    Debug.Log("Navigation cancelled. Resetting everything...");

    // Reset player tracking
    playerHasScanned = false;
    hasSetStartingPoint = false;

    // Clear navigation path
    line.positionCount = 0;

    // Reset dropdown selection
    dropdown.value = 0;
    dropdown.captionText.text = dropdown.options[0].text;

    // Reset UI text
    startingpoint.text = "-";
    destination.text = "-";

    // Hide UI elements
    infoPanel.SetActive(false);

    // Optionally, reset navigation base if needed
    if (navigationBase != null) {
        Destroy(navigationBase); // Remove the spawned QR reference
        navigationBase = null;
    }

    Debug.Log("Navigation reset complete.");
}

private void EstimateDistanceAndTime()
{
    if (navMeshPath.status != NavMeshPathStatus.PathComplete)
    {
        Debug.LogWarning("No valid path found to estimate distance and time.");
        
        if (distanceText != null)
            distanceText.text = "Distance: -- m";
        
        if (estimatedTimeText != null)
            estimatedTimeText.text = "ETA: -- sec";

        return;
    }

    float totalDistance = 0f;

    for (int i = 0; i < navMeshPath.corners.Length - 1; i++)
    {
        totalDistance += Vector3.Distance(navMeshPath.corners[i], navMeshPath.corners[i + 1]);
    }

    float walkingSpeed = 1.4f; // meters per second (average walking speed)
    float estimatedTime = totalDistance / walkingSpeed;

    Debug.Log($"Estimated Distance: {totalDistance:F1} meters");
    Debug.Log($"Estimated Arrival Time: {estimatedTime:F1} seconds");

    if (distanceText != null)
        distanceText.text = $"{totalDistance:F1} m";

    if (estimatedTimeText != null)
        estimatedTimeText.text = $"{estimatedTime:F1} sec";
}


*/

}
