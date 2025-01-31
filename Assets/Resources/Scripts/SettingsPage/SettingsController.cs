using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsController : MonoBehaviour
{
    public SceneManagerScript sceneManager; 

    public void LoadProfileScene() 
    {
        SceneManager.LoadScene("ProfilePage");
    }

}
