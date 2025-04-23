using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class UserInventory : MonoBehaviour
{
    public HashSet<string> ownedItems = new HashSet<string>();
    public bool isLoaded = false;

    public void LoadUserInventory(string userId)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        db.Collection("users").Document(userId).Collection("inventory").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to load user inventory.");
                return;
            }

            foreach (var doc in task.Result.Documents)
            {
                string itemName = doc.Id;
                ownedItems.Add(itemName);
                Debug.Log($"Inventory Item: {itemName}, Is Used: {doc.GetValue<bool>("isUsed")}");
            }

            isLoaded = true;
        });
    }
}
