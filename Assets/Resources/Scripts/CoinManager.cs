using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using System.Collections.Generic;

public class CoinManager : MonoBehaviour
{
    private FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public void AddCoinsToUser(int coins, string rewardName)
    {
        if (string.IsNullOrEmpty(UserSession.UserId))
        {
            Debug.LogError("User ID is null or empty.");
            return;
        }

        DocumentReference rewardRef = db.Collection("users").Document(UserSession.UserId)
            .Collection("information").Document("rewardsClaimed");

        rewardRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;
                bool rewardClaimed = snapshot.ContainsField(rewardName) && snapshot.GetValue<bool>(rewardName);

                if (!rewardClaimed)
                {
                    DocumentReference profileRef = db.Collection("users").Document(UserSession.UserId)
                        .Collection("information").Document("profile");

                    profileRef.GetSnapshotAsync().ContinueWithOnMainThread(coinTask =>
                    {
                        if (coinTask.IsCompletedSuccessfully)
                        {
                            int currentCoins = coinTask.Result.ContainsField("userCoins") ? coinTask.Result.GetValue<int>("userCoins") : 0;
                            int newCoinBalance = currentCoins + coins;

                            profileRef.UpdateAsync("userCoins", newCoinBalance);
                            rewardRef.UpdateAsync(rewardName, true);

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
                // Initialize rewardClaimed if not existing
                rewardRef.SetAsync(new Dictionary<string, object> { { rewardName, false } }, SetOptions.MergeAll);
            }
        });
    }

    public void AddCoinsDirectly(int coins)
    {
        if (string.IsNullOrEmpty(UserSession.UserId))
        {
            Debug.LogError("User ID is null or empty.");
            return;
        }

        DocumentReference profileRef = db.Collection("users").Document(UserSession.UserId)
            .Collection("information").Document("profile");

        profileRef.GetSnapshotAsync().ContinueWithOnMainThread(coinTask =>
        {
            if (coinTask.IsCompletedSuccessfully)
            {
                int currentCoins = coinTask.Result.ContainsField("userCoins") ? coinTask.Result.GetValue<int>("userCoins") : 0;
                int newCoinBalance = currentCoins + coins;

                profileRef.UpdateAsync("userCoins", newCoinBalance).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompletedSuccessfully)
                    {
                        Debug.Log($"Added {coins} coins. New balance: {newCoinBalance}");
                    }
                    else
                    {
                        Debug.LogError("Failed to update coins: " + updateTask.Exception);
                    }
                });
            }
            else
            {
                Debug.LogError("Failed to retrieve user coins: " + coinTask.Exception);
            }
        });
    }

    public void AddExperienceDirectly(int experience)
    {
        if (string.IsNullOrEmpty(UserSession.UserId))
        {
            Debug.LogError("User ID is null or empty.");
            return;
        }

        DocumentReference profileRef = db.Collection("users").Document(UserSession.UserId)
            .Collection("information").Document("profile");

        profileRef.GetSnapshotAsync().ContinueWithOnMainThread(expTask =>
        {
            if (expTask.IsCompletedSuccessfully)
            {
                int currentExp = expTask.Result.ContainsField("exp") ? expTask.Result.GetValue<int>("exp") : 0;
                int newExpBalance = currentExp + experience;

                profileRef.UpdateAsync("exp", newExpBalance).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompletedSuccessfully)
                    {
                        Debug.Log($"Added {experience} experience points. New EXP balance: {newExpBalance}");
                    }
                    else
                    {
                        Debug.LogError("Failed to update experience: " + updateTask.Exception);
                    }
                });
            }
            else
            {
                Debug.LogError("Failed to retrieve user experience: " + expTask.Exception);
            }
        });
    }
}
