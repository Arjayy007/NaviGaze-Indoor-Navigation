using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using SchedulesModel.Models;

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
                CheckForClassNavigation();
            }
            else
            {
                Debug.LogWarning("No schedules found in Firebase.");
            }
        });
    }

    void CheckForClassNavigation()
    {
        DateTime now = DateTime.Now;
        string dayInAWeek = now.DayOfWeek.ToString();

        foreach (ScheduleData schedule in userSchedules)
        {
            if (dayInAWeek.Equals(schedule.dayOfTheWeek, StringComparison.OrdinalIgnoreCase))
            {
                DateTime classStartTime = DateTime.Parse(schedule.startTime);
                TimeSpan beforeClass = TimeSpan.FromHours(1);
                TimeSpan afterClass = TimeSpan.FromMinutes(30);

                DateTime validStartTime = classStartTime.Subtract(beforeClass);
                DateTime validEndTime = classStartTime.Add(afterClass);

                if (now >= validStartTime && now <= validEndTime)
                {
                    Debug.Log("Navigation for class.");
                }
                else
                {
                    Debug.Log("Normal navigation.");
                }
            }
        }
    }
}

