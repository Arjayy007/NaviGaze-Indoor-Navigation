using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OfflineMapController : MonoBehaviour
{
    public Dropdown firstDropdown;  // The first dropdown (Campus)
    public Dropdown secondDropdown; // The second dropdown (Floors/Buildings)

    // Panel references
    public GameObject congressGroundFloorPanel;
    public GameObject congressSecondFloorPanel;
    public GameObject congressThirdFloorPanel;
    public GameObject congressFourthFloorPanel;
    public GameObject congressFifthFloorPanel;

    public GameObject camarinOldBuilding1stFloorPanel;
    public GameObject camarinOldBuilding2ndFloorPanel;
    public GameObject camarinOldBuilding3rdFloorPanel;
    public GameObject camarin2ndBuildingPanel;

    public GameObject engineeringFirstFloorPanel;
    public GameObject engineeringSecondFloorPanel;
    public GameObject engineeringThirdFloorPanel;
    public GameObject engineeringFourthFloorPanel;

    private Dictionary<string, GameObject> panelDictionary = new Dictionary<string, GameObject>();

    void Start()
    {
        // Initialize panel dictionary
        panelDictionary.Add("Congress: Ground Floor", congressGroundFloorPanel);
        panelDictionary.Add("Congress: 2nd Floor", congressSecondFloorPanel);
        panelDictionary.Add("Congress: 3rd Floor", congressThirdFloorPanel);
        panelDictionary.Add("Congress: 4th Floor", congressFourthFloorPanel);
        panelDictionary.Add("Social Hall", congressFifthFloorPanel);

        panelDictionary.Add("Old Building: 1st Floor", camarinOldBuilding1stFloorPanel);
        panelDictionary.Add("Old Building: 2nd Floor", camarinOldBuilding2ndFloorPanel);
        panelDictionary.Add("Old Building: 3rd Floor", camarinOldBuilding3rdFloorPanel);
        panelDictionary.Add("2nd Building", camarin2ndBuildingPanel);

        panelDictionary.Add("Engineering: 1st Floor", engineeringFirstFloorPanel);
        panelDictionary.Add("Engineering: 2nd Floor", engineeringSecondFloorPanel);
        panelDictionary.Add("Engineering: 3rd Floor", engineeringThirdFloorPanel);
        panelDictionary.Add("Engineering: 4th Floor", engineeringFourthFloorPanel);

        // Attach event listeners
        firstDropdown.onValueChanged.AddListener(delegate { UpdateSecondDropdown(); });
        secondDropdown.onValueChanged.AddListener(delegate { UpdatePanelVisibility(); });
    }

    void UpdateSecondDropdown()
    {
        secondDropdown.interactable = true;
        secondDropdown.ClearOptions();

        List<string> secondDropdownOptions = new List<string>();
        string selectedOption = firstDropdown.options[firstDropdown.value].text;

        switch (selectedOption)
        {
            case "Congress":
                secondDropdownOptions.AddRange(new List<string> { "Congress: Ground Floor", "Congress: 2nd Floor", "Congress: 3rd Floor", "Congress: 4th Floor", "Social Hall" });
                break;

            case "Camarin":
                secondDropdownOptions.AddRange(new List<string> { "Old Building: 1st Floor", "Old Building: 2nd Floor", "Old Building: 3rd Floor", "2nd Building" });
                break;

            case "Engineering":
                secondDropdownOptions.AddRange(new List<string> { "Engineering: 1st Floor", "Engineering: 2nd Floor", "Engineering: 3rd Floor", "Engineering: 4th Floor" });
                break;

            default:
                secondDropdown.interactable = false;
                return;
        }

        secondDropdown.AddOptions(secondDropdownOptions);
        HideAllPanels();
    }

    void UpdatePanelVisibility()
    {
        HideAllPanels();
        string selectedOption = secondDropdown.options[secondDropdown.value].text;

        if (panelDictionary.ContainsKey(selectedOption))
        {
            GameObject panel = panelDictionary[selectedOption];
            panel.SetActive(true);
        }
    }

    void HideAllPanels()
    {
        foreach (var panel in panelDictionary.Values)
        {
            panel.SetActive(false);
        }
    }

    // 🔽 DRAG AND ZOOM FEATURE BELOW 🔽

    private Vector2 lastMousePosition;
    private float zoomSpeed = 0.1f;
    private float minZoom = 0.5f;
    private float maxZoom = 2.5f;

    void Update()
    {
        GameObject activePanel = GetActivePanel();
        if (activePanel != null)
        {
            RectTransform rectTransform = activePanel.GetComponent<RectTransform>();

            // Drag
            if (Input.GetMouseButtonDown(0))
            {
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                Vector2 delta = (Vector2)Input.mousePosition - lastMousePosition;
                rectTransform.anchoredPosition += delta;
                lastMousePosition = Input.mousePosition;
            }

            // Zoom
            float scroll = Input.mouseScrollDelta.y;
            if (scroll != 0)
            {
                float newScale = Mathf.Clamp(rectTransform.localScale.x + scroll * zoomSpeed, minZoom, maxZoom);
                rectTransform.localScale = new Vector3(newScale, newScale, 1f);
            }
        }
    }

    private GameObject GetActivePanel()
    {
        foreach (var panel in panelDictionary.Values)
        {
            if (panel.activeSelf)
                return panel;
        }
        return null;
    }
}
