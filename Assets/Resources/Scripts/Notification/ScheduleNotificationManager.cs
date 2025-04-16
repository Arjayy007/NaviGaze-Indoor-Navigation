using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using SchedulesModel.Models;
using NotificationModel.Models;

public class ScheduleNotificationManager : MonoBehaviour
{
    private FirebaseFirestore firestore;
    private string userId;
    private List<ScheduleData> userSchedules = new List<ScheduleData>();
    private List<NotificationData> userNotifications = new List<NotificationData>();

    public InAppNotification inAppNotification;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (inAppNotification == null)
        {
            inAppNotification = FindObjectOfType<InAppNotification>();
        }
    }

    void Start()
    {
        FirebaseApp.CheckDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Firebase dependencies are not ready.");
            }
        });
    }

    void InitializeFirebase()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        userId = UserSession.UserId;

        if (!string.IsNullOrEmpty(userId))
        {
            LoadUserSchedules();
            LoadUserNotifications();
            StartCoroutine(CheckScheduleRoutine());
        }
        else
        {
            Debug.LogWarning("User ID is null!");
        }
    }

    void LoadUserSchedules()
    {
        firestore.Collection("users").Document(userId).Collection("schedules").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result != null)
            {
                userSchedules.Clear();
                foreach (var doc in task.Result.Documents)
                {
                    var schedule = doc.ToDictionary();
                    ScheduleData sched = new ScheduleData(
                        schedule["subjectCode"].ToString(),
                        schedule["subjectName"].ToString(),
                        schedule["room"].ToString(),
                        schedule["dayOfTheWeek"].ToString(),
                        schedule["startTime"].ToString(),
                        schedule["endTime"].ToString(),
                        schedule["campus"].ToString()
                    );
                    userSchedules.Add(sched);
                }
                Debug.Log($"Loaded {userSchedules.Count} schedules.");
            }
            else
            {
                Debug.LogWarning("No schedules found.");
            }
        });
    }

    void LoadUserNotifications()
    {
        firestore.Collection("users").Document(userId).Collection("notifications").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result != null)
            {
                userNotifications.Clear();
                foreach (var doc in task.Result.Documents)
                {
                    var notif = doc.ToDictionary();
                    NotificationData newNotif = new NotificationData(
                        notif["message"].ToString(),
                        notif["timestamp"].ToString(),
                        Convert.ToBoolean(notif["isRead"]),
                        Convert.ToBoolean(notif["isNotificationSent"]),
                        Convert.ToInt32(notif["notificationWeek"])
                    );
                    userNotifications.Add(newNotif);
                }
                Debug.Log($"Loaded {userNotifications.Count} notifications.");
            }
            else
            {
                Debug.LogWarning("No notifications found.");
            }
        });
    }

    IEnumerator CheckScheduleRoutine()
    {
        while (true)
        {
            CheckForUpcomingClasses();
            yield return new WaitForSeconds(30);
        }
    }

    void CheckForUpcomingClasses()
    {
        DateTime now = DateTime.Now;
        string dayInAWeek = now.DayOfWeek.ToString();

        foreach (ScheduleData schedule in userSchedules)
        {
            if (dayInAWeek.Equals(schedule.dayOfTheWeek, StringComparison.OrdinalIgnoreCase))
            {
                DateTime classStartTime = ParseTime(schedule.startTime);
                DateTime notificationTime = classStartTime.AddMinutes(-10);

                if (now >= notificationTime && now < classStartTime)
                {
                    if (!IsNotificationSent(schedule.subjectName))
                    {
                        SaveNotification(schedule.subjectName, schedule.room, schedule.startTime);
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
            if (notification.message.Contains(subject) &&
                notification.notificationWeek == currentWeek &&
                notification.isNotificationSent)
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

        foreach (var notif in userNotifications)
        {
            if (notif.message.Contains(subject) &&
                notif.message.Contains(startTime) &&
                notif.notificationWeek == currentWeek &&
                notif.isNotificationSent)
            {
                Debug.Log($"Notification already sent this week for: {subject} at {startTime}.");
                return;
            }
        }

        var newNotification = new Dictionary<string, object>
        {
            { "message", message },
            { "timestamp", timestamp },
            { "isRead", false },
            { "isNotificationSent", true },
            { "notificationWeek", currentWeek }
        };

        firestore.Collection("users").Document(userId).Collection("notifications").AddAsync(newNotification).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Notification saved to Firestore.");
                userNotifications.Add(new NotificationData(message, timestamp, false, true, currentWeek));
                inAppNotification.ShowSystemNotification(message);
            }
            else
            {
                Debug.LogError("Failed to save notification to Firestore.");
            }
        });
    }

    int GetCurrentWeekNumber(DateTime date)
    {
        var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;
        return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }

    DateTime ParseTime(string time)
    {
        return DateTime.ParseExact(time, "h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
    }
}
