using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;
using Firebase;


public class AccessoriesController : MonoBehaviour
{
    public GameObject itemPrefab; 
    public Transform contentParent;
    public UserInventory userInventory;
    public BuyItem buyItem; // Reference to the BuyItem script

void Start()
{
    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
    {
        var dependencyStatus = task.Result;
        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("Firebase dependencies are available.");
            string userId = UserSession.UserId;
            userInventory.LoadUserInventory(userId);
            StartCoroutine(WaitForInventoryAndLoadItems());
        }
        else
        {
            Debug.LogError($"Firebase dependencies not resolved: {dependencyStatus}");
        }
    });
}
    IEnumerator WaitForInventoryAndLoadItems()
    {
        while (!userInventory.isLoaded)
        {
            yield return null; 
        }
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

            foreach (DocumentSnapshot document in task.Result.Documents)
            {
                if (!document.Exists) continue;

                string itemName = document.GetValue<string>("itemName");
                int itemPrice = document.GetValue<int>("itemPrice");

                if (userInventory.ownedItems.Contains(itemName))
                {
                    Debug.Log($"Skipping owned item: {itemName}");
                    continue;
                }

                Debug.Log($"Item Retrieved - ID: {document.Id}, Name: {itemName}, Price: ₱{itemPrice}");
                GameObject itemObj = Instantiate(itemPrefab, contentParent);

                Transform nameTextTransform = itemObj.transform.Find("itemName");
                Transform priceTextTransform = itemObj.transform.Find("itemPrice");

                if (nameTextTransform != null && priceTextTransform != null)
                {
                    nameTextTransform.GetComponent<TMP_Text>().text = itemName;
                    priceTextTransform.GetComponent<TMP_Text>().text = "₱" + itemPrice;
                }

                Sprite itemSprite = Resources.Load<Sprite>("Assets/" + itemName);
                Transform imageTransform = itemObj.transform.Find("Image");
                if (imageTransform != null)
                {
                    Image imageComponent = imageTransform.GetComponent<Image>();
                    if (imageComponent != null && itemSprite != null)
                    {
                        imageComponent.sprite = itemSprite;
                        Debug.Log($"Sprite set for '{itemName}'.");
                    }
                    else
                    {
                        Debug.LogWarning($"Sprite for '{itemName}' not found or image component missing.");
                    }
                }

                 Button previewButton = itemObj.transform.Find("Preview").GetComponent<Button>();
                if (previewButton != null)
                {
                    previewButton.onClick.AddListener(() => OnPreviewButtonClicked(itemName, itemPrice));
                }
                else
                {
                    Debug.LogWarning("PreviewButton not found in prefab.");
                }

                Button buyButton = itemObj.transform.Find("BuyItem").GetComponent<Button>();
                if (buyButton != null)
                {
                       string capturedItemName = itemName;
                        int capturedItemPrice = itemPrice;
                        Sprite capturedSprite = itemSprite;
                        buyButton.onClick.AddListener(() =>
                         {
                            buyItem.ShowConfirmationPanel(itemName, itemPrice, itemSprite, itemObj);
                        });
                }
                else
                {
                    Debug.LogWarning("PreviewButton not found in prefab.");
                }

            }
        });
    }

    void OnPreviewButtonClicked(string itemName, int itemPrice)
    {
        ItemPreviewData.itemName = itemName;
        ItemPreviewData.itemPrice = "₱" + itemPrice.ToString();
        Debug.Log($"Previewing: {itemName}, Price: {ItemPreviewData.itemPrice}");
        UnityEngine.SceneManagement.SceneManager.LoadScene("PreviewPage");
    }
}
