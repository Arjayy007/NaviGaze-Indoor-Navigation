using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.XR.ARFoundation;

public class NewIndoorNavBagongSilang : MonoBehaviour {
    [SerializeField] private Transform player;
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private GameObject trackedImagePrefab;
    [SerializeField] private LineRenderer line;
    [SerializeField] private TMP_Dropdown dropdown;

    private List<GameObject> navigationTargets = new List<GameObject>();
    private NavMeshSurface navMeshSurface;
    private NavMeshPath navMeshPath;
    private GameObject navigationBase;
    private CoinManager coinManager;

    private void Start() {
        navMeshPath = new NavMeshPath();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        navigationTargets = GameObject.FindGameObjectsWithTag("Target").ToList();

        if (navigationTargets.Count == 0) {
            Debug.LogWarning("No targets found with the 'Target' tag!");
        }

        PopulateDropdown();
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        coinManager = FindAnyObjectByType<CoinManager>();

        UpdateLineRenderer(); // Ensure line rendering starts immediately
    }

    private void Update() {
        if (navigationTargets.Count > 0 && dropdown.options.Count > 0 && dropdown.value != 0) {
            UpdateLineRenderer();
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
            Debug.Log($"Recentered player to {qrCodeName}");

            string selectedTargetName = dropdown.options[dropdown.value].text;

            if (qrCodeName == selectedTargetName) {
                line.positionCount = 0;
                Debug.Log($"Arrived at {qrCodeName}. Navigation complete!");
                if (coinManager != null) {
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
        if (dropdown.value == 0) { 
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
        UpdateLineRenderer();
    }
}
