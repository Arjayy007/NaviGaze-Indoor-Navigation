using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Globalization;

public class DisplayLockedBadges : MonoBehaviour
{
    public GameObject goldBadgePrefab;
    public GameObject silverBadgePrefab;
    public GameObject bronzeBadgePrefab;
    public Transform badgeContainer;
    public Transform unlockedBadgeContainer;

    private FirebaseFirestore firestore;
    private string userId;

    private int bronzeRequirement = 0;
    private int silverRequirement = 0;
    private int goldRequirement = 0;

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
        // First, calculate the badge requirements before displaying badges
        CalculateBadgeRequirements(() =>
        {
            string[] badgeTypes = { "Gold", "Silver", "Bronze" };

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

                        // Replace {X} with actual requirement
                        int requirement = type == "Bronze" ? bronzeRequirement :
                                          type == "Silver" ? silverRequirement : goldRequirement;

                        message = message.Replace("{X}", requirement.ToString());

                        CreateBadge(type, title, message);
                    }
                    else
                    {
                        Debug.LogWarning($"Badge type '{type}' not found in Firestore.");
                    }
                });
            }

            // Check streaks and display unlocked badges
            DisplayUnlockedBadges();
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

    // NEW METHOD: Calculates the badge requirements based on total classes
    void CalculateBadgeRequirements(System.Action onComplete)
    {
        CollectionReference scheduleRef = firestore
            .Collection("users")
            .Document(userId)
            .Collection("schedules");

        scheduleRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                QuerySnapshot snapshot = task.Result;

                int totalClasses = snapshot.Count;

                bronzeRequirement = Mathf.CeilToInt(totalClasses * (1f / 3f));  // 33%
                silverRequirement = Mathf.CeilToInt(totalClasses * (2f / 3f));  // 66%
                goldRequirement = totalClasses;                                 // 100%

                Debug.Log($"[BadgeReq] Total Classes: {totalClasses}, Bronze: {bronzeRequirement}, Silver: {silverRequirement}, Gold: {goldRequirement}");

                onComplete?.Invoke(); // Proceed to show badges
            }
            else
            {
                Debug.LogError("Failed to retrieve schedule data or no schedules found.");
            }
        });
    }

    void DisplayUnlockedBadges()
    {
        // Get current week number programmatically
        int currentWeek = GetCurrentWeekNumber();

        DocumentReference streaksRef = firestore.Collection("users").Document(userId).Collection("streaks").Document("week");

        streaksRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DocumentSnapshot snapshot = task.Result;
                int streaks = snapshot.GetValue<int>(currentWeek.ToString()); // Get streaks for the current week

                // Determine which badges should be unlocked
                if (streaks >= bronzeRequirement)
                {
                    CreateUnlockedBadge(bronzeBadgePrefab);
                }

                if (streaks >= silverRequirement)
                {
                    CreateUnlockedBadge(silverBadgePrefab);
                }

                if (streaks >= goldRequirement)
                {
                    CreateUnlockedBadge(goldBadgePrefab);
                }
            }
            else
            {
                Debug.LogError("No streak data found for the current week.");
            }
        });
    }

    // Method to get the current week number of the year
    int GetCurrentWeekNumber()
    {
        DateTime currentDate = DateTime.Now;
        CultureInfo ci = CultureInfo.CurrentCulture;
        int weekNumber = ci.Calendar.GetWeekOfYear(currentDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        Debug.Log($"Current week number: {weekNumber}");
        return weekNumber;
    }

    void CreateUnlockedBadge(GameObject badgePrefab)
    {
        GameObject unlockedBadge = Instantiate(badgePrefab, unlockedBadgeContainer);
        unlockedBadge.SetActive(true);

        Image badgeImage = unlockedBadge.GetComponent<Image>();
        if (badgeImage != null)
        {
            badgeImage.color = Color.white; // No dimmed effect for unlocked badges
        }
        else
        {
            Debug.LogError("Image component not found on unlocked badge prefab!");
        }

        Debug.Log($"Unlocked badge created: {badgePrefab.name}");
    }
}
