using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;

public class AccessoriesController : MonoBehaviour
{

    public GameObject itemPrefab; // Assign the prefab in the Inspector
    public Transform contentParent; // Assign the Content object of the Scroll View
    private DatabaseReference dbReference;
    private int userCoins; 
    private string userId; 
    [SerializeField] private GameObject confirmationPanel; 
    public GameObject successPanel;
    public GameObject failurePanel;
    public Button closeSuccessButton;  
    [SerializeField] private Text confirmationText; 
    [SerializeField] private Text itemNameText; 
    [SerializeField] private Text itemPriceText; 
    [SerializeField] private Image itemImage; 
    [SerializeField] private Button yesButton; 
    [SerializeField] private Button noButton; 

    void Start()
    {
        userId = UserSession.UserId; 
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is not set! Ensure the user is logged in or registered.");
            return;
        }

        // Check if the button is assigned in the Inspector
        if (closeSuccessButton != null)
        {
            closeSuccessButton.onClick.AddListener(OnCloseSuccessPanel);
        }
        else
        {
            Debug.LogError("CloseSuccessButton is not assigned in the Inspector.");
        }

        itemPrefab.SetActive(false);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                LoadItemsFromDatabase(); 
            }
            else
            {
                Debug.LogError("Could not resolve Firebase dependencies.");
            }
        });
    }

   void LoadItemsFromDatabase()
{
    string userInventoryPath = $"users/{userId}/inventory";

    // Clear the existing UI before loading new items
    ClearExistingUI();

    dbReference.GetValueAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted)
        {
            DataSnapshot snapshot = task.Result;

            // Get a list of owned items
            HashSet<string> ownedItems = new HashSet<string>();
            if (snapshot.Child(userInventoryPath).Exists)
            {
                foreach (var item in snapshot.Child(userInventoryPath).Children)
                {
                    ownedItems.Add(item.Value.ToString());
                }
            }

            // Now fetch all shop items
            dbReference.Child("items").GetValueAsync().ContinueWithOnMainThread(itemTask =>
            {
                if (itemTask.IsCompleted)
                {
                    DataSnapshot itemsSnapshot = itemTask.Result;

                    foreach (DataSnapshot item in itemsSnapshot.Children)
                    {
                        string itemName = item.Child("itemName").Value.ToString();
                        string itemPrice = item.Child("itemPrice").Value.ToString();

                        // Skip items that the user already owns
                        if (!ownedItems.Contains(itemName))
                        {
                            CreateUIItem(itemName, itemPrice);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Failed to fetch shop items from Firebase.");
                }
            });
        }
        else
        {
            Debug.LogError("Failed to fetch user inventory from Firebase.");
        }
    });
}


void ClearExistingUI()
{
    foreach (Transform child in contentParent)
    {
        Destroy(child.gameObject);
    }
    Debug.Log("Cleared all existing UI items.");
}


    void CreateUIItem(string name, string price)
    {
        
        GameObject newItem = Instantiate(itemPrefab, contentParent);

        newItem.SetActive(true);

        Text[] texts = newItem.GetComponentsInChildren<Text>();
        foreach (Text text in texts)
        {
            if (text.name == "ItemName")
            {
                text.text = name; 
            }
            else if (text.name == "ItemPrice")
            {
                text.text = "$" + price; 
            }
        }

        Sprite itemImage = Resources.Load<Sprite>($"Assets/{name}");  


        Debug.Log($"Attempting to load image for {name} from path: {name}");


        if (itemImage != null)
        {
            Image itemImageComponent = newItem.transform.Find("ItemImage").GetComponent<Image>();
            itemImageComponent.sprite = itemImage; 
        }
        else
        {
            Debug.LogWarning($"Image for {name} not found. Using placeholder.");
            Sprite placeholderImage = Resources.Load<Sprite>("placeholder"); // Assuming you have a placeholder image
            if (placeholderImage != null)
            {
                Image itemImageComponent = newItem.transform.Find("ItemImage").GetComponent<Image>();
                itemImageComponent.sprite = placeholderImage;
            }
        }

        Button buyButton = newItem.transform.Find("BuyItem").GetComponent<Button>();
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => ShowConfirmationPanel(name, price, itemImage));
        }

    }

    void ShowConfirmationPanel(string itemName, string itemPrice, Sprite itemSprite)
    {
        confirmationPanel.SetActive(true);

        confirmationText.text = "Confirm Purchase?";
        itemNameText.text = itemName;
        itemPriceText.text = "$" + itemPrice;

        if (itemSprite != null)
        {
            itemImage.sprite = itemSprite;
        }
        else
        {
            Debug.LogWarning("Item image is null. Using a placeholder.");
            itemImage.sprite = Resources.Load<Sprite>("placeholder"); 
        }

        // Set up Yes button to confirm purchase
        yesButton.onClick.RemoveAllListeners(); 
        yesButton.onClick.AddListener(() => BuyItem(itemName));

        // Set up No button to close the Confirmation Panel
        noButton.onClick.RemoveAllListeners(); 
        noButton.onClick.AddListener(() => confirmationPanel.SetActive(false));
    }

    public void BuyItem(string itemName)
    {
        // Define paths in the Firebase database
        string itemsPath = "items";
        string userCoinsPath = $"users/{userId}/userCoins"; 
        string userInventoryPath = $"users/{userId}/inventory"; 

        // Fetch item details and user coins
        dbReference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                DataSnapshot snapshot = task.Result;

                // Find the item in the database based on itemName
                DataSnapshot itemSnapshot = null;
                foreach (var item in snapshot.Child(itemsPath).Children)
                {
                    if (item.Child("itemName").Value.ToString() == itemName)
                    {
                        itemSnapshot = item;
                        break;
                    }
                }

                if (itemSnapshot == null)
                {
                    Debug.LogError($"Item with name '{itemName}' not found in the database.");
                    return;
                }

                int itemPrice = int.Parse(itemSnapshot.Child("itemPrice").Value.ToString());

                // Retrieve the user's coins from the correct path
                DataSnapshot userCoinsSnapshot = snapshot.Child("users").Child(userId).Child("userCoins");
                if (!userCoinsSnapshot.Exists)
                {
                    Debug.LogError("User's coin balance not found in the database.");
                    return;
                }
                int userCoins = int.Parse(userCoinsSnapshot.Value.ToString());

                // Check if the user has enough coins
                if (userCoins >= itemPrice)
                {
                    // Deduct the item's price from the user's coins
                    int newCoinBalance = userCoins - itemPrice;
                    dbReference.Child(userCoinsPath).SetValueAsync(newCoinBalance);

                    // Update inventory
                    UpdateInventory(snapshot, userInventoryPath, itemName);

                    confirmationPanel.SetActive(false);
                    successPanel.SetActive(true);
                    Debug.Log("Purchase successful!");
                }
                else
                {
                    confirmationPanel.SetActive(false);
                    failurePanel.SetActive(true);
                    Debug.Log("Insufficient Coins!");
                }
            }
            else
            {
                Debug.LogError("Error fetching data from Firebase: " + task.Exception);
            }
        });
    }

    private void UpdateInventory(DataSnapshot snapshot, string userInventoryPath, string itemName)
    {
        if (!snapshot.Child(userInventoryPath).Exists)
        {
            // If inventory doesn't exist, create a new inventory with the item
            Dictionary<string, string> newInventory = new Dictionary<string, string> {
            { "Item 1", itemName }
        };
            dbReference.Child(userInventoryPath).SetValueAsync(newInventory).ContinueWithOnMainThread(createTask =>
            {
                if (createTask.IsCompleted)
                {
                    Debug.Log($"Inventory created and item '{itemName}' added successfully!");
                }
                else
                {
                    Debug.LogError("Failed to create inventory: " + createTask.Exception);
                }
            });
        }
        else
        {
            // If inventory exists, add the new item
            Dictionary<string, string> inventory = new Dictionary<string, string>();
            int index = 1;

            foreach (var item in snapshot.Child(userInventoryPath).Children)
            {
                inventory.Add($"Item {index}", item.Value.ToString());
                index++;
            }

            inventory.Add($"Item {index}", itemName);

            dbReference.Child(userInventoryPath).SetValueAsync(inventory).ContinueWithOnMainThread(updateTask =>
            {
                if (updateTask.IsCompleted)
                {
                    Debug.Log($"Item '{itemName}' purchased and added to inventory successfully!");
                }
                else
                {
                    Debug.LogError("Failed to update inventory: " + updateTask.Exception);
                }
            });
        }
    }

    void RemoveItemFromScrollView(string itemName)
    {
        foreach (Transform child in contentParent)
        {
            if (child.name == itemName)
            {
                Destroy(child.gameObject);
                Debug.Log($"Removed item {itemName} from the ScrollView.");
            }
        }
    }

    void OnCloseSuccessPanel()
    {
        successPanel.SetActive(false);
        RefreshInventory();
    }

    void RefreshInventory()
    {
        // Clear existing items in the Scroll View
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Fetch the updated inventory again
        LoadItemsFromDatabase();
    }

    public void CloseConfirmationPanel() 
    {
        confirmationPanel.SetActive(false);
    }

    public void CloseSuccessPanel() 
    {
        successPanel.SetActive(false);
    }

    public void CloseFailurePanel() 
    {
        failurePanel.SetActive(false);
    }
}


