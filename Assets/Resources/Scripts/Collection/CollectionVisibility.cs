using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class CollectionVisibility : MonoBehaviour
{
    public GameObject collectionPanel;
    public GameObject badgesPanel;
    public GameObject missionsPanel;
    public GameObject userAccessoriesPanel;
    public Text Coins;
    public Text Exp;
    public Text Fullname;
    public Text ProgramSection;
    private DatabaseReference dbReference;
    private string userId;

    private void Start()
    {
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
                FirebaseApp app = FirebaseApp.DefaultInstance;
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;

                LoadUserData();
            }
            else
            {
                Debug.LogError("Failed to initialize Firebase: " + task.Result);
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
                    string userCoins = snapshot.Child("userCoins").Value.ToString();
                    string userExp = snapshot.Child("exp").Value.ToString();
                    string firstName = snapshot.Child("firstName").Value.ToString();
                    string lastName = snapshot.Child("lastName").Value.ToString();
                    string program = snapshot.Child("program").Value.ToString();
                    string yearAndSection = snapshot.Child("yearSection").Value.ToString();

                    ProgramSection.text = program + " " + yearAndSection;
                    Fullname.text = firstName + " " + lastName;
                    Coins.text = userCoins;
                    Exp.text = userExp;
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

    public void ShowCollection() 
    {
        collectionPanel.SetActive(true);
        badgesPanel.SetActive(false);
        missionsPanel.SetActive(false);
    }

    public void ShowBadges() 
    {
        badgesPanel.SetActive(true);
        missionsPanel.SetActive(false);
        collectionPanel.SetActive(false);
    }

    public void ShowMissions() 
    {
        missionsPanel.SetActive(true);
        collectionPanel.SetActive(false);
        badgesPanel.SetActive(false);
    }

    public void ShowUserAccessories() 
    {
        userAccessoriesPanel.SetActive(true);
    }


}
