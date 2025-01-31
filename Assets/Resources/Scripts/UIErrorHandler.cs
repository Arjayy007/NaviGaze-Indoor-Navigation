using TMPro;
using UnityEngine;

public class UIErrorHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI errorText; // Reference to the TMP text

    private void Start()
    {
        HideError(); // Ensure the error text is hidden at the start
    }

    public void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true); // Enable the text
        }
    }

    public void HideError()
    {
        if (errorText != null)
            errorText.gameObject.SetActive(false); // Hide the text
    }
}
