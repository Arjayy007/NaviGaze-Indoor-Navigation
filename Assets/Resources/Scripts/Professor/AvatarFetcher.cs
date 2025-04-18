using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Threading.Tasks;

public class AvatarDisplay : MonoBehaviour
{
    public Image avatarImage;
    public Text Fullname;

    private FirebaseFirestore firestore;
    private string userId;

    void Start()
    {
        userId = UserSession.UserId;
        InitializeFirebase();
    }

    private async void InitializeFirebase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            firestore = FirebaseFirestore.DefaultInstance;

            LoadAvatarFromFirestore();
            LoadUserDataFromFirestore();
        }
        else
        {
            Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
        }
    }

    async void LoadAvatarFromFirestore()
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
                Debug.LogWarning("Avatar not found. Using default.");
                return "Capybara Avatar";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error fetching avatar: {e.Message}");
            return "Capybara Avatar";
        }
    }

    void LoadUserDataFromFirestore()
    {
        DocumentReference docRef = firestore
            .Collection("users")
            .Document(userId)
            .Collection("information")
            .Document("profile");

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    string firstName = snapshot.ContainsField("firstName") ? snapshot.GetValue<string>("firstName") : "";
                    string lastName = snapshot.ContainsField("lastName") ? snapshot.GetValue<string>("lastName") : "";

                    Fullname.text = $"{firstName} {lastName}";
                }
                else
                {
                    Debug.LogError("User profile document not found.");
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve user data.");
            }
        });
    }

    void LoadAvatarImage(string avatarName)
    {
        string path = $"Avatars/{avatarName}";
        Debug.Log($"Attempting to load avatar from: {path}");

        Sprite avatarSprite = Resources.Load<Sprite>(path);

        if (avatarImage == null)
        {
            Debug.LogError("avatarImage is not assigned in the inspector.");
            return;
        }

        if (avatarSprite != null)
        {
            avatarImage.sprite = avatarSprite;
            Debug.Log($"Successfully loaded avatar: {path}");
        }
        else
        {
            avatarImage.sprite = Resources.Load<Sprite>("Avatars/placeholder"); // fallback
        }
    }
}
