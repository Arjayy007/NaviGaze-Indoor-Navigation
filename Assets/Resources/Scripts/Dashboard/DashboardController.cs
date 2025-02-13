using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class DashboardController : MonoBehaviour
{
    public Text UsernameText;   
    public Text CoinsText;      
    private DatabaseReference dbReference;
    private string userId;

    private void Start()
    {
       userId = UserSession.UserId;

    StartCoroutine(InitializeFirebase());
}

IEnumerator InitializeFirebase()
{
    var dependencyTask = FirebaseApp.CheckDependenciesAsync();
    yield return new WaitUntil(() => dependencyTask.IsCompleted);

    if (dependencyTask.Result == DependencyStatus.Available)
    {
        Debug.Log("Firebase initialized successfully.");
        FirebaseApp app = FirebaseApp.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        LoadUserData();
    }
    else
    {
        Debug.LogError("Firebase not initialized: " + dependencyTask.Result);
    }
}

    // Function to load user data from Firebase
    public void LoadUserData()
    {
        string userPath = $"users/{UserSession.UserId}"; 

        dbReference.Child(userPath).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    string userName = snapshot.Child("firstName").Value.ToString();
                    string userCoins = snapshot.Child("userCoins").Value.ToString();


                    UsernameText.text = userName;  
                    CoinsText.text = userCoins + "Coins";

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

    public void LoadNotificationScene()
    {
        SceneManager.LoadScene("NotificationPage");
    }

}
