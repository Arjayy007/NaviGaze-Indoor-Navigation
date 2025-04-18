using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase;
using Firebase.Extensions;
using System.Collections.Generic;
using Firebase.Firestore;

public class EditSchedule : MonoBehaviour
{
    public InputField subjectCodeInput;
    public InputField subjectNameInput;
    public InputField roomInput;
    public Dropdown dayDropdown;
    public Dropdown startTimeDropdown;
    public Dropdown endTimeDropdown;

    public Transform tableContainer;
    public GameObject rowTemplate;

    private FirebaseFirestore firestore;
    private string userId;
    private string currentScheduleID;

    void Start()
    {
        rowTemplate.SetActive(false);
        userId = UserSession.UserId;
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase initialized successfully.");
                firestore = FirebaseFirestore.DefaultInstance;
            }
            else
            {
                Debug.LogError("Failed to initialize Firebase: " + task.Result);
            }
        });
    }

    public void AddClickListenerToRow(GameObject row, string scheduleID, string subjectCode, string subjectName, string room, string day, string startTime, string endTime)
    {
        Button button = row.GetComponent<Button>();
        if (button == null)
        {
            button = row.AddComponent<Button>();
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            Debug.Log("Clicked schedule row with ID: " + scheduleID); 
            PopulateFields(scheduleID, subjectCode, subjectName, room, day, startTime, endTime);
        });
    }

    private void PopulateFields(string scheduleID, string subjectCode, string subjectName, string room, string day, string startTime, string endTime)
    {
        currentScheduleID = scheduleID; // Store selected document ID
        subjectCodeInput.text = subjectCode;
        subjectNameInput.text = subjectName;
        roomInput.text = room;
        dayDropdown.value = dayDropdown.options.FindIndex(option => option.text == day);
        startTimeDropdown.value = startTimeDropdown.options.FindIndex(option => option.text == startTime);
        endTimeDropdown.value = endTimeDropdown.options.FindIndex(option => option.text == endTime);
    }

    // 🔄 CALL THIS from Update Button's OnClick event
    public void UpdateScheduleInFirestore()
    {
        if (string.IsNullOrEmpty(currentScheduleID))
        {
            Debug.LogError("No schedule selected for update.");
            return;
        }

        string subjectCode = subjectCodeInput.text.Trim();
        string subjectName = subjectNameInput.text.Trim();
        string room = roomInput.text.Trim();
        string day = dayDropdown.options[dayDropdown.value].text;
        string startTime = startTimeDropdown.options[startTimeDropdown.value].text;
        string endTime = endTimeDropdown.options[endTimeDropdown.value].text;

        DocumentReference docRef = firestore.Collection("users").Document(userId).Collection("schedules").Document(currentScheduleID);

        Dictionary<string, object> updatedData = new Dictionary<string, object>
        {
            { "subjectCode", subjectCode },
            { "subjectName", subjectName },
            { "room", room },
            { "dayOfTheWeek", day },
            { "startTime", startTime },
            { "endTime", endTime }
        };

        docRef.UpdateAsync(updatedData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Schedule updated successfully!");
            }
            else
            {
                Debug.LogError("Failed to update schedule: " + task.Exception);
            }
        });
    }
}
