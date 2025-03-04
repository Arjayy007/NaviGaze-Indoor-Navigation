using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

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
    public GameObject slideUpPanel;
    public ClassNavigationManager classNavigationManager;


    private List<GameObject> navigationTargets = new List<GameObject>(); // List of all target locations
    private NavMeshPath navMeshPath;
    private GameObject navigationBase;

    private bool isQRCodeScanned = false;
    private bool hasSavedToDatabase = false;

    private void Start()
    {
        navMeshPath = new NavMeshPath();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        navigationTargets = GameObject.FindGameObjectsWithTag("Target").ToList();
        PopulateDropdown();

        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
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
            // Move player to new QR position
            player.position = targetCube.transform.position;
            Debug.Log($"Player repositioned to {qrCodeName}");

            if (targetCube.name == destinationPoint.text)
            {

                string startPoint = startingPoint.text;
                string endPoint = destinationPoint.text;

                if (!hasSavedToDatabase)
                {
                    openHistory();
                    classNavigationManager.CheckForClassNavigation(startPoint, endPoint);
                    hasSavedToDatabase = true;
                };


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

        // Reset estimated distance and time display
        estimatedDistance.text = "";
        estimatedTime.text = "";

        // Reset QR scan flags
        isQRCodeScanned = false;

        // Clear destination texts
        destinationPoint.text = "";
        destinationRoom.text = "";
    }


}