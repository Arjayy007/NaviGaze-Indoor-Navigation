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
                LoadUserData(); 
            }
            else
            {
                Debug.LogError("Firebase not initialized: " + task.Result);
            }
        });
 
    }

    void LoadUserData()
    {
        string userPath = $"users/{userId}"; 

        dbReference.Child(userPath).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    string firstName = snapshot.Child("firstName").Value.ToString();
                    string lastName = snapshot.Child("lastName").Value.ToString();

                    string fullName = firstName + " " + lastName;


                    Fullname.text = fullName;  
                }
                else
                {
                    Debug.LogError("User data not found!");
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve user data.");
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


