using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using System.Threading.Tasks;
using Firebase.Extensions;

public class ProfileAvatar : MonoBehaviour
{
    public Image avatarImage; // Assign in Inspector
    private DatabaseReference dbReference;
    private string userId; // Already declared in your script

    void Start()
    {
        userId = UserSession.UserId;
        InitializeFirebase();
    }

    private async Task InitializeFirebase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;

            // Now it's safe to load avatar and user data
            LoadAvatarFromDatabase();
        }
        else
        {
            Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
        }
    }

    async void LoadAvatarFromDatabase()
    {
        string avatarName = await GetAvatarName();
        LoadAvatarImage(avatarName);
        Debug.Log($"Load Avatar Image: {avatarName}");

    }

    async Task<string> GetAvatarName()
    {
        try
        {
            Debug.Log($"Fetching avatar for userId: {userId}");
            var snapshot = await dbReference.Child("users").Child(userId).Child("avatarName").GetValueAsync();

            if (snapshot.Exists && snapshot.Value != null)
            {
                string avatarName = snapshot.Value.ToString();
                Debug.Log($"Avatar found: {avatarName}");
                return avatarName;
            }
            else
            {
                Debug.LogWarning($"Avatar not found for userId: {userId}. Using default.");
                return "Capybara Avatar";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error fetching avatar: {e.Message}");
            return "Capybara Avatar";
        }
    }

    void LoadAvatarImage(string avatarName)
    {
        string path = $"Avatars/{avatarName}";

        Debug.Log($"Attempting to load avatar from: {path}");

        Sprite avatarSprite = Resources.Load<Sprite>(path);

        if (avatarImage == null)
        {
            Debug.LogError("avatarImage is not assigned! Drag the UI Image into the script in Inspector.");
            return;
        }

        if (avatarSprite != null)
        {
            avatarImage.sprite = avatarSprite;
            Debug.Log($"Successfully loaded avatar: {path}");
        }
        else
        {
            avatarImage.sprite = Resources.Load<Sprite>("Avatars/placeholder"); // Load fallback
        }
    }
}
