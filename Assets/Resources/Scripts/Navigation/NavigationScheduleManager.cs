using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;

public class NavigationScheduleManager : MonoBehaviour
{
    private DatabaseReference dbReference;

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        RetrieveUserSchedules();
    }

    public void RetrieveUserSchedules()
    {
        if (string.IsNullOrEmpty(UserSession.UserId))
        {
            Debug.LogError("User ID is null or empty.");
            return;
        }

        DatabaseReference userSchedulesRef = dbReference.Child("users").Child(UserSession.UserId).Child("schedules");

        userSchedulesRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                int scheduleCount = 0;

                foreach (var schedule in task.Result.Children)
                {
                    scheduleCount++; // Count total schedules
                }

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
