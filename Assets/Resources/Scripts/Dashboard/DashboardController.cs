using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DashboardController : MonoBehaviour
{
    public Text UsernameText;
    public Text CoinsText;

    private FirebaseFirestore firestore;
    private string userId;
    public GameObject noInternetPanel;


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
                LoadUserData();
            }
            else
            {
                Debug.LogError("Could not initialize Firebase: " + task.Result);
            }
        });
    }

    public void LoadUserData()
    {
        if (firestore == null || string.IsNullOrEmpty(userId))
        {
            Debug.LogError("Firestore or User ID is not initialized.");
            return;
        }

        DocumentReference profileRef = firestore
            .Collection("users")
            .Document(userId)
            .Collection("information")
            .Document("profile");

        profileRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    Dictionary<string, object> userData = snapshot.ToDictionary();

                    string firstName = userData.ContainsKey("firstName") ? userData["firstName"].ToString() : "User";
                    string coins = userData.ContainsKey("userCoins") ? userData["userCoins"].ToString() : "0"; // Optional

                    UsernameText.text = firstName;
                    CoinsText.text = coins + " Coins";
                }
                else
                {
                    Debug.LogWarning("Profile document does not exist.");
                }
            }
            else
            {
                Debug.LogError("Failed to fetch profile: " + task.Exception);
            }
        });
    }

    public void LoadNotificationScene()
    {
        SceneManager.LoadScene("NotificationPage");
    }
}
