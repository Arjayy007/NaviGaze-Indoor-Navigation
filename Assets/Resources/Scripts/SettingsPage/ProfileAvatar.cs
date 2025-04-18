using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ProfileAvatar : MonoBehaviour
{
    public Image avatarImage; // Assign in Inspector
    private FirebaseFirestore firestore;
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
            firestore = FirebaseFirestore.DefaultInstance;
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
            DocumentReference docRef = firestore
                .Collection("users")
                .Document(userId)
                .Collection("information")
                .Document("profile");

            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists && snapshot.ContainsField("avatarName"))
            {
                string avatarName = snapshot.GetValue<string>("avatarName");
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
