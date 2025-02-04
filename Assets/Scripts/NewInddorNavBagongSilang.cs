using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using TMPro; // Import TextMeshPro namespace
using UnityEngine.XR.ARFoundation;

public class NewIndoorNavBagongSilang : MonoBehaviour {
    [SerializeField] private Transform player; // Reference to the player (e.g., AR camera)
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private GameObject trackedImagePrefab;
    [SerializeField] private LineRenderer line; // Line Renderer component
    [SerializeField] private TMP_Dropdown dropdown; // TextMeshPro Dropdown for selecting targets

    private List<GameObject> navigationTargets = new List<GameObject>(); // All target cubes
    private NavMeshSurface navMeshSurface;
    private NavMeshPath navMeshPath;
    private GameObject navigationBase;
    private bool playerHasScanned = false; // Track if the player has scanned a QR code
    private CoinManager coinManager;    // Reference to the CoinManager script

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
                SetPlayerPositionFromQRCode(updatedImage.referenceImage.name);
            }
        }
    }

    private void SetPlayerPositionFromQRCode(string qrCodeName) {
        GameObject targetCube = navigationTargets.FirstOrDefault(target => target.name == qrCodeName);
    
        if (targetCube != null) {
            player.position = targetCube.transform.position;
            playerHasScanned = true;
            Debug.Log($"Recentered player to {qrCodeName}");

            // Get the currently selected target from the dropdown
            string selectedTargetName = dropdown.options[dropdown.value].text;

            if (qrCodeName == selectedTargetName) {
                line.positionCount = 0;
                Debug.Log($"Arrived at {qrCodeName}. Navigation complete!");

                if (coinManager != null)
                {
                    coinManager.AddCoinsToUser(50, "onTimeReward");
                }
            } else {
                UpdateLineRenderer();
            }
        } else {
            Debug.LogWarning($"No matching target cube found for QR Code: {qrCodeName}");
        }
    }

    private void PopulateDropdown() {
        dropdown.options.Clear();
        dropdown.options.Add(new TMP_Dropdown.OptionData("-Select Destination-"));
        navigationTargets = navigationTargets.OrderBy(target => target.name).ToList();

        foreach (var target in navigationTargets) {
            if (target != null) {
                dropdown.options.Add(new TMP_Dropdown.OptionData(target.name));
                Debug.Log($"Added to dropdown: {target.name}");
            }
        }

        dropdown.RefreshShownValue();
        dropdown.value = 0;
        dropdown.captionText.text = dropdown.options[0].text;
    }

    private void UpdateLineRenderer() {
        if (dropdown.value == 0 || !playerHasScanned) { 
            line.positionCount = 0;
            return;
        }

        string selectedTargetName = dropdown.options[dropdown.value].text;
        GameObject selectedTarget = navigationTargets.FirstOrDefault(target => target.name == selectedTargetName);

        if (selectedTarget != null) {
            NavMesh.CalculatePath(player.position, selectedTarget.transform.position, NavMesh.AllAreas, navMeshPath);

            if (navMeshPath.status == NavMeshPathStatus.PathComplete) {
                line.positionCount = navMeshPath.corners.Length;
                line.SetPositions(navMeshPath.corners);
            } else {
                line.positionCount = 0;
            }
        } else {
            line.positionCount = 0;
        }
    }

    private void OnDropdownValueChanged(int index) {
        if (index == 0 || !playerHasScanned) {
            line.positionCount = 0;
        } else {
            UpdateLineRenderer();
        }
    }
}
