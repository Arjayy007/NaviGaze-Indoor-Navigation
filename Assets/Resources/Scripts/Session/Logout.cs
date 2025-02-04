using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Logout : MonoBehaviour
{
    public Button logoutButton;

    void Start()
    {
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(PerformLogout);
        }
        else 
        {
            Debug.LogError("Logout Button is not assigned!");
        }
    }

    void PerformLogout() 
    {
        // Clear User Session
        UserSession.ClearSession();
        Debug.Log("Logging Out...");

        //Go Back to Landing Scene
        SceneManager.LoadScene("LandingPage");
    }
}
