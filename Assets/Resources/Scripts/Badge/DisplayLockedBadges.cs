using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;

public class DisplayLockedBadges : MonoBehaviour
{
    public GameObject goldBadgePrefab;
    public GameObject silverBadgePrefab;
    public GameObject bronzeBadgePrefab;
    public Transform badgeContainer;

    private FirebaseFirestore firestore;
    private string userId;

    void Start()
    {
        userId = UserSession.UserId;

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is not set! Ensure the user is logged in.");
            return;
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                firestore = FirebaseFirestore.DefaultInstance;
                FetchBadges();
            }
            else
            {
                Debug.LogError("Firebase dependencies not resolved: " + task.Result);
            }
        });
    }

    void FetchBadges()
    {
        string[] badgeTypes = { "Gold", "Silver", "Bronze" };
        int count = 0;

        foreach (string type in badgeTypes)
        {
            DocumentReference docRef = firestore.Collection("badges").Document(type);

            docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    DocumentSnapshot snapshot = task.Result;

                    string title = snapshot.ContainsField("title") ? snapshot.GetValue<string>("title") : "No Title";
                    string message = snapshot.ContainsField("message") ? snapshot.GetValue<string>("message") : "No Message";

                    CreateBadge(type, title, message);
                    count++;
                }
                else
                {
                    Debug.LogWarning($"Badge type '{type}' not found in Firestore.");
                }
            });
        }
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

        Image badgeImage = newBadge.GetComponent<Image>();
        if (badgeImage != null)
        {
            badgeImage.color = new Color(0.5f, 0.5f, 0.5f); // Dimmed effect for locked badges
        }
        else
        {
            Debug.LogError($"Image component not found on {badgeType} badge prefab!");
        }

        TextMeshProUGUI titleText = newBadge.transform.Find("BadgeTitle")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI messageText = newBadge.transform.Find("Message")?.GetComponent<TextMeshProUGUI>();

        if (titleText != null) titleText.text = title;
        else Debug.LogError($"BadgeTitle not found in {badgeType} prefab!");

        if (messageText != null) messageText.text = message;
        else Debug.LogError($"Message not found in {badgeType} prefab!");

        Debug.Log($"Created {badgeType} badge: {title}");
    }
}
