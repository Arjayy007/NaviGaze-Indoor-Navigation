using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;

public class AccessoriesController : MonoBehaviour
{
    public GameObject itemPrefab; 
    public Transform contentParent;

    void Start()
    {
        LoadItemsFromFirestore();
    }

    void LoadItemsFromFirestore()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        db.Collection("items").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to load items from Firestore.");
                return;
            }
            QuerySnapshot snapshot = task.Result;
            Debug.Log($"Total items retrieved: {snapshot.Count}");
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (!document.Exists)
                {
                    Debug.LogWarning("Document doesn't exist.");
                    continue;
                }
                string itemName = document.GetValue<string>("itemName");
                int itemPrice = document.GetValue<int>("itemPrice");

                Debug.Log($"Item Retrieved - ID: {document.Id}, Name: {itemName}, Price: ₱{itemPrice}");
                GameObject itemObj = Instantiate(itemPrefab, contentParent);
                Transform nameTextTransform = itemObj.transform.Find("itemName");
                Transform priceTextTransform = itemObj.transform.Find("itemPrice");

                if (nameTextTransform != null && priceTextTransform != null)
                {
                    nameTextTransform.GetComponent<TMP_Text>().text = itemName;
                    priceTextTransform.GetComponent<TMP_Text>().text = "₱" + itemPrice;
                }
                else
                {
                    Debug.LogWarning("Text components not found in prefab. Check naming.");
                }
                Sprite itemSprite = Resources.Load<Sprite>("Assets/" + itemName);
                Transform imageTransform = itemObj.transform.Find("Image");
                if (imageTransform != null)
                {
                Image imageComponent = imageTransform.GetComponent<Image>();
                if (imageComponent != null)
                {
                    imageComponent.sprite = itemSprite;
                    Debug.Log($"Sprite set for '{itemName}'.");
                }
                else
                {
                Debug.LogWarning("Image component not found on 'Image' object.");
                }
            }
                else
            {
                Debug.LogWarning("Child named 'Image' not found in prefab.");
            }
            }
        });
    }
}
