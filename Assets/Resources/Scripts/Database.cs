using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;

public class Database : MonoBehaviour
{
    private DatabaseReference reference;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        
    }

    // Update is called once per frame
   public void AddToDatabase (){
    Dictionary<string, object> badgeData = new Dictionary<string, object>();
        badgeData.Add("Bronze", new Dictionary<string, object>
        {
            { "title", "Kasipagan Badge" },
            { "message", "Attend at least {X} consecutive classes in a week." }
        });
        badgeData.Add("Silver", new Dictionary<string, object>
        {
            { "title", "Kasipagan Badge" },
            { "message", "Attend at least {Y} consecutive classes in a week." }
        });
        badgeData.Add("Gold", new Dictionary<string, object>
        {
            { "title", "Kasipagan Badge" },
            { "message", "Attend all your classes this week with no absences!" }
        });

        reference.Child("badges").SetValueAsync(badgeData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Badge data has been added to the database.");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("Error adding badge data to the database: " + task.Exception);
            }
        });
    }
}
