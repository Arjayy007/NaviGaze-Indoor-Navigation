using UnityEngine;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;

public class DisplayLockedBadges : MonoBehaviour
{
    public GameObject goldBadgePrefab;
    public GameObject silverBadgePrefab;
    public GameObject bronzeBadgePrefab;
    public Transform badgeContainer; // Assign in Unity to place badge prefabs inside

    private DatabaseReference dbReference;
    private string userId;

    void Start()
    {
        userId = UserSession.UserId; // Ensure UserSession.UserId is correctly set
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is not set! Ensure the user is logged in.");
            return;
        }

        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        FetchBadges();
    }

    void FetchBadges()
    {
        string badgesPath = "badges"; // Path to badges in Firebase

        dbReference.Child(badgesPath).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    Debug.LogError("No badge data found in Firebase.");
                    return;
                }

                // Clear previous badges (if any)
                foreach (Transform child in badgeContainer)
                {
                    Destroy(child.gameObject);
                }

                int count = 0;

                foreach (var badge in snapshot.Children)
                {
                    string badgeKey = badge.Key; // e.g., "Bronze", "Silver", "Gold"
                    string title = badge.Child("title").Value.ToString();
                    string message = badge.Child("message").Value.ToString();

                    CreateBadge(badgeKey, title, message);
                    count++;
                }

                Debug.Log($"Total Badges Displayed: {count}");
            }
            else
            {
                Debug.LogError("Failed to fetch badges from Firebase: " + task.Exception);
            }
        });
    }

    void CreateBadge(string badgeType, string title, string message)
    {
        GameObject badgePrefab = null;

        switch (badgeType)
        {
            case "Gold":
                badgePrefab = goldBadgePrefab;
                break;
            case "Silver":
                badgePrefab = silverBadgePrefab;
                break;
            case "Bronze":
                badgePrefab = bronzeBadgePrefab;
                break;
            default:
                Debug.LogWarning($"Unknown badge type: {badgeType}");
                return;
        }

        if (badgePrefab == null)
        {
            Debug.LogError($"Prefab for {badgeType} badge is not assigned!");
            return;
        }

        GameObject newBadge = Instantiate(badgePrefab, badgeContainer);
        newBadge.SetActive(true);

        TextMeshProUGUI titleText = newBadge.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI messageText = newBadge.transform.Find("Message")?.GetComponent<TextMeshProUGUI>();

        if (titleText != null) titleText.text = title;
        else Debug.LogError($"Title text not found in {badgeType} badge prefab!");

        if (messageText != null) messageText.text = message;
        else Debug.LogError($"Message text not found in {badgeType} badge prefab!");

        Debug.Log($"Created {badgeType} badge: {title}");
    }
}
