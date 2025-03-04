using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using SchedulesModel.Models;
using System.Threading.Tasks;

public class ClassNavigationManager : MonoBehaviour
{
    private DatabaseReference dbReference;
    private string userId;
    private List<ScheduleData> userSchedules = new List<ScheduleData>();

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
        bool classNavigationSaved = false; // Track if class navigation is saved

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
                    status = "Late"; // Class hasn't started yet
                }
                else if  (now < classStartTime)
                {
                    status = "On Time"; // If student is within 15 minutes after start time, mark as late
                }

                // If within valid time range, save navigation
                if (now >= validStartTime && now <= validEndTime)
                {
                    string navigationType = "Class";
                    Dictionary<string, object> classNavigation = NavigationHistoryData.ClassNavigation(startingPoint, destination, navigationType, status);

                    SaveNavigationHistory(classNavigation);
                    classNavigationSaved = true;

                    Debug.Log($"Class navigation saved for {schedule.subjectName} at {schedule.startTime}");
                }
            }
        }

        // If no class navigation was saved, save a normal navigation
        if (!classNavigationSaved)
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
            }
            else
            {
                Debug.LogWarning("Failed to save navigation history.");
            }
        });
    }

}

