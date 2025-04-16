using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Schedule : MonoBehaviour
{
    public Transform tableContainer;   // The parent container for rows
    public GameObject rowTemplate;     // Prefab for each row (Make sure it's assigned in Inspector)
    private FirebaseFirestore firestore;
    private string userId;
    
    async void Start()
    {
        rowTemplate.SetActive(false); // Disable after ensuring it's initially active
        userId = UserSession.UserId;  // Fetch the logged-in user ID

        // Await Firebase initialization
        var task = FirebaseApp.CheckAndFixDependenciesAsync();
        await task;

        if (task.Result == DependencyStatus.Available)
        {
            Debug.Log("Firebase initialized successfully.");
            FirebaseApp app = FirebaseApp.DefaultInstance;
            firestore = FirebaseFirestore.DefaultInstance;  // Initialize Firestore
            FetchSchedules(); // Call the method to fetch schedules asynchronously
        }
        else
        {
            Debug.LogError("Firebase not initialized: " + task.Result);
        }
    }

    public async void FetchSchedules()
    {
        if (firestore == null)
        {
            Debug.LogError("Firebase Firestore is not initialized yet!");
            return;
        }

        string schedulePath = $"users/{userId}/schedules";  // Path to user schedules
        Debug.Log("Fetching schedules from: " + schedulePath);

        try
        {
            // Query Firestore to get schedules
            QuerySnapshot snapshot = await firestore.Collection(schedulePath).GetSnapshotAsync();

            if (snapshot.Count == 0)
            {
                Debug.LogWarning("No schedules found in the database!");
                return;
            }

            Debug.Log("Successfully retrieved schedules. Count: " + snapshot.Count);

            // Clear existing rows in the table
            foreach (Transform child in tableContainer)
            {
                Destroy(child.gameObject);
            }

            // Loop through each schedule document and add to table
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                string subjectCode = document.GetValue<string>("subjectCode") ?? "N/A";
                string subjectName = document.GetValue<string>("subjectName") ?? "N/A";
                string room = document.GetValue<string>("room") ?? "N/A";
                string day = document.GetValue<string>("dayOfTheWeek") ?? "N/A";
                string startTime = document.GetValue<string>("startTime") ?? "N/A";
                string endTime = document.GetValue<string>("endTime") ?? "N/A";

                AddRowToTable(subjectCode, subjectName, room, day, startTime, endTime);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error fetching schedules: " + ex.Message);
        }
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
