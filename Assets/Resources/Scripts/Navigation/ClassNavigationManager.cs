using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using SchedulesModel.Models;
using System.Threading.Tasks;
using Firebase.Extensions;

public class ClassNavigationManager : MonoBehaviour
{
    private DatabaseReference dbReference;
    private string userId;
    private List<ScheduleData> userSchedules = new List<ScheduleData>();
    public CoinManager coinManager;


    void Start()
    {
        InitializeFirebase();
        
    }

    void InitializeFirebase()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
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
        Debug.Log($"Fetching schedules for user: {userId}");
        dbReference.Child("users").Child(userId).Child("schedules").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;
                userSchedules.Clear();

                foreach (var schedule in snapshot.Children)
                {
                    string subjectCode = schedule.Child("subjectCode").Value.ToString();
                    string subjectName = schedule.Child("subjectName").Value.ToString();
                    string room = schedule.Child("room").Value.ToString();
                    string dayOfTheWeek = schedule.Child("dayOfTheWeek").Value.ToString();
                    string startTime = schedule.Child("startTime").Value.ToString();
                    string endTime = schedule.Child("endTime").Value.ToString();
                    string campus = schedule.Child("campus").Value.ToString();

                    ScheduleData sched = new ScheduleData(subjectCode, subjectName, room, dayOfTheWeek, startTime, endTime, campus);
                    userSchedules.Add(sched);
                }

                Debug.Log($"Loaded {userSchedules.Count} schedules.");

            }
            else
            {
                Debug.LogWarning("No schedules found in Firebase.");
            }
        });
    }

    public void CheckForClassNavigation(string startingPoint, string destination)
    {
        DateTime now = DateTime.Now;
        string dayInAWeek = now.DayOfWeek.ToString();
        bool classFound = false;  // Tracks if at least one class navigation is detected

        foreach (ScheduleData schedule in userSchedules)
        {
            if (dayInAWeek.Equals(schedule.dayOfTheWeek, StringComparison.OrdinalIgnoreCase))
            {
                DateTime classStartTime = DateTime.Parse(schedule.startTime);
                TimeSpan beforeClass = TimeSpan.FromHours(1);
                TimeSpan afterClass = TimeSpan.FromMinutes(15);

                DateTime validStartTime = classStartTime.Subtract(beforeClass);
                DateTime validEndTime = classStartTime.Add(afterClass);

                string status = "On Time";

                if (now > classStartTime && now <= classStartTime.AddMinutes(15))
                {
                    status = "Late";
                }
                else if (now < classStartTime)
                {
                    status = "On Time";
                }

                if (now >= validStartTime && now <= validEndTime)
                {
                    string navigationType = "Class";
                    Dictionary<string, object> classNavigation = NavigationHistoryData.ClassNavigation(startingPoint, destination, navigationType, status);

                    SaveNavigationHistory(classNavigation);
                    SaveClassStreak(schedule.subjectName);

                    classFound = true;  // A valid class navigation was found
                    Debug.Log($"Class navigation saved for {schedule.subjectName} at {schedule.startTime}");
                }
            }
        }

        // If no class navigation was saved, save a normal navigation
        if (!classFound)
        {
            string navigationType = "Normal";
            Dictionary<string, object> normalNavigation = NavigationHistoryData.NormalNavigation(startingPoint, destination, navigationType);

            SaveNavigationHistory(normalNavigation);
            Debug.Log("No class found, saving normal navigation.");
        }
    }


    public void SaveNavigationHistory(Dictionary<string, object> navigationData)
    {
        dbReference.Child("users").Child(userId).Child("navigationHistory").Push().SetValueAsync(navigationData).ContinueWith(Task =>
        {
            if (Task.IsCompleted)
            {
                Debug.Log("Navigation history saved.");

                if (coinManager != null)
                {
                    coinManager.AddCoinsDirectly(10);
                    coinManager.AddExperienceDirectly(10);
                    Debug.Log("10 coins added for completing navigation.");
                    Debug.Log("10 experience added for completing navigation.");
                }
            }
            else
            { 
                Debug.LogWarning("Failed to save navigation history.");
            }
        });
    }

    public void SaveClassStreak(string className)
    {
        string currentWeek = DateTime.Now.ToString("yyyy-'W'ww"); // Example: "2025-W10" (Year-Week)

        DatabaseReference streakRef = dbReference.Child("users").Child(userId).Child("streaks").Child(className);

        streakRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                int currentStreak = int.Parse(task.Result.Value.ToString());
                int newStreak = currentStreak + 1;

                streakRef.SetValueAsync(newStreak).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompletedSuccessfully)
                    {
                        Debug.Log($"Streak updated for {className}: {newStreak}");
                    }
                    else
                    {
                        Debug.LogWarning("Failed to update streak.");
                    }
                });
            }
            else
            {
                streakRef.SetValueAsync(1).ContinueWithOnMainThread(setTask =>
                {
                    if (setTask.IsCompletedSuccessfully)
                    {
                        Debug.Log($"New streak started for {className}: 1");
                    }
                    else
                    {
                        Debug.LogWarning("Failed to start streak.");
                    }
                });
            }
        });
    }

}

