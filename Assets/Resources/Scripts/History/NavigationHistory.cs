using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;

public class NavigationHistory : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject navigationHistoryCardPrefab;  // Assign your prefab in Unity Inspector
    public Transform navigationHistoryContainer;   // Assign the Scroll View's content panel
    private DatabaseReference dbReference;

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        LoadNotifications();
        
    }

    void LoadNotifications()
    {
        string userId = UserSession.UserId; // Assuming you have user authentication

        Debug.Log($"[NotificationManager] Fetching navigation history for user: {userId}");

        dbReference.Child("users").Child(userId).Child("navigationHistory").GetValueAsync().ContinueWithOnMainThread(task =>
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
                    Debug.LogWarning("[NotificationManager] No navigation history found for this user.");
                    return;
                }

                // Print the raw JSON retrieved
                Debug.Log("[History] Raw JSON from Firebase: " + snapshot.GetRawJsonValue());

                // Clear previous history
                foreach (Transform child in navigationHistoryContainer)
                {
                    Destroy(child.gameObject);
                }

                int count = 0;

                foreach (var child in snapshot.Children)
                {
                    if (child.HasChild("startingPoint") && child.HasChild("destination") && child.HasChild("timestamp"))
                    {
                        string startingCube = child.Child("startingPoint").Value.ToString();
                        string endCube = child.Child("destination").Value.ToString();
                        string timestamp = child.Child("timestamp").Value.ToString();

                        // Convert date format to Month Name
                        string[] dateTimeParts = timestamp.Split(' '); 
                        if (dateTimeParts.Length >= 3) // Ensure valid split
                        {
                            string originalDate = dateTimeParts[0]; // Extract "04/03/2025"
                            string navigationTime = dateTimeParts[1] + " " + dateTimeParts[2]; // Extract "7:07:48 PM"

                            // Convert "MM/dd/yyyy" to "Month dd, yyyy" (e.g., "March 04, 2025")
                            string navigationDate = ConvertDateToMonthName(originalDate);

                            Debug.Log($"[NotificationManager] Navigation {count + 1}: {startingCube} → {endCube} at {navigationDate} {navigationTime}");

                            CreateNotificationCard(startingCube, endCube, navigationDate, navigationTime);
                            count++;
                        }
                        else
                        {
                            Debug.LogWarning($"[NotificationManager] Invalid timestamp format: {timestamp}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[NotificationManager] Skipping invalid history entry (missing fields).");
                    }
                }

                Debug.Log($"[NotificationManager] Total Navigation History Displayed: {count}");
            }
        });
    }

    string ConvertDateToMonthName(string date)
    {
        try
        {
            // Parse the date (Format: MM/dd/yyyy)
            DateTime parsedDate = DateTime.ParseExact(date, "MM/dd/yyyy", null);

            // Convert to "Month dd, yyyy" format
            return parsedDate.ToString("MMMM dd, yyyy"); // Example: "March 04, 2025"
        }
        catch (FormatException)
        {
            Debug.LogError($"[NotificationManager] Error parsing date: {date}");
            return date; // Return original if parsing fails
        }
    }

    void CreateNotificationCard(string startPoint, string endPoint, string navigationDate, string navigationTime)
    {
        if (navigationHistoryCardPrefab == null || navigationHistoryContainer == null)
        {
            Debug.LogError("[NotificationManager] navigationHistoryCardPrefab or navigationHistoryContainer is NOT assigned!");
            return;
        }

        GameObject newCard = Instantiate(navigationHistoryCardPrefab, navigationHistoryContainer);
        newCard.SetActive(true);

        TextMeshProUGUI start = newCard.transform.Find("startingPoint")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI end = newCard.transform.Find("destinationPoint")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI date = newCard.transform.Find("Date")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI time = newCard.transform.Find("Time")?.GetComponent<TextMeshProUGUI>();

        if (start != null) start.text = startPoint;
        else Debug.LogError("[NotificationManager] StartingPointText NOT found in prefab!");

        if (end != null) end.text = endPoint;
        else Debug.LogError("[NotificationManager] DestinationPointText NOT found in prefab!");

        if (date != null) date.text = navigationDate;
        else Debug.LogError("[NotificationManager] DateText NOT found in prefab!");

        if (time != null) time.text = navigationTime;
        else Debug.LogError("[NotificationManager] TimeText NOT found in prefab!");

        Debug.Log($"[NotificationManager] Created new navigation history card: {newCard.name}");
    }


}
