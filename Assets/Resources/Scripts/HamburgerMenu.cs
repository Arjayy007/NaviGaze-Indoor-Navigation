using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class HamburgerMenu : MonoBehaviour
{
    public SceneManagerScript sceneManager;
    public GameObject menuPanel;
    public GameObject invinsibleButton;
    public Text Fullname;

    private DatabaseReference dbReference;
    private bool isMenuVisible = false;
    private string userId;

    private void Start()
    {
        userId = UserSession.UserId;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase initialized successfully.");
                FirebaseApp app = FirebaseApp.DefaultInstance;
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;

                invinsibleButton.SetActive(false); 
            }
            else
            {
                Debug.LogError("Firebase not initialized: " + task.Result);
            }
        });
 
    }





    public void ToggleMenu()
    {
        isMenuVisible = !isMenuVisible;  
        menuPanel.SetActive(isMenuVisible);
        invinsibleButton.SetActive(isMenuVisible); // Show/hide overlay
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


