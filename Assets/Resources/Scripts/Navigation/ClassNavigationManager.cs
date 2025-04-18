using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using SchedulesModel.Models;

public class ClassNavigationManager : MonoBehaviour
{
    private FirebaseFirestore firestore;
    private string userId;
    private List<ScheduleData> userSchedules = new List<ScheduleData>();
    public CoinManager coinManager;


   void Start()
    {
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        userId = UserSession.UserId;

        if (!string.IsNullOrEmpty(userId))
        {
            LoadUserSchedules();
        }
        else
        {
            Debug.LogWarning("User ID is null! Cannot load schedules.");
        }
    }

    void LoadUserSchedules()
    {
        Debug.Log($"[Firestore] Fetching schedules for user: {userId}");

        CollectionReference scheduleRef = firestore.Collection("users").Document(userId).Collection("schedules");
        scheduleRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;
                userSchedules.Clear();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    Dictionary<string, object> data = document.ToDictionary();

                    string subjectCode = data["subjectCode"].ToString();
                    string subjectName = data["subjectName"].ToString();
                    string room = data["room"].ToString();
                    string dayOfTheWeek = data["dayOfTheWeek"].ToString();
                    string startTime = data["startTime"].ToString();
                    string endTime = data["endTime"].ToString();
                    string campus = data["campus"].ToString();

                    ScheduleData sched = new ScheduleData(subjectCode, subjectName, room, dayOfTheWeek, startTime, endTime, campus);
                    userSchedules.Add(sched);
                }

                Debug.Log($"[Firestore] Loaded {userSchedules.Count} schedules.");
            }
            else
            {
                Debug.LogWarning("[Firestore] Failed to fetch schedules.");
            }
        });
    }

    public void CheckForClassNavigation(string startingPoint, string destination)
    {
        DateTime now = DateTime.Now;
        string dayInAWeek = now.DayOfWeek.ToString();
        bool classFound = false;

        foreach (ScheduleData schedule in userSchedules)
        {
            if (dayInAWeek.Equals(schedule.dayOfTheWeek, StringComparison.OrdinalIgnoreCase))
            {
                DateTime classStartTime = DateTime.Parse(schedule.startTime);
                TimeSpan beforeClass = TimeSpan.FromHours(1);
                TimeSpan afterClass = TimeSpan.FromMinutes(15);

                DateTime validStartTime = classStartTime.Subtract(beforeClass);
                DateTime validEndTime = classStartTime.Add(afterClass);

                string status = now > classStartTime && now <= classStartTime.AddMinutes(15) ? "Late" : "On Time";

                if (now >= validStartTime && now <= validEndTime)
                {
                    string navigationType = "Class";
                    Dictionary<string, object> classNavigation = NavigationHistoryData.ClassNavigation(startingPoint, destination, navigationType, status);

                    SaveNavigationHistory(classNavigation);
                    SaveClassStreak();
                    classFound = true;

                    Debug.Log($"[Firestore] Class navigation saved for {schedule.subjectName} at {schedule.startTime}");
                }
            }
        }

        if (!classFound)
        {
            string navigationType = "Normal";
            Dictionary<string, object> normalNavigation = NavigationHistoryData.NormalNavigation(startingPoint, destination, navigationType);
            SaveNavigationHistory(normalNavigation);

            Debug.Log("[Firestore] No class found, saving normal navigation.");
        }
    }

    public void SaveNavigationHistory(Dictionary<string, object> navigationData)
    {
        DocumentReference docRef = firestore
            .Collection("users")
            .Document(userId)
            .Collection("navigationHistory")
            .Document(); // Auto-generated ID

        docRef.SetAsync(navigationData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("[Firestore] Navigation history saved.");

                if (coinManager != null)
                {
                    coinManager.AddCoinsDirectly(10);
                    coinManager.AddExperienceDirectly(10);
                    Debug.Log("[Rewards] 10 coins and 10 XP awarded.");
                }
            }
            else
            {
                Debug.LogWarning("[Firestore] Failed to save navigation history.");
            }
        });
    }

   public void SaveClassStreak()
{
    string currentWeekNumber = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
        DateTime.Now,
        System.Globalization.CalendarWeekRule.FirstFourDayWeek,
        DayOfWeek.Monday
    ).ToString(); // Example: "8"

    DocumentReference weekRef = firestore.Collection("users").Document(userId).Collection("streaks").Document("week");

    weekRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted)
        {
            DocumentSnapshot snapshot = task.Result;
            Dictionary<string, object> updateData = new Dictionary<string, object>();

            if (snapshot.Exists && snapshot.ContainsField(currentWeekNumber))
            {
                int currentStreak = snapshot.GetValue<int>(currentWeekNumber);
                updateData[currentWeekNumber] = currentStreak + 1;
            }
            else
            {
                updateData[currentWeekNumber] = 1;
            }

            weekRef.SetAsync(updateData, SetOptions.MergeAll).ContinueWithOnMainThread(setTask =>
            {
                if (setTask.IsCompletedSuccessfully)
                {
                    Debug.Log($"Streak for week {currentWeekNumber} updated.");
                }
                else
                {
                    Debug.LogWarning($"Failed to update streak for week {currentWeekNumber}.");
                }
            });
        }
        else
        {
            Debug.LogWarning("Failed to fetch streak document.");
        }
    });
}

}