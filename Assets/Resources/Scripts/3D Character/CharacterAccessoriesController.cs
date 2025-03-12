using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;

public class CharacterAccessoriesController : MonoBehaviour
{
    public GameObject catGlass; // Assign in the Inspector
    public GameObject catHat;   // Assign in the Inspector

    private DatabaseReference dbReference;
    private string userId;

    void Start()
    {
        userId = UserSession.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is not set! Ensure the user is logged in or registered.");
            return;
        }

        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        LoadCharacterAccessories();
    }

    void LoadCharacterAccessories()
    {
        string inventoryPath = $"users/{userId}/inventory";

        dbReference.Child(inventoryPath).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot == null || !snapshot.Exists)
                {
                    Debug.Log("No inventory items found for the user.");
                    return;
                }

                foreach (var item in snapshot.Children)
                {
                    string itemName = item.Key;
                    bool isUsed = item.Child("isUsed").Exists && item.Child("isUsed").Value.ToString() == "True";

                    if (isUsed)
                    {
                        EnableAccessory(itemName);
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to fetch inventory data: " + task.Exception);
            }
        });
    }

    void EnableAccessory(string itemName)
    {
        switch (itemName)
        {
            case "Cat Glasses":
                if (catGlass != null)
                {
                    catGlass.SetActive(true);
                }
                break;

            case "Cat Hat":
                if (catHat != null)
                {
                    catHat.SetActive(true);
                }
                break;

            default:
                Debug.LogWarning($"Accessory '{itemName}' not recognized.");
                break;
        }
    }
}
