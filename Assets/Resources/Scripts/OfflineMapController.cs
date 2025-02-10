using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class OfflineMapController : MonoBehaviour
{
    public GameObject panel;
    public GameObject panel2;
    public GameObject panel3;

public Dropdown OfflineMapDropdown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
          OfflineMapDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
 public void OnDropdownValueChanged(int index)
    {
        // Get the selected value
        string selectedOption = OfflineMapDropdown.options[index].text;
        Debug.Log("Selected: " + selectedOption);

        // Example: Perform different actions based on the selected value
        switch (index)
        {
            case 0:
            panel.SetActive (true);
            panel2.SetActive (false);
            panel3.SetActive (false);
                Debug.Log("Option 1 selected");
                break;
            case 1:
            panel.SetActive (false);
            panel2.SetActive (true);
            panel3.SetActive (false);
                Debug.Log("Option 2 selected");
                break;
            case 2:
             panel.SetActive (false);
            panel2.SetActive (false);
            panel3.SetActive (true);
                Debug.Log("Option 3 selected");
                break;
        }
    }
}
