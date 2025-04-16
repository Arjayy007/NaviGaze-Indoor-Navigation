using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class NotificationManager : MonoBehaviour
{
    public GameObject notificationCardPrefab;  // Assign your prefab in Unity Inspector
    public Transform notificationContainer;    // Assign the Scroll View's content panel

    private FirebaseFirestore firestore;

    void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        LoadNotifications();
    }

    void LoadNotifications()
    {
        string userId = UserSession.UserId;

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("[NotificationManager] User ID is null or empty.");
            return;
        }

        Debug.Log($"[NotificationManager] Fetching notifications for user: {userId}");

        firestore.Collection("users").Document(userId).Collection("notification")
    .OrderByDescending("timestamp")
    .GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("[NotificationManager] Firestore Error: " + task.Exception);
                return;
            }

            QuerySnapshot snapshot = task.Result;

            if (snapshot.Count == 0)
            {
                Debug.LogWarning("[NotificationManager] No notifications found for this user.");
                return;
            }

            // Clear previous notifications
            foreach (Transform child in notificationContainer)
            {
                Destroy(child.gameObject);
            }

            int count = 0;

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                Dictionary<string, object> data = doc.ToDictionary();

                if (data.ContainsKey("message") && data.ContainsKey("timestamp"))
                {
                    string message = data["message"].ToString();

                if (data["timestamp"] is Timestamp firestoreTimestamp)
{
    DateTime dateTime = firestoreTimestamp.ToDateTime();
    string formattedTime = dateTime.ToString("hh:mm tt"); // Full time like "09:49 PM"
    
    Debug.Log($"[NotificationManager] Notification {count + 1}: {message} at {formattedTime}");
    CreateNotificationCard(message, formattedTime);
    count++;
}

             else
             {
            Debug.LogWarning("[NotificationManager] Timestamp format is invalid.");
        }
                }
                else
                {
                    Debug.LogWarning("[NotificationManager] Skipping invalid notification entry (missing fields).");
                }
            }

            Debug.Log($"[NotificationManager] Total Notifications Displayed: {count}");
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

    if (messageText != null)
        messageText.text = message;
    else
        Debug.LogError("[NotificationManager] MessageText NOT found in prefab!");

    if (timeText != null)
        timeText.text = timestamp; // Don't split here
    else
        Debug.LogError("[NotificationManager] TimestampText NOT found in prefab!");

    Debug.Log($"[NotificationManager] Created new notification card: {newCard.name}");
}


    public void backButton()
    {
        SceneManager.LoadScene("DashboardPage");
    }
}
