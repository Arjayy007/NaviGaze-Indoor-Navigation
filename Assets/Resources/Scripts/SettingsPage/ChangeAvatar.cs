using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine.SceneManagement;

public class ChangeAvatar : MonoBehaviour
{
    public GameObject ChangeAvatarPanel;
    public Button avatarButton1, avatarButton2, avatarButton3, avatarButton4;

    private string selectedAvatar = ""; // Store the selected avatar
    private string userId;
    private FirebaseFirestore firestore;

    void Start()
    {
        userId = UserSession.UserId;
        InitializeFirebase();

        // Assign button click events
        avatarButton1.onClick.AddListener(() => SelectAvatar("Capybara Avatar"));
        avatarButton2.onClick.AddListener(() => SelectAvatar("placeholder"));
        avatarButton3.onClick.AddListener(() => SelectAvatar("Capybara V2"));
        avatarButton4.onClick.AddListener(() => SelectAvatar("Sitting Capybara"));
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                firestore = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firestore initialized successfully.");
            }
            else
            {
                Debug.LogError("Firestore not initialized: " + task.Result);
            }
        });
    }

    void SelectAvatar(string avatarName)
    {
        selectedAvatar = avatarName;
        Debug.Log($"Avatar selected: {selectedAvatar}");
    }

    public void SaveAvatar()
    {
        if (string.IsNullOrEmpty(selectedAvatar))
        {
            Debug.LogWarning("No avatar selected. Please choose one before saving.");
            return;
        }

        if (firestore == null)
        {
            Debug.LogError("Firestore not initialized. Cannot update avatar.");
            return;
        }

        Debug.Log($"Saving avatar: {selectedAvatar}");

        DocumentReference profileRef = firestore
            .Collection("users")
            .Document(userId)
            .Collection("information")
            .Document("profile");

        profileRef.UpdateAsync("avatarName", selectedAvatar).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Avatar updated successfully in Firestore.");
            }
            else
            {
                Debug.LogError("Error updating avatar in Firestore: " + task.Exception);
            }
        });
    }

    public void LoadProfilePage()
    {
        Invoke(nameof(LoadScene), 0.5f);
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
