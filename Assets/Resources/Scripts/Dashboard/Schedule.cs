using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine.UI;
using Firebase;

public class Schedule : MonoBehaviour
{
    public Transform tableContainer;   // The parent container for rows
    public GameObject rowTemplate;     // Prefab for each row (Make sure it's assigned in Inspector)
    private DatabaseReference dbReference;
    private string userId;
    
    void Start()
    {
        rowTemplate.SetActive(false); // Disable after ensuring it's initially active
        userId = UserSession.UserId;  // Fetch the logged-in user ID
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase initialized successfully.");
                FirebaseApp app = FirebaseApp.DefaultInstance;
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                FetchSchedules();

            }
            else
            {
                Debug.LogError("Firebase not initialized: " + task.Result);
            }
        }); 
    }

    public void FetchSchedules()
    {
        if (dbReference == null)
        {
            Debug.LogError("Firebase is not initialized yet!");
            return;
        }

        string schedulePath = $"users/{userId}/schedules";  // Path to user schedules
        Debug.Log("Fetching schedules from: " + schedulePath);

        dbReference.Child(schedulePath).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error fetching schedules: " + task.Exception);
                return;
            }

            if (!task.Result.Exists)
            {
                Debug.LogWarning("No schedules found in the database!");
                return;
            }

            DataSnapshot snapshot = task.Result;
            Debug.Log("Successfully retrieved schedules. Count: " + snapshot.ChildrenCount);

            foreach (Transform child in tableContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var childSnapshot in snapshot.Children)
            {
                string scheduleID = childSnapshot.Key; 
                Debug.Log("Processing Schedule ID: " + scheduleID);

                string subjectCode = childSnapshot.Child("subjectCode").Value?.ToString() ?? "N/A";
                string subjectName = childSnapshot.Child("subjectName").Value?.ToString() ?? "N/A";
                string room = childSnapshot.Child("room").Value?.ToString() ?? "N/A";
                string day = childSnapshot.Child("dayOfTheWeek").Value?.ToString() ?? "N/A";
                string startTime = childSnapshot.Child("startTime").Value?.ToString() ?? "N/A";
                string endTime = childSnapshot.Child("endTime").Value?.ToString() ?? "N/A";

                AddRowToTable(subjectCode, subjectName, room, day, startTime, endTime);
            }
        });
    }

    private void AddRowToTable(string subjectCode, string subjectName, string room, string day, string startTime, string endTime)
    {
        GameObject newRow = Instantiate(rowTemplate, tableContainer);
        newRow.SetActive(true);

        // Enable all child objects inside the new row
        foreach (Transform child in newRow.transform)
        {
            child.gameObject.SetActive(true);

            // Enable Horizontal Layout Group if it exists on children
            HorizontalLayoutGroup childLayoutGroup = child.GetComponent<HorizontalLayoutGroup>();
            if (childLayoutGroup != null)
            {
                childLayoutGroup.enabled = true;
            }
        }

        // Enable Horizontal Layout Group on the row itself
        HorizontalLayoutGroup rowLayoutGroup = newRow.GetComponent<HorizontalLayoutGroup>();
        if (rowLayoutGroup != null)
        {
            rowLayoutGroup.enabled = true;
        }

        Text[] rowColumns = newRow.GetComponentsInChildren<Text>(true);

        if (rowColumns.Length >= 6)
        {
            for (int i = 0; i < rowColumns.Length; i++)
            {
                rowColumns[i].gameObject.SetActive(true); // Ensure text objects are enabled
                rowColumns[i].enabled = true; 
            }

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

        // Force UI update
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(tableContainer.GetComponent<RectTransform>());
    }





}
