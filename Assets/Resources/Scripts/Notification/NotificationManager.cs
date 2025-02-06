using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using System;
using TMPro; // For TextMeshPro UI support

public class NotificationManager : MonoBehaviour
{
    public GameObject notificationCardPrefab; // Assign in Inspector
    public Transform notificationContainer;  // Assign the Scroll View Content in Inspector

    private DatabaseReference dbReference;
    private string userId;

    void Start()
    {
        // Get User ID from UserSession instead of Firebase Auth
        userId = UserSession.UserId;

        if (!string.IsNullOrEmpty(userId))
        {
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;
            LoadNotifications();
        }
        else
        {
            Debug.LogWarning("User ID is null! Make sure user is logged in.");
        }
    }

    void LoadNotifications()
    {
        dbReference.Child("notifications").Child(userId).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;

                // Clear existing UI notifications
                foreach (Transform child in notificationContainer)
                {
                    Destroy(child.gameObject);
                }

                // Loop through notifications in Firebase
                foreach (var notif in snapshot.Children)
                {
                    string title = notif.Child("title").Value.ToString();
                    string message = notif.Child("message").Value.ToString();
                    string timestamp = notif.Child("timestamp").Value.ToString();
                    bool isRead = Convert.ToBoolean(notif.Child("read").Value);

                    CreateNotificationCard(notif.Key, title, message, timestamp, isRead);
                }
            }
            else
            {
                Debug.LogWarning("No notifications found for user: " + userId);
            }
        });
    }

    void CreateNotificationCard(string notifId, string title, string message, string timestamp, bool isRead)
    {
        GameObject notifCard = Instantiate(notificationCardPrefab, notificationContainer);
        notifCard.transform.Find("Title").GetComponent<TMP_Text>().text = title;
        notifCard.transform.Find("Message").GetComponent<TMP_Text>().text = message;
        notifCard.transform.Find("Timestamp").GetComponent<TMP_Text>().text = timestamp;

        // Mark as Read button
        Button markAsReadButton = notifCard.transform.Find("MarkAsReadButton").GetComponent<Button>();
        markAsReadButton.onClick.AddListener(() => MarkAsRead(notifId, notifCard));
    }

    public void MarkAsRead(string notifId, GameObject notifCard)
    {
        dbReference.Child("notifications").Child(userId).Child(notifId).Child("read").SetValueAsync(true);
        Destroy(notifCard); // Remove from UI
    }
}
