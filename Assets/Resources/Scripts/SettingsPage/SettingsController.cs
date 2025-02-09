using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsController : MonoBehaviour
{
    public SceneManagerScript sceneManager; 

    public void LoadProfileScene() 
    {
        SceneManager.LoadScene("ProfilePage");
    }

    public void LoadEditSchedule() 
    {
        SceneManager.LoadScene("EditSchedulePage");
    }

    public void LoadPrivacyPolicy() 
    {
        SceneManager.LoadScene("PrivacyPolicyPage");
    }

}
