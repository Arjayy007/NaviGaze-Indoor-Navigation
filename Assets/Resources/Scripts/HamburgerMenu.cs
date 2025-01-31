using UnityEngine;
using UnityEngine.SceneManagement;

public class HamburgerMenu : MonoBehaviour
{
    public SceneManagerScript sceneManager;
    public GameObject menuPanel; 
    private bool isMenuVisible = false;

   
    public void ToggleMenu()
    {
        isMenuVisible = !isMenuVisible;  
        menuPanel.SetActive(isMenuVisible);  
    }

    public void LoadOfflineMap() 
    {
        SceneManager.LoadScene("OfflineMapPage");
    }

    public void LoadAchievementMap() 
    {
        SceneManager.LoadScene("CollectionPage");
    }

    public void LoadHistoryMap() 
    {
        SceneManager.LoadScene("HistoryPage");
    }

    public void LoadSettingMap() 
    {
        SceneManager.LoadScene("SettingsPage");
    }


}


