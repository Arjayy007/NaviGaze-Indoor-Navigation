using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using SchedulesModel.Models;
using NotificationModel.Models;
using Firebase;
using Firebase.Extensions;

public class ScheduleNotificationManager : MonoBehaviour
{
    private DatabaseReference dbReference;
    private string userId;
    private List<ScheduleData> userSchedules = new List<ScheduleData>();
    private List<NotificationData> userNotifications = new List<NotificationData>();


    void Awake()
    {
        DontDestroyOnLoad(gameObject); // Make this object persistent
    }

    void Start()
    {
        // Check Firebase dependencies asynchronously and initialize Firebase only after it's ready
        FirebaseApp.CheckDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            DependencyStatus status = task.Result;
            if (status == DependencyStatus.Available)
            {
                // Firebase is ready, initialize Firebase and load user schedules
                InitializeFirebase();
            }
            else
            {
                // Firebase is not ready
                Debug.LogError($"Firebase dependencies are not ready: {status}");
            }
        });
    }

    void InitializeFirebase()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;  // Initialize Firebase Database
        userId = UserSession.UserId;

        if (!string.IsNullOrEmpty(userId))
        {
            LoadUserSchedules(); // Load schedules after Firebase is initialized
             LoadUserNotifications();
            StartCoroutine(CheckScheduleRoutine()); // Start checking schedules every minute
        }
        else
        {
            Debug.LogWarning("User ID is null! Cannot load schedules.");
        }
    }

void LoadUserSchedules()
{
    // Fetch user schedules from Firebase based on the updated structure
    Debug.Log($"Fetching data from path: users/{userId}/schedules"); // Print the full path to check
    dbReference.Child("users").Child(userId).Child("schedules").GetValueAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted && task.Result.Exists)
        {
            DataSnapshot snapshot = task.Result;
            userSchedules.Clear(); // Clear previous schedules

            Debug.Log("Schedules fetched successfully from Firebase:");

            foreach (var schedule in snapshot.Children)
            {
                // Debug: print each schedule's data
                string subjectCode = schedule.Child("subjectCode").Value.ToString();
                string subjectName = schedule.Child("subjectName").Value.ToString();
                string room = schedule.Child("room").Value.ToString();
                string dayOfTheWeek = schedule.Child("dayOfTheWeek").Value.ToString();
                string startTime = schedule.Child("startTime").Value.ToString();
                string endTime = schedule.Child("endTime").Value.ToString();
                string campus = schedule.Child("campus").Value.ToString();

                Debug.Log($"Subject: {subjectName}, Room: {room}, Day: {dayOfTheWeek}, StartTime: {startTime}, EndTime: {endTime}, Campus: {campus}");

                ScheduleData sched = new ScheduleData(subjectCode, subjectName, room, dayOfTheWeek, startTime, endTime, campus);
                userSchedules.Add(sched);
            }
        }
        else
        {
            Debug.LogWarning("No schedules found in Firebase.");
        }
    });
}

void LoadUserNotifications()
{
    dbReference.Child("notifications").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted)
        {
            userNotifications.Clear(); // Clear before updating

            if (task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;
                foreach (var notification in snapshot.Children)
                {
                    string message = notification.Child("message").Value.ToString();
                    string timestamp = notification.Child("timestamp").Value.ToString();
                    bool isRead = Convert.ToBoolean(notification.Child("isRead").Value);
                    bool isSent = Convert.ToBoolean(notification.Child("isNotificationSent").Value);
                    int notificationWeek = Convert.ToInt32(notification.Child("notificationWeek").Value);

                    userNotifications.Add(new NotificationData(message, timestamp, isRead, isSent, notificationWeek));
                }
            }
            Debug.Log($"Loaded {userNotifications.Count} notifications.");
        }
        else
        {
            Debug.LogWarning("No notifications found in Firebase.");
        }
    });
}




IEnumerator CheckScheduleRoutine()
{
    while (true)
    {
        Debug.Log("Checking for upcoming classes..."); // Add this debug log
        CheckForUpcomingClasses();
        yield return new WaitForSeconds(30); // Check every 1 minute
    }
}


void CheckForUpcomingClasses()
{
    DateTime now = DateTime.Now; // Get current system time

    string dayInAWeek = now.DayOfWeek.ToString(); // Get the current day of the week as a string
    // Debug: Print the current timestamp and day of the week every time the check runs
    Debug.Log($"Current system date and time: {now.ToString("yyyy-MM-dd HH:mm:ss")}");
    Debug.Log($"Current system day of the week: {dayInAWeek}");

    foreach (ScheduleData schedule in userSchedules)
    {
        string scheduleDay = schedule.dayOfTheWeek.Trim(); // Ensure no extra spaces
        if (dayInAWeek.Equals(scheduleDay, StringComparison.OrdinalIgnoreCase)) // Check if the schedule is for today
        {
            DateTime classStartTime = ParseTime(schedule.startTime);
            DateTime notificationTime = classStartTime.AddMinutes(-10); // 10 minutes before class

            if (now >= notificationTime && now < classStartTime) // If it's time to notify
            {
                // Only attempt to save a notification if not already done
                if (!IsNotificationSent(schedule.subjectName))
                {
                    // Debug: Only print when notification is saved
                    Debug.Log($"Notification for {schedule.subjectName} class is being saved.");
                    SaveNotification(schedule.subjectName, schedule.room, schedule.startTime);
                }
                else
                {
                    Debug.Log($"Notification already sent this week for {schedule.subjectName}.");
                }
            }
        }
    }
}

bool IsNotificationSent(string subject)
{
    int currentWeek = GetCurrentWeekNumber(DateTime.Now);
    foreach (var notification in userNotifications)
    {
        // Check if the message contains the subject name and if the notification is already sent this week
        if (notification.message.Contains(subject) && notification.notificationWeek == currentWeek && notification.isNotificationSent)
        {
            return true;
        }
    }
    return false;
}




void SaveNotification(string subject, string room, string startTime)
{
    string message = $"{subject} in {room} starts at {startTime}. Get ready!";
    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    int currentWeek = GetCurrentWeekNumber(DateTime.Now);

    // Check if this specific notification (same subject & start time) already exists for this week
    bool notificationExists = false;
    foreach (var notification in userNotifications)
    {
        if (notification.message.Contains(subject) && notification.message.Contains(startTime) &&
            notification.notificationWeek == currentWeek && notification.isNotificationSent)
        {
            notificationExists = true;
            break; // Exit loop early
        }
    }

    if (!notificationExists)
    {
        // Create a new notification object
        NotificationData newNotification = new NotificationData(message, timestamp, false, true, currentWeek);

        // Generate a new unique key in Firebase
        string notifKey = dbReference.Child("notifications").Child(userId).Push().Key;
        
        // Save the notification in Firebase
        dbReference.Child("notifications").Child(userId).Child(notifKey)
            .SetRawJsonValueAsync(JsonUtility.ToJson(newNotification))
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"Notification saved: {message}");
                    
                    // Update local notification list immediately to prevent duplicates
                    userNotifications.Add(newNotification);
                }
                else
                {
                    Debug.LogError("Failed to save notification in Firebase.");
                }
            });
    }
    else
    {
        Debug.Log($"Notification already sent this week for: {subject} at {startTime}.");
    }
}





int GetCurrentWeekNumber(DateTime date)
{
    var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;
    return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
}

DateTime ParseTime(string time)
{
    // Parse the time in 12-hour format (e.g., "11:00 PM") and convert to 24-hour format (e.g., "23:00:00")
    return DateTime.ParseExact(time, "h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
}



}
