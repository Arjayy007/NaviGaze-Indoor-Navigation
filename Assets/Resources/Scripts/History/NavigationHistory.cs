using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using Firebase.Firestore;
public class NavigationHistory : MonoBehaviour
{

    public GameObject navigationHistoryCardPrefab;  
    public Transform navigationHistoryContainer;  
    private FirebaseFirestore firestore;

    void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        LoadNavigationHistory();
        
    }

 void LoadNavigationHistory()
    {
        string userId = UserSession.UserId;

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("[NavigationHistory] User ID is null or empty.");
            return;
        }

        Debug.Log($"[NavigationHistory] Fetching navigation history for user: {userId}");

        firestore.Collection("users").Document(userId).Collection("navigationHistory")
        .OrderByDescending("timestamp")
        .GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("[NavigationHistory] Firestore Error: " + task.Exception);
                return;
            }

            QuerySnapshot snapshot = task.Result;

            if (snapshot.Count == 0)
            {
                Debug.LogWarning("[NavigationHistory] No navigation history found.");
                return;
            }

            foreach (Transform child in navigationHistoryContainer)
            {
                Destroy(child.gameObject);
            }

            int count = 0;

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                Dictionary<string, object> data = doc.ToDictionary();

                if (data.ContainsKey("startingPoint") && data.ContainsKey("destination") && data.ContainsKey("timestamp"))
                {
                    string startingCube = data["startingPoint"].ToString();
                    string endCube = data["destination"].ToString();
                    string navigationType = data.ContainsKey("navigationType") ? data["navigationType"].ToString() : "Default";

                    Timestamp timestampObj = (Timestamp)data["timestamp"];

                    string navigationDate = "";
                    string navigationTime = "";

                    if (timestampObj != null)
                    {
                        DateTime time = timestampObj.ToDateTime();
                        navigationDate = time.ToString("MMMM dd, yyyy"); 
                        navigationTime = time.ToString("hh:mm tt");      
                    }

                    Debug.Log($"[NavigationHistory] {startingCube} → {endCube} at {navigationDate} {navigationTime}");

                    CreateNotificationCard(startingCube, endCube, navigationDate, navigationTime);
                    count++;
                }
                else
                {
                    Debug.LogWarning("[NavigationHistory] Skipping invalid entry (missing fields).");
                }
            }

            Debug.Log($"[NavigationHistory] Total Navigation History Displayed: {count}");
        });
    }


    string ConvertDateToMonthName(string date)
    {
        try
        {
   
            DateTime parsedDate = DateTime.ParseExact(date, "MM/dd/yyyy", null);

     
            return parsedDate.ToString("MMMM dd, yyyy"); 
        }
        catch (FormatException)
        {
            Debug.LogError($"[NotificationManager] Error parsing date: {date}");
            return date; 
        }
    }

     void CreateNotificationCard(string startPoint, string endPoint, string navigationDate, string navigationTime)
    {
        if (navigationHistoryCardPrefab == null || navigationHistoryContainer == null)
        {
            Debug.LogError("[NavigationHistory] Prefab or Container is not assigned!");
            return;
        }

        GameObject newCard = Instantiate(navigationHistoryCardPrefab, navigationHistoryContainer);
        newCard.SetActive(true);

        TextMeshProUGUI start = newCard.transform.Find("startingPoint")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI end = newCard.transform.Find("destinationPoint")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI date = newCard.transform.Find("Date")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI time = newCard.transform.Find("Time")?.GetComponent<TextMeshProUGUI>();

        if (start != null) start.text = startPoint;
        if (end != null) end.text = endPoint;
        if (date != null) date.text = navigationDate;
        if (time != null) time.text = navigationTime;

        Debug.Log($"[NavigationHistory] Created card for: {startPoint} → {endPoint}");
    }
}
