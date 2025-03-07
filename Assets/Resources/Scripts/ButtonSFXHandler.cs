using UnityEngine;
using UnityEngine.UI;

public class ButtonSFXHandler : MonoBehaviour
{
    void Start()
    {
        // Find all Button components in the scene
        Button[] buttons = FindObjectsOfType<Button>();

        // Loop through each button and add the click sound listener
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => AudioManager.Instance.PlayButtonClick());
        }
    }
}
