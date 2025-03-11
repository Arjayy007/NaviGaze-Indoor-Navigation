using UnityEngine;
using UnityEngine.UI;

public class OfflineMapController : MonoBehaviour
{
    public GameObject panel;   // Main panel (Campus Panel 1)
    public GameObject panel2;  // Main panel (Campus Panel 2)
    public GameObject panel3;  // Main panel (Campus Panel 3)

    public Dropdown OfflineMapDropdown;  // First dropdown (Campus selection)
    public Dropdown FloorCampusDropdown; // Second dropdown (Floor selection)

    public GameObject[] floorPanels; // Array of floor panels (Ground, 1st, 2nd, etc.)

    void Start()
    {
        OfflineMapDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        FloorCampusDropdown.onValueChanged.AddListener(OnFloorDropdownValueChanged);
        FloorCampusDropdown.gameObject.SetActive(false); // Hide initially
    }

    public void OnDropdownValueChanged(int index)
    {
        string selectedOption = OfflineMapDropdown.options[index].text;
        Debug.Log("Selected: " + selectedOption);

        // Show/Hide Campus Panels
        switch (index)
        {
            case 0: // Panel 1 (Enables Floor Dropdown)
                panel.SetActive(true);
                panel2.SetActive(false);
                panel3.SetActive(false);
                FloorCampusDropdown.gameObject.SetActive(true); // Show floor dropdown
                break;
            case 1: // Panel 2 (Hides Floor Dropdown)
                panel.SetActive(false);
                panel2.SetActive(true);
                panel3.SetActive(false);
                FloorCampusDropdown.gameObject.SetActive(false); // Hide floor dropdown
                HideAllFloorPanels();
                break;
            case 2: // Panel 3 (Hides Floor Dropdown)
                panel.SetActive(false);
                panel2.SetActive(false);
                panel3.SetActive(true);
                FloorCampusDropdown.gameObject.SetActive(false); // Hide floor dropdown
                HideAllFloorPanels();
                break;
        }
    }

    public void OnFloorDropdownValueChanged(int index)
    {
        // Hide all floor panels first
        HideAllFloorPanels();

        // Show the selected floor panel
        if (index >= 0 && index < floorPanels.Length)
        {
            floorPanels[index].SetActive(true);
        }
    }

    private void HideAllFloorPanels()
    {
        foreach (GameObject floorPanel in floorPanels)
        {
            floorPanel.SetActive(false);
        }
    }
}
