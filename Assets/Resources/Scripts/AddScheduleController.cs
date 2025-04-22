using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using SchedulesModel.Models;

public class AddScheduleController : MonoBehaviour
{

    public RectTransform panel; // The UI panel to move
    private float originalY;
    private bool keyboardVisible = false;

    public SceneManagerScript sceneManager;
    private FirebaseFirestore firestore;

    public InputField subjectCode;
    public InputField subjectName;
    public InputField room;
    public Dropdown dayOfTheWeek;
    public Dropdown startTime;
    public Dropdown endTime;
    public Dropdown campus;

    public GameObject rowTemplate;
    public Transform tableContainer;
    public ScheduleData schedulesModel;
    private bool switchScene = false;

    private List<ScheduleData> scheduleList = new List<ScheduleData>();
    string userId = UserSession.UserId;

    void Start()
    {

        originalY = panel.anchoredPosition.y;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                firestore = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firebase Firestore Initialized Successfully");

                Debug.Log("User ID: " + userId);
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
            }
        });

        subjectCode.onValueChanged.AddListener(OnClassCodeChanged);
    }

    void Update()
    {

       if (TouchScreenKeyboard.visible)
        {
            if (!keyboardVisible)
            {
                keyboardVisible = true;
                panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, originalY + 300); // Move up
            }
        }
        else
        {
            if (keyboardVisible)
            {
                keyboardVisible = false;
                panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, originalY); // Move back
            }
        }



        if (switchScene)
        {
            switchScene = false;
            string selectedRole = PlayerPrefs.GetString("SelectedRole", "");

            if (selectedRole == "Student")
            {
                SceneManager.LoadScene("DashboardPage");
            }
            else if (selectedRole == "Professor")
            {
                SceneManager.LoadScene("ProfessorDashboard");
            }
        }
    }

    public void OnAddScheduleButtonClicked()
    {
        if (string.IsNullOrEmpty(subjectCode.text) ||
            string.IsNullOrEmpty(subjectName.text) ||
            string.IsNullOrEmpty(room.text))
        {
            Debug.LogWarning("Please fill in all input fields!");
            return;
        }

        GameObject newRow = Instantiate(rowTemplate, tableContainer);
        newRow.SetActive(true);

        TextMeshProUGUI[] rowColumns = newRow.GetComponentsInChildren<TextMeshProUGUI>();

        if (rowColumns.Length >= 6)
        {
            rowColumns[0].text = subjectCode.text;                                 // Subject Code
            rowColumns[1].text = subjectName.text;                                // Subject Name
            rowColumns[2].text = room.text;                                       // Room
            rowColumns[3].text = dayOfTheWeek.options[dayOfTheWeek.value].text;   // Day of the Week
            rowColumns[4].text = startTime.options[startTime.value].text;         // Start Time
            rowColumns[5].text = endTime.options[endTime.value].text;             // End Time
        }
        else
        {
            Debug.LogError("Row template does not have enough columns to populate data.");
        }

        ScheduleData schedule = new ScheduleData(
            subjectCode.text,
            subjectName.text,
            room.text,
            dayOfTheWeek.options[dayOfTheWeek.value].text,
            startTime.options[startTime.value].text,
            endTime.options[endTime.value].text,
            campus.options[campus.value].text
        );

        scheduleList.Add(schedule);
        ClearInputFields();
    }

    public void OnSaveButtonClicked()
    {
        if (scheduleList.Count == 0)
        {
            Debug.LogWarning("No schedule data to save!");
            return;
        }

        foreach (var schedule in scheduleList)
        {
            SaveToDatabase(schedule);
        }
        scheduleList.Clear();
        ClearTable();
    }

    private void SaveToDatabase(ScheduleData schedule)
    {
        if (firestore == null)
        {
            Debug.LogError("Firestore is not initialized.");
            return;
        }

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("No user is logged in. Cannot associate schedule.");
            return;
        }

        DocumentReference userDocRef = firestore.Collection("users").Document(userId);
        CollectionReference schedulesCollection = userDocRef.Collection("schedules");

        Dictionary<string, object> scheduleData = new Dictionary<string, object>
        {
            { "subjectCode", schedule.subjectCode },
            { "subjectName", schedule.subjectName },
            { "room", schedule.room },
            { "dayOfTheWeek", schedule.dayOfTheWeek },
            { "startTime", schedule.startTime },
            { "endTime", schedule.endTime},
            { "campus", schedule.campus}
        };

    
        schedulesCollection.AddAsync(scheduleData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Schedule data saved to Firestore under user: " + userId);
                switchScene = true;
            }
            else
            {
                Debug.LogError("Failed to save schedule data: " + task.Exception);
            }
        });
    }

    private void ClearInputFields()
    {
        subjectCode.text = "";
        subjectName.text = "";
        room.text = "";
        dayOfTheWeek.value = 0;
        startTime.value = 0;
        endTime.value = 0;
        campus.value = 0;
    }

    public void SkipButtonClicked()
    {
        string selectedRole = PlayerPrefs.GetString("SelectedRole", "");

        if (selectedRole == "Student")
        {
            SceneManager.LoadScene("DashboardPage");
        }
        else if (selectedRole == "Professor")
        {
            SceneManager.LoadScene("ProfessorDashboard");
        }
    }

    private void ClearTable()
    {
        foreach (Transform child in tableContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnClassCodeChanged(string input)
    {
        subjectName.text = ClassCodeDictionary.GetSubjectName(input);
    }
}
