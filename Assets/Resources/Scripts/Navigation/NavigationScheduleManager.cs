using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class NavigationScheduleManager : MonoBehaviour
{
    private FirebaseFirestore firestore;

    void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        RetrieveUserSchedules();
    }

    public void RetrieveUserSchedules()
    {
        if (string.IsNullOrEmpty(UserSession.UserId))
        {
            Debug.LogError("User ID is null or empty.");
            return;
        }

        CollectionReference userSchedulesRef = firestore
            .Collection("users")
            .Document(UserSession.UserId)
            .Collection("schedules");

        userSchedulesRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Count > 0)
            {
                int scheduleCount = task.Result.Count;

                // Divide schedules into streak categories
                int bronzeStreak, silverStreak, goldStreak;
                CalculateStreaks(scheduleCount, out bronzeStreak, out silverStreak, out goldStreak);

                Debug.Log($"Total Schedules: {scheduleCount}");
                Debug.Log($"Bronze Streak: {bronzeStreak}, Silver Streak: {silverStreak}, Gold Streak: {goldStreak}");
            }
            else
            {
                Debug.LogWarning("No schedules found for the user.");
            }
        });
    }

    private void CalculateStreaks(int totalSchedules, out int bronzeStreak, out int silverStreak, out int goldStreak)
    {
        bronzeStreak = totalSchedules / 3; 
        silverStreak = bronzeStreak;
        goldStreak = bronzeStreak;

        int remainder = totalSchedules % 3; 

        if (remainder == 1)
        {
            goldStreak += 1; 
        }
        else if (remainder == 2)
        {
            silverStreak += 1; 
            goldStreak += 1; 
        }
    }
}
