using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    private DatabaseReference dbReference;

    void Start()
    {
        string databaseUrl = "https://navigaze-448413-default-rtdb.asia-southeast1.firebasedatabase.app/";
        dbReference = FirebaseDatabase.GetInstance(databaseUrl).RootReference;
    }

    public void AddCoinsToUser(int coins, string rewardName)
    {
        if (string.IsNullOrEmpty(UserSession.UserId))
        {
            Debug.LogError("User ID is null or empty.");
            return;
        }

        DatabaseReference userRef = dbReference.Child("users").Child(UserSession.UserId);

        userRef.Child("badgesCollected").Child(rewardName).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                bool rewardClaimed = task.Result.Exists && bool.Parse(task.Result.Value.ToString());

                if (!rewardClaimed)
                {
                    // Get current coins balance
                    userRef.Child("userCoins").GetValueAsync().ContinueWithOnMainThread(coinTask =>
                    {
                        if (coinTask.IsCompletedSuccessfully)
                        {
                            int currentCoins = coinTask.Result.Exists ? int.Parse(coinTask.Result.Value.ToString()) : 0;
                            int newCoinBalance = currentCoins + coins;

                            // Update the user's coin balance and set the reward as claimed
                            userRef.Child("userCoins").SetValueAsync(newCoinBalance);
                            userRef.Child("rewardsClaimed").Child(rewardName).SetValueAsync(true); // Mark the reward as claimed
                            Debug.Log($"Reward '{rewardName}' claimed. New coin balance: {newCoinBalance}");
                        }
                    });
                }
                else
                {
                    Debug.Log($"Reward '{rewardName}' has already been claimed.");
                }
            }
            else
            {
                // Initialize rewardClaimed if it does not exist
                userRef.Child("rewardsClaimed").Child(rewardName).SetValueAsync(false);
            }
        });
    }
}
