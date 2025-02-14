using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

public class NewIndoorNav : MonoBehaviour {
    [SerializeField] private Transform player; // AR camera representing the player
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private GameObject trackedImagePrefab;
    [SerializeField] private LineRenderer line; // Line Renderer for navigation path
    [SerializeField] private TMP_Dropdown dropdown; // Destination selector
    [SerializeField] private GameObject infoPanel; // UI panel for arrival confirmation
    [SerializeField] private SlideUpAnimation slideUpAnimation;
    [SerializeField] private TMP_Text startingPoint;
    [SerializeField] private TMP_Text destinationPoint;

    private List<GameObject> navigationTargets = new List<GameObject>(); // List of all target locations
    private NavMeshPath navMeshPath;
    private GameObject navigationBase;

    private string lastScannedQRCode = ""; // Track last scanned QR code to avoid redundant updates

    private void Start() {
        navMeshPath = new NavMeshPath();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        navigationTargets = GameObject.FindGameObjectsWithTag("Target").ToList();
        PopulateDropdown();

        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
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
    foreach (var trackedImage in args.added.Concat(args.updated)) { 
        string qrCodeName = trackedImage.referenceImage.name; 

        // Directly update position and navigation
        SetPlayerPositionFromQRCode(qrCodeName);
    }
}



    private void SetPlayerPositionFromQRCode(string qrCodeName) {
    GameObject targetCube = navigationTargets.FirstOrDefault(target => target.name == qrCodeName);
    startingPoint.text = qrCodeName;

    if (targetCube != null) {
        player.position = targetCube.transform.position; // Move player to new QR position
        Debug.Log($"Player repositioned to {qrCodeName}");

        if (slideUpAnimation != null) {
                slideUpAnimation.SlideUp();
        }

        if (dropdown.options[dropdown.value].text == qrCodeName) {
            infoPanel.SetActive(true); // Show arrival confirmation
            line.positionCount = 0; // Clear the line (Arrived at destination)
        } else {
            UpdateLineRenderer(); // Continue updating the navigation path
        }

    } else {
        Debug.LogWarning($"No matching target found for QR Code: {qrCodeName}");
    }
}


  private void PopulateDropdown() {
    dropdown.options.Clear();

    dropdown.options.Add(new TMP_Dropdown.OptionData("-Select Destination"));

    navigationTargets = navigationTargets.OrderBy(target => target.name).ToList();

    foreach (var target in navigationTargets) {
        dropdown.options.Add(new TMP_Dropdown.OptionData(target.name));
    }

    dropdown.RefreshShownValue();

    dropdown.value = 0;
    dropdown.captionText.text = dropdown.options[0].text;

    UpdateLineRenderer();
}

private void UpdateLineRenderer() {
    if (dropdown.value == 0) { 
        line.positionCount = 0;
        return;
    }

    string selectedTargetName = dropdown.options[dropdown.value].text;
    GameObject selectedTarget = navigationTargets.FirstOrDefault(target => target.name == selectedTargetName);

    if (selectedTarget != null) {
        // Always calculate from the latest player position
        NavMesh.CalculatePath(player.position, selectedTarget.transform.position, NavMesh.AllAreas, navMeshPath);

        if (navMeshPath.status == NavMeshPathStatus.PathComplete) {
            line.positionCount = navMeshPath.corners.Length;
            line.SetPositions(navMeshPath.corners);
        } else {
            line.positionCount = 0;
        }
    } else {
        Debug.LogWarning($"No valid target found for {selectedTargetName}");
        line.positionCount = 0;
    }
}



    private void OnDropdownValueChanged(int index) {
        UpdateLineRenderer();
        destinationPoint.text = dropdown.options[dropdown.value].text;
    }

    public void CloseHistoryPanel() {
        slideUpAnimation.SlideUp();
        
    }
}