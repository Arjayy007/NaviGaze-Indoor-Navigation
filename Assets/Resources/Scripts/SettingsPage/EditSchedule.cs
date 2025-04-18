using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
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

                FetchSchedules();
            }
            else
            {
                Debug.LogError("Failed to initialize Firebase: " + task.Result);
            }
        });
    }

    public void FetchSchedules()
    {
        if (firestore == null)
        {
            Debug.LogError("Firestore is not initialized yet!");
            return;
        }

        firestore.Collection("users").Document(userId).Collection("schedules")
            .GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error fetching schedules: " + task.Exception);
                return;
            }

            QuerySnapshot snapshot = task.Result;

            if (snapshot.Count == 0)
            {
                Debug.LogWarning("No schedules found in Firestore!");
                return;
            }

            // Clear existing rows before adding new ones
            foreach (Transform child in tableContainer)
            {
                if (child.gameObject != rowTemplate)
                {
                    Destroy(child.gameObject);
                }
            }

            // Add new rows to the table
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                string scheduleID = doc.Id;
                Debug.Log("Processing Schedule ID: " + scheduleID);

                Dictionary<string, object> data = doc.ToDictionary();

                string subjectCode = data.ContainsKey("subjectCode") ? data["subjectCode"].ToString() : "N/A";
                string subjectName = data.ContainsKey("subjectName") ? data["subjectName"].ToString() : "N/A";
                string room = data.ContainsKey("room") ? data["room"].ToString() : "N/A";
                string day = data.ContainsKey("dayOfTheWeek") ? data["dayOfTheWeek"].ToString() : "N/A";
                string startTime = data.ContainsKey("startTime") ? data["startTime"].ToString() : "N/A";
                string endTime = data.ContainsKey("endTime") ? data["endTime"].ToString() : "N/A";

                AddRowToTable(subjectCode, subjectName, room, day, startTime, endTime, scheduleID);
            }
        });
    }

    private void AddRowToTable(string subjectCode, string subjectName, string room, string day, string startTime, string endTime, string scheduleID)
    {
        GameObject newRow = Instantiate(rowTemplate, tableContainer);
        newRow.SetActive(true);

        // Create a new Button component dynamically for the row
        Button rowButton = newRow.AddComponent<Button>();
        rowButton.interactable = true;
        rowButton.onClick.RemoveAllListeners();
        rowButton.onClick.AddListener(() =>
        {
            // Directly populate the fields with row data
            PopulateEditFields(subjectCode, subjectName, room, day, startTime, endTime, scheduleID);
        });

        // Enable the child elements
        foreach (Transform child in newRow.transform)
        {
            child.gameObject.SetActive(true);
        }

        // Populate the Text components
        Text[] rowColumns = newRow.GetComponentsInChildren<Text>(true);
        if (rowColumns.Length >= 6)
        {
            rowColumns[0].text = subjectCode;
            rowColumns[1].text = subjectName;
            rowColumns[2].text = room;
            rowColumns[3].text = day;
            rowColumns[4].text = startTime;
            rowColumns[5].text = endTime;
        }
        else
        {
            Debug.LogError("Row template does not have enough columns!");
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(tableContainer.GetComponent<RectTransform>());
    }

    public void PopulateEditFields(string subjectCode, string subjectName, string room, string day, string startTime, string endTime, string scheduleID)
    {
        subjectCodeInput.text = subjectCode;
        subjectNameInput.text = subjectName;
        roomInput.text = room;

        SetDropdownValue(dayDropdown, day);
        SetDropdownValue(startTimeDropdown, startTime);
        SetDropdownValue(endTimeDropdown, endTime);

        currentScheduleID = scheduleID;
    }

    private void SetDropdownValue(Dropdown dropdown, string value)
    {
        int index = dropdown.options.FindIndex(option => option.text == value);
        if (index != -1)
        {
            dropdown.value = index;
        }
    }

    public void SaveEditedSchedule()
    {
        if (string.IsNullOrEmpty(currentScheduleID))
        {
            Debug.LogError("No schedule selected for editing.");
            return;
        }

        var updatedSchedule = new Dictionary<string, object>
        {
            ["subjectCode"] = subjectCodeInput.text,
            ["subjectName"] = subjectNameInput.text,
            ["room"] = roomInput.text,
            ["dayOfTheWeek"] = dayDropdown.options[dayDropdown.value].text,
            ["startTime"] = startTimeDropdown.options[startTimeDropdown.value].text,
            ["endTime"] = endTimeDropdown.options[endTimeDropdown.value].text
        };

        firestore.Collection("users").Document(userId).Collection("schedules").Document(currentScheduleID)
            .SetAsync(updatedSchedule).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Schedule updated successfully!");
                SceneManager.LoadScene("EditSchedulePage");
            }
            else
            {
                Debug.LogError("Error updating schedule: " + task.Exception);
            }
        });
    }
}
