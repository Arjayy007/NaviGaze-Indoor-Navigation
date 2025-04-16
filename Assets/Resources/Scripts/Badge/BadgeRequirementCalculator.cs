using System;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class BadgeRequirementCalculator : MonoBehaviour
{
    private FirebaseFirestore firestore;

    void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        string userId = UserSession.UserId;

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is not set. Ensure the user is logged in.");
            return;
        }

        CountUserClasses(userId);
    }

    void CountUserClasses(string userId)
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

                int bronzeRequirement = Mathf.CeilToInt(totalClasses / 3.0f);
                int silverRequirement = Mathf.CeilToInt((totalClasses - bronzeRequirement) / 2.0f);
                int goldRequirement = totalClasses;

                Debug.Log($"Total Classes: {totalClasses}");
                Debug.Log($"Bronze Requirement: {bronzeRequirement}");
                Debug.Log($"Silver Requirement: {silverRequirement}");
                Debug.Log($"Gold Requirement: {goldRequirement}");
            }
            else
            {
                Debug.LogError("Failed to retrieve schedule data or no schedules found.");
            }
        });
    }
}
