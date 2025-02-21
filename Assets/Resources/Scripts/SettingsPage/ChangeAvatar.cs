using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using System.Threading.Tasks;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class ChangeAvatar : MonoBehaviour
{
    public GameObject ChangeAvatarPanel;
    public Button avatarButton1, avatarButton2, avatarButton3, avatarButton4;

    private string selectedAvatar = ""; // Store the selected avatar
    private string userId;
    private DatabaseReference dbReference;

    void Start()
    {
        userId = UserSession.UserId;
        InitializeFirebase();

        // Assign button click events (now they call UpdateAvatar immediately)
        avatarButton1.onClick.AddListener(() => SelectAvatar("Capybara Avatar"));
        avatarButton2.onClick.AddListener(() => SelectAvatar("placeholder"));
        avatarButton3.onClick.AddListener(() => SelectAvatar("Capybara V2"));
        avatarButton4.onClick.AddListener(() => SelectAvatar("Sitting Capybara"));
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase initialized successfully.");
                FirebaseApp app = FirebaseApp.DefaultInstance;
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
            }
            else
            {
                Debug.LogError("Firebase not initialized: " + task.Result);
            }
        });
    }

    void SelectAvatar(string avatarName)
    {
        selectedAvatar = avatarName; // Store the selected avatar
        Debug.Log($"Avatar selected: {selectedAvatar}");
    }

    public void SaveAvatar()
    {
        if (string.IsNullOrEmpty(selectedAvatar))
        {
            Debug.LogWarning("No avatar selected. Please choose one before saving.");
            return;
        }

        if (dbReference == null)
        {
            Debug.LogError("Firebase not initialized. Cannot update avatar.");
            return;
        }

        Debug.Log($"Saving avatar: {selectedAvatar}");

        dbReference.Child("users").Child(userId).Child("avatarName").SetValueAsync(selectedAvatar)
            .ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Avatar updated successfully in Firebase.");
                }
                else
                {
                    Debug.LogError("Error updating avatar.");
                }
            });
    }

    public void LoadProfilePage()
    {
        Invoke(nameof(LoadScene), 0.5f); // Delays by 0.5 seconds
    }

    private void LoadScene()
    {
        SceneManager.LoadScene("ProfilePage");
    }

    public void CloseAvatarPanel()
    {
        ChangeAvatarPanel.SetActive(false);
    }

    public void LoadChangeAvatarPanel()
    {
        ChangeAvatarPanel.SetActive(true);
    }
}
