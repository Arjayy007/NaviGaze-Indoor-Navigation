using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class Schedule : MonoBehaviour
{
    public Transform tableContainer;
    public GameObject rowTemplate;
    private FirebaseFirestore firestore;
    private string userId;

    public EditSchedule editSchedule;

    private void Start()
    {
        userId = UserSession.UserId;
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                firestore = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firestore initialized.");
                FetchSchedules();
            }
            else
            {
                Debug.LogError("Could not initialize Firebase: " + task.Result);
            }
        });
    }

    public void FetchSchedules()
    {
        if (firestore == null)
        {
            Debug.LogError("Firebase Firestore is not initialized yet!");
            return;
        }

        string schedulePath = $"users/{userId}/schedules";
        Debug.Log("Fetching schedules from: " + schedulePath);

        firestore.Collection(schedulePath).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error fetching schedules: " + task.Exception);
                return;
            }

            QuerySnapshot snapshot = task.Result;

            if (snapshot.Count == 0)
            {
                Debug.LogWarning("No schedules found in the database!");
                return;
            }

            Debug.Log("Successfully retrieved schedules. Count: " + snapshot.Count);

            foreach (Transform child in tableContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                string scheduleID = document.Id; // ✅ Extract document ID here
                string subjectCode = document.GetValue<string>("subjectCode") ?? "N/A";
                string subjectName = document.GetValue<string>("subjectName") ?? "N/A";
                string room = document.GetValue<string>("room") ?? "N/A";
                string day = document.GetValue<string>("dayOfTheWeek") ?? "N/A";
                string startTime = document.GetValue<string>("startTime") ?? "N/A";
                string endTime = document.GetValue<string>("endTime") ?? "N/A";

                AddRowToTable(scheduleID, subjectCode, subjectName, room, day, startTime, endTime);
            }
        });
    }

    private void AddRowToTable(string scheduleID, string subjectCode, string subjectName, string room, string day, string startTime, string endTime)
    {
        GameObject newRow = Instantiate(rowTemplate, tableContainer);
        newRow.SetActive(true);

        foreach (Transform child in newRow.transform)
        {
            child.gameObject.SetActive(true);
            HorizontalLayoutGroup childLayoutGroup = child.GetComponent<HorizontalLayoutGroup>();
            if (childLayoutGroup != null)
                childLayoutGroup.enabled = true;
        }

        HorizontalLayoutGroup rowLayoutGroup = newRow.GetComponent<HorizontalLayoutGroup>();
        if (rowLayoutGroup != null)
            rowLayoutGroup.enabled = true;

        Text[] rowColumns = newRow.GetComponentsInChildren<Text>(true);

        if (rowColumns.Length >= 6)
        {
            for (int i = 0; i < rowColumns.Length; i++)
            {
                rowColumns[i].gameObject.SetActive(true);
                rowColumns[i].enabled = true;
            }

            rowColumns[0].text = subjectCode;
            rowColumns[1].text = subjectName;
            rowColumns[2].text = room;
            rowColumns[3].text = day;
            rowColumns[4].text = startTime;
            rowColumns[5].text = endTime;

            Button rowButton = newRow.GetComponentInChildren<Button>();
            if (rowButton != null)
            {
                rowButton.enabled = true;
                rowButton.interactable = true;
            }

            if (editSchedule != null)
            {
                // ✅ Pass scheduleID to the click handler for editing
                editSchedule.AddClickListenerToRow(newRow, scheduleID, subjectCode, subjectName, room, day, startTime, endTime);
            }
        }
        else
        {
            Debug.LogError("Row template does not have enough columns!");
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(tableContainer.GetComponent<RectTransform>());
    }
}
