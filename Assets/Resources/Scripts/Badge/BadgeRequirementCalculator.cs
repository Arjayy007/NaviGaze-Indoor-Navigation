using System;
using Firebase.Database;
using UnityEngine;

public class BadgeRequirementCalculator : MonoBehaviour
{
    private DatabaseReference dbReference;
    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        string userId = UserSession.UserId;
        CountUserClasses(userId);
    }

    void CountUserClasses(string userId)
    {
        dbReference.Child("users").Child(userId).Child("schedules").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                int totalClasses = (int)task.Result.ChildrenCount;

                // Calculate badge requirements
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
