using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using System.Threading.Tasks;
using Firebase.Extensions;

public class AvatarDisplay : MonoBehaviour
{
    public Image avatarImage; // Assign in Inspector
    public Text Fullname;
    private DatabaseReference dbReference;
    private string userId; // Already declared in your script

    void Start()
    {
        userId = UserSession.UserId;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        LoadAvatarFromDatabase();
        LoadUserData();
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

    public void LoadUserData()
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
