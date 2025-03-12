using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Extensions;
using Firebase;
using System.Collections.Generic;

public class CollectionController : MonoBehaviour
{
    public Transform contentParent; // Assign the Content object of the Scroll View
    public GameObject CollectionPanelPrefab; // Assign the panel prefab in the Inspector

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

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase is ready.");
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                LoadUserInventoryFromDatabase();
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
            }
        });
    }

    void LoadUserInventoryFromDatabase()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is not set!");
            return;
        }

        string userInventoryPath = $"users/{userId}/inventory";
        dbReference.Child(userInventoryPath).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot == null || !snapshot.Exists)
                {
                    Debug.LogError("User inventory is empty or doesn't exist.");
                    return;
                }

                foreach (var item in snapshot.Children)
                {
                    string itemName = item.Key; // The actual item name
                    bool isUsed = item.Child("isUsed").Exists && item.Child("isUsed").Value.ToString() == "True"; // Fetch 'isUsed'

                    CreateUIItem(itemName, isUsed);
                }
            }
            else
            {
                Debug.LogError("Failed to fetch user inventory from Firebase: " + task.Exception);
            }
        });
    }

    void CreateUIItem(string itemName, bool isUsed)
    {
        GameObject newItem = Instantiate(CollectionPanelPrefab, contentParent);

        if (newItem == null)
        {
            Debug.LogError("Failed to instantiate the item panel prefab.");
            return;
        }

        newItem.SetActive(true);

        Text[] texts = newItem.GetComponentsInChildren<Text>();
        foreach (Text text in texts)
        {
            if (text.name == "ItemName")
            {
                text.text = itemName;
            }
        }

        Sprite itemImage = Resources.Load<Sprite>($"Assets/{itemName}");
        Image itemImageComponent = newItem.transform.Find("ItemImage").GetComponent<Image>();
        if (itemImage != null)
        {
            itemImageComponent.sprite = itemImage;
        }
        else
        {
            Debug.LogWarning($"Image for {itemName} not found. Using placeholder.");
            Sprite placeholderImage = Resources.Load<Sprite>("placeholder");
            if (placeholderImage != null)
            {
                itemImageComponent.sprite = placeholderImage;
            }
        }

        // Set up the UseButton
        Button useButton = newItem.transform.Find("UseButton").GetComponent<Button>();
        if (useButton != null)
        {
            Text buttonText = useButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = isUsed ? "Unuse" : "Use";
            }

            useButton.onClick.AddListener(() => ToggleItemUsage(itemName, useButton));
        }
    }

    void ToggleItemUsage(string itemName, Button useButton)
    {
        string itemPath = $"users/{userId}/inventory/{itemName}/isUsed";

        dbReference.Child(itemPath).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                bool currentState = task.Result.Exists && task.Result.Value.ToString() == "True";
                bool newState = !currentState; // Toggle the value

                dbReference.Child(itemPath).SetValueAsync(newState).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompleted)
                    {
                        Text buttonText = useButton.GetComponentInChildren<Text>();
                        if (buttonText != null)
                        {
                            buttonText.text = newState ? "Unuse" : "Use";
                        }
                        Debug.Log($"Item '{itemName}' is now {(newState ? "used" : "unused")}.");
                    }
                    else
                    {
                        Debug.LogError($"Failed to update 'isUsed' for '{itemName}': " + updateTask.Exception);
                    }
                });
            }
            else
            {
                Debug.LogError($"Failed to fetch 'isUsed' state for '{itemName}': " + task.Exception);
            }
        });
    }
}
