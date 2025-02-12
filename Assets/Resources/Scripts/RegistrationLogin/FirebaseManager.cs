using System.Threading.Tasks;
using Google;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Extensions;

public class GoogleSignInManager : MonoBehaviour
{
    public string GoogleAPI = "754613247534-jbgduudurt59h0aorch62991brp0jg6p.apps.googleusercontent.com";

    private GoogleSignInConfiguration configuration;
    private DatabaseReference databaseRef;
    public Button GoogleSignInButton;

    private void Awake()
    {
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = GoogleAPI,
            RequestIdToken = false,
            RequestEmail = true
        };

        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;

        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;

        if (GoogleSignInButton != null)
        {
            GoogleSignInButton.interactable = true;
        }
    }

    public void GoogleSignInClick()
    {
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthenticatedFinished);
    }

    private void OnGoogleAuthenticatedFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            Debug.LogError("Google Sign-In Error: " + task.Exception);
            return;
        }

        if (task.IsCanceled)
        {
            Debug.LogError("Google Sign-In was canceled.");
            return;
        }

        GoogleSignInUser googleUser = task.Result;
        Debug.Log($"Google Sign-In Success! Name: {googleUser.DisplayName}, Email: {googleUser.Email}");

        // Splitting first and last name
        string firstName = "";
        string lastName = "";
        string[] nameParts = googleUser.DisplayName.Split(' ');

        if (nameParts.Length > 1)
        {
            firstName = nameParts[0];
            lastName = nameParts[nameParts.Length - 1];
        }
        else
        {
            firstName = googleUser.DisplayName;
        }

        Debug.Log($"First Name: {firstName}");
        Debug.Log($"Last Name: {lastName}");

        // Check if the user already exists in Firebase
        CheckUserExistsInDatabase(googleUser.Email, firstName, lastName);
    }

    private void CheckUserExistsInDatabase(string email, string firstName, string lastName)
    {
        databaseRef.Child("users").OrderByChild("email").EqualTo(email).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error checking user in database: " + task.Exception);
                return;
            }

            if (task.IsCompletedSuccessfully)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    foreach (var child in snapshot.Children)
                    {
                        string userId = child.Key; // Get the unique user ID
                        Debug.Log($"User exists with ID: {userId}");

                        // Store user ID in session
                        UserSession.UserId = userId;

                        // Load main scene for existing users
                        LoadScene("DashboardPage"); 
                        return;
                    }
                }
                else
                {
                    Debug.Log("User not found in the database. Saving details for registration.");

                    // Save user details in PlayerPrefs
                    PlayerPrefs.SetString("userEmail", email);
                    PlayerPrefs.SetString("userFirstName", firstName);
                    PlayerPrefs.SetString("userLastName", lastName);
                    PlayerPrefs.SetInt("isGoogleSignUp", 1); 
                    PlayerPrefs.Save();

                    // Load registration scene
                    LoadScene("RegistrationPage");
                }
            }
        });
    }

    private void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void GoogleSignOut()
    {
        GoogleSignIn.DefaultInstance.SignOut();
        Debug.Log("Google Sign-Out Successful.");
    }
}
