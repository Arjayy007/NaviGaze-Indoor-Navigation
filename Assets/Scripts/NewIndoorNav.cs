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

    private List<GameObject> navigationTargets = new List<GameObject>(); // All target cubes
    private NavMeshSurface navMeshSurface;
    private NavMeshPath navMeshPath;

    private GameObject navigationBase;

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
    }

    private void Update() {
        // Continuously update navigation when a target is selected
        if (navigationTargets.Count > 0 && dropdown.options.Count > 0) {
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
            if (navigationBase != null)
                navigationBase.transform.SetPositionAndRotation(
                    updatedImage.transform.position,
                    Quaternion.Euler(0, updatedImage.transform.rotation.eulerAngles.y, 0)
                );
        }
    }

   private void PopulateDropdown() {
    dropdown.options.Clear();

    // Sort navigationTargets alphabetically by their names
    navigationTargets = navigationTargets.OrderBy(target => target.name).ToList();

    foreach (var target in navigationTargets) {
        if (target != null) {
            dropdown.options.Add(new TMP_Dropdown.OptionData(target.name));
            Debug.Log($"Added to dropdown: {target.name}");
        }
    }

    dropdown.RefreshShownValue();

    if (navigationTargets.Count > 0) {
        dropdown.value = 0;
        dropdown.captionText.text = dropdown.options[0].text;
        Debug.Log($"Default selection: {dropdown.options[0].text}");
        UpdateLineRenderer();
    } else {
        Debug.LogWarning("No navigation targets found to populate dropdown!");
    }
}


private void UpdateLineRenderer() {
    if (dropdown.options.Count == 0) {
        Debug.LogWarning("No options available in the dropdown!");
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
        } else {
            Debug.LogWarning("No valid path found to the selected target!");
            line.positionCount = 0;
        }
    } else {
        Debug.LogWarning($"No valid target found with the name: {selectedTargetName}");
        line.positionCount = 0;
    }
}


    private void OnDropdownValueChanged(int index) {
        // Triggered when a dropdown selection changes
        UpdateLineRenderer();
    }

    
}
