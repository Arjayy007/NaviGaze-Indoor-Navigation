using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using Firebase;

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
            else {
                Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
            }
        });

        LoadUserInventoryFromDatabase();
    }

    void LoadUserInventoryFromDatabase()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is not set!");
            return;
        }

        string userInventoryPath = $"users/{UserSession.UserId}/inventory"; 
        dbReference.Child(userInventoryPath).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot == null)
                {
                    Debug.LogError("Snapshot is null.");
                    return;
                }

                if (snapshot.Exists)
                {

                    if (snapshot.ChildrenCount == 0)
                    {
                        Debug.LogError("No items found in the user's inventory.");
                        return;
                    }

                    foreach (var item in snapshot.Children)
                    {
                        string itemName = item.Value.ToString(); 
                        CreateUIItem(itemName);
                    }
                }
                else {
                    Debug.LogError("User inventory is empty or doesn't exist.");
                }
            }
            else {
                Debug.LogError("Failed to fetch user inventory from Firebase: " + task.Exception);
            }
        });
    }

    void CreateUIItem(string itemName)
    {
        Debug.Log($"Creating UI panel for item: {itemName}");
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

        Debug.Log($"Attempting to load image for {itemName} from path: Assets/{itemName}");

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
    }
}
