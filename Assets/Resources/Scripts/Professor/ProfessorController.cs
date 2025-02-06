using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Extensions;
using Firebase;

public class ProfessorController : MonoBehaviour
{
    public Text Fullname;
    public Text Department;

    private DatabaseReference dbReference;
    private string userId;

    void Start()
    {
        userId = UserSession.UserId;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase initialized successfully.");
                FirebaseApp app = FirebaseApp.DefaultInstance;
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;

                LoadUserData();
            }
            else
            {
                Debug.LogError("Firebase not initialized: " + task.Result);
            }
        });
    }

    public void LoadUserData()
    {
        string userPath = $"users/{userId}";

        dbReference.Child(userPath).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    string firstName = snapshot.Child("firstName").Value.ToString();
                    string lastName = snapshot.Child("lastName").Value.ToString();
                    string department = snapshot.Child("department").Value.ToString();

                    string fullName = firstName + " " + lastName;

                    Department.text = department;
                    Fullname.text = fullName;
                }
                else
                {
                    Debug.LogError("User data not found!");
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve user data.");
            }
        });
    }
}