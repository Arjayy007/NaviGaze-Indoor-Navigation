using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;

public class CollectionController : MonoBehaviour
{
    public Transform contentParent;
    public GameObject CollectionPanelPrefab;

    private FirebaseFirestore firestore;
    private string userId;

    void Start()
    {
        userId = UserSession.UserId;

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is not set!");
            return;
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                firestore = FirebaseFirestore.DefaultInstance;
                LoadUserInventoryFromFirestore();
            }
            else
            {
                Debug.LogError("Firebase initialization failed: " + task.Result);
            }
        });
    }

    void LoadUserInventoryFromFirestore()
    {
        CollectionReference inventoryRef = firestore.Collection("users").Document(userId).Collection("inventory");

        inventoryRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;

                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    string itemName = doc.Id;
                    bool isUsed = doc.ContainsField("isUsed") && doc.GetValue<bool>("isUsed");

                    CreateUIItem(itemName, isUsed);
                }
            }
            else
            {
                Debug.LogError("Failed to fetch inventory: " + task.Exception);
            }
        });
    }

    void CreateUIItem(string itemName, bool isUsed)
    {
        GameObject newItem = Instantiate(CollectionPanelPrefab, contentParent);
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
        itemImageComponent.sprite = itemImage ?? Resources.Load<Sprite>("placeholder");

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
        DocumentReference itemRef = firestore.Collection("users").Document(userId).Collection("inventory").Document(itemName);

        itemRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                bool currentState = task.Result.GetValue<bool>("isUsed");
                bool newState = !currentState;

                itemRef.UpdateAsync("isUsed", newState).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompleted)
                    {
                        Text buttonText = useButton.GetComponentInChildren<Text>();
                        if (buttonText != null)
                        {
                            buttonText.text = newState ? "Unuse" : "Use";
                        }
                        Debug.Log($"'{itemName}' usage toggled: {(newState ? "used" : "unused")}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to update 'isUsed': {updateTask.Exception}");
                    }
                });
            }
            else
            {
                Debug.LogError($"Item '{itemName}' not found or fetch failed.");
            }
        });
    }
}
