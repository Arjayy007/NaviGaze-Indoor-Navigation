using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OfflineMapController : MonoBehaviour
{
    public Dropdown firstDropdown;  // The first dropdown (Campus)
    public Dropdown secondDropdown; // The second dropdown (Floors/Buildings)

    void Start()
    {
        // Ensure Dropdown 2 starts as disabled
        secondDropdown.interactable = false;

        // Clear previous options
        firstDropdown.onValueChanged.AddListener(delegate { UpdateSecondDropdown(); });
    }

    void UpdateSecondDropdown()
    {
        // Enable second dropdown when first dropdown has a valid selection
        secondDropdown.interactable = true;
        secondDropdown.ClearOptions();

        List<string> secondDropdownOptions = new List<string>();

        // Get selected option from first dropdown
        string selectedOption = firstDropdown.options[firstDropdown.value].text;

        // Populate second dropdown based on first dropdown selection
        switch (selectedOption)
        {
            case "Congress":
                secondDropdownOptions.AddRange(new List<string> { "Ground Floor", "2nd Floor", "3rd Floor", "4th Floor", "5th Floor" });
                break;

            case "Camarin":
                secondDropdownOptions.AddRange(new List<string> { "Old Building: 1st Floor", "Old Building: 2nd Floor", "Old Building: Third Floor", "Building 2" });
                break;

            case "Engineering":
                secondDropdownOptions.AddRange(new List<string> { "First Floor", "Second Floor", "Third Floor", "Fourth Floor" });
                break;

            default:
                secondDropdown.interactable = false;
                return; // No valid option selected
        }

        // Apply new options to second dropdown
        secondDropdown.AddOptions(secondDropdownOptions);
    }
}
