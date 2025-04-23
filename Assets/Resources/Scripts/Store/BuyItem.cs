using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class BuyItem : MonoBehaviour
{
    public GameObject confirmationPanel;
    public GameObject successPanel;
    public GameObject errorPanel;

    public TMP_Text itemNameText;
    public TMP_Text itemPriceText;
    public Image itemImage;

    public Button yesButton;
    public Button noButton;

    private string currentItemName;
    private int currentItemPrice;
    private Sprite currentItemSprite;

    private string userId;
    private GameObject currentStoreItemObject;
    void Start()
    {
        userId = UserSession.UserId;

        noButton.onClick.AddListener(() =>
        {
            confirmationPanel.SetActive(false);
        });
    }



public void ShowConfirmationPanel(string itemName, int itemPrice, Sprite itemSprite, GameObject storeItemObj)
{
    currentItemName = itemName;
    currentItemPrice = itemPrice;
    currentItemSprite = itemSprite;
    currentStoreItemObject = storeItemObj;

    itemNameText.text = itemName;
    itemPriceText.text = "₱" + itemPrice;
    itemImage.sprite = itemSprite;

    confirmationPanel.SetActive(true);

    yesButton.onClick.RemoveAllListeners();
    yesButton.onClick.AddListener(() => {
        BuyItemNow(currentItemName, currentItemPrice, currentStoreItemObject);
        confirmationPanel.SetActive(false);
    });
}


    private void BuyItemNow(string itemName, int itemPrice, GameObject storeItemObj)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        DocumentReference profileRef = db.Collection("users").Document(userId)
                                         .Collection("information").Document("profile");

        profileRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            confirmationPanel.SetActive(false);

            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to retrieve user profile.");
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists && snapshot.ContainsField("userCoins"))
            {
                int currentCoins = snapshot.GetValue<int>("userCoins");

                if (currentCoins >= itemPrice)
                {
                    int updatedCoins = currentCoins - itemPrice;

                    profileRef.UpdateAsync("userCoins", updatedCoins).ContinueWithOnMainThread(updateTask =>
                    {
                        if (updateTask.IsCompletedSuccessfully)
                        {
                            DocumentReference inventoryRef = db.Collection("users").Document(userId)
                                                               .Collection("inventory").Document(itemName);

                            Dictionary<string, object> inventoryData = new Dictionary<string, object>
                            {
                                { "isUsed", false }
                            };

                            inventoryRef.SetAsync(inventoryData).ContinueWithOnMainThread(invTask =>
                            {
                                if (invTask.IsCompletedSuccessfully)
                                {
                                    successPanel.SetActive(true); 
                                    Destroy(storeItemObj); 
                                    Debug.Log($"'{itemName}' bought and added to inventory.");

                                }
                                else
                                {
                                    Debug.LogError("Failed to add item to inventory.");
                                }
                            });
                        }
                        else
                        {
                            Debug.LogError("Failed to update userCoins.");
                        }
                    });
                }
                else
                {
                    errorPanel.SetActive(true);
                    Debug.LogWarning("Not enough coins.");
                }
            }
        });
    }

    public void CloseSuccessPanel()
{
    successPanel.SetActive(false);
}

public void CloseErrorPanel()
{
    errorPanel.SetActive(false);
}

public void CloseConfirmationPanel()
{
    confirmationPanel.SetActive(false);
}
}
