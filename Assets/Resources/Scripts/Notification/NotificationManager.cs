using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    public GameObject notificationCardPrefab;  // Assign your prefab in Unity Inspector
    public Transform notificationContainer;   // Assign the Scroll View's content panel
    private DatabaseReference dbReference;

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        LoadNotifications();
        
    }

    void LoadNotifications()
    {
        string userId = UserSession.UserId; // Assuming you have user authentication

        Debug.Log($"[NotificationManager] Fetching notifications for user: {userId}");

        dbReference.Child("notifications").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("[NotificationManager] Firebase Database Error: " + task.Exception);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    Debug.LogWarning("[NotificationManager] No notifications found for this user.");
                    return;
                }

                // Print the raw JSON retrieved
                Debug.Log("[NotificationManager] Raw JSON from Firebase: " + snapshot.GetRawJsonValue());

                // Clear previous notifications
                foreach (Transform child in notificationContainer)
                {
                    Destroy(child.gameObject);
                }

                int count = 0;

                foreach (var child in snapshot.Children)
                {
                    if (child.HasChild("message") && child.HasChild("timestamp"))
                    {
                        string message = child.Child("message").Value.ToString();
                        string timestamp = child.Child("timestamp").Value.ToString();

                        Debug.Log($"[NotificationManager] Notification {count + 1}: {message} at {timestamp}");

                        CreateNotificationCard(message, timestamp);
                        count++;
                    }
                    else
                    {
                        Debug.LogWarning("[NotificationManager] Skipping invalid notification entry (missing fields).");
                    }
                }

                Debug.Log($"[NotificationManager] Total Notifications Displayed: {count}");
            }
        });
    }

  void CreateNotificationCard(string message, string timestamp)
{
    if (notificationCardPrefab == null || notificationContainer == null)
    {
        Debug.LogError("[NotificationManager] notificationCardPrefab or notificationContainer is NOT assigned!");
        return;
    }

    GameObject newCard = Instantiate(notificationCardPrefab, notificationContainer);
    newCard.SetActive(true);

    TextMeshProUGUI messageText = newCard.transform.Find("Message")?.GetComponent<TextMeshProUGUI>();
    TextMeshProUGUI timeText = newCard.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();

    if (messageText != null) messageText.text = message;
    else Debug.LogError("[NotificationManager] MessageText NOT found in prefab!");

    if (timeText != null) timeText.text = timestamp.Split(' ')[1]; // Extract only the time part
    else Debug.LogError("[NotificationManager] TimestampText NOT found in prefab!");

    Debug.Log($"[NotificationManager] Created new notification card: {newCard.name}");
}


}
