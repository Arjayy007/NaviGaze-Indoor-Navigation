using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;

public class ProfessorController : MonoBehaviour
{
    public Text Fullname;
    public Text Department;

    private FirebaseFirestore firestore;
    private string userId;

    void Start()
    {
        userId = UserSession.UserId;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase initialized successfully.");
                firestore = FirebaseFirestore.DefaultInstance;

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
        DocumentReference profileRef = firestore
            .Collection("users")
            .Document(userId)
            .Collection("information")
            .Document("profile");

        profileRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                DocumentSnapshot snapshot = task.Result;

                string firstName = snapshot.ContainsField("firstName") ? snapshot.GetValue<string>("firstName") : "";
                string lastName = snapshot.ContainsField("lastName") ? snapshot.GetValue<string>("lastName") : "";
                string department = snapshot.ContainsField("department") ? snapshot.GetValue<string>("department") : "";

                string fullName = firstName + " " + lastName;

                Department.text = department;
                Fullname.text = fullName;
            }
            else
            {
                Debug.LogError("User profile not found or failed to retrieve.");
            }
        });
    }

    public void LoadProfessorDashboard()
    {
        SceneManager.LoadScene("ProfessorDashboard");
    }
}
