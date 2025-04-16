using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using System.Collections.Generic;

public class CollectionVisibility : MonoBehaviour
{
    public GameObject collectionPanel;
    public GameObject badgesPanel;
    public GameObject missionsPanel;
    public GameObject userAccessoriesPanel;

public TMP_Text Coins;
public TMP_Text Exp;
public TMP_Text Fullname;
public TMP_Text ProgramSection;


    private FirebaseFirestore firestore;
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
                firestore = FirebaseFirestore.DefaultInstance;
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
    DocumentReference docRef = firestore.Collection("users")
                                         .Document(userId)
                                         .Collection("information")
                                         .Document("profile");

    docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted && task.Result.Exists)
        {
            DocumentSnapshot snapshot = task.Result;

            // Use correct types based on Firestore field type
            long userCoins = snapshot.ContainsField("userCoins") ? snapshot.GetValue<long>("userCoins") : 0;
            long userExp = snapshot.ContainsField("exp") ? snapshot.GetValue<long>("exp") : 0;

            string firstName = snapshot.ContainsField("firstName") ? snapshot.GetValue<string>("firstName") : "N/A";
            string lastName = snapshot.ContainsField("lastName") ? snapshot.GetValue<string>("lastName") : "N/A";
            string program = snapshot.ContainsField("program") ? snapshot.GetValue<string>("program") : "N/A";
            string yearAndSection = snapshot.ContainsField("yearSection") ? snapshot.GetValue<string>("yearSection") : "N/A";

            ProgramSection.text = $"{program} {yearAndSection}";
            Fullname.text = $"{firstName} {lastName}";
           Coins.text = userCoins.ToString() + " Coins";
           Exp.text = userExp.ToString() + " XP";


            Debug.Log("Document data: " + snapshot.ToDictionary());
            Debug.Log("Has userCoins: " + snapshot.ContainsField("userCoins"));
            Debug.Log("Has exp: " + snapshot.ContainsField("exp"));
            Debug.Log("userCoins value: " + userCoins);
            Debug.Log("userExp value: " + userExp);
            Debug.Log("Fullname: " + Fullname.text);
            Debug.Log("Program and Section: " + ProgramSection.text);


        }
        else
        {
            Debug.LogError("User profile document not found or error occurred.");
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
