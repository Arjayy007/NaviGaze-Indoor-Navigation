using userDataModel.Models;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEditor.MPE;
using System.Data;


public class StudentLoginController : MonoBehaviour
{
    public SceneManagerScript sceneManager;

    public InputField usernameInputField;
    public InputField passwordInputField;
    private DatabaseReference dbReference;
    public UserData userData;
    public UIErrorHandler errorHandler;

    public bool switchScene = false;
    
    void Start()
    {
         FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                string databaseUrl = "https://navigaze-448413-default-rtdb.asia-southeast1.firebasedatabase.app/";
                dbReference = FirebaseDatabase.GetInstance(app, databaseUrl).RootReference;
                Debug.Log("Firebase Initialized Successfully");
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
            }
        });
    }

    void Update()
    {
        if (switchScene)
        {
            switchScene = false;
            SceneManager.LoadScene("DashboardPage");
        }
    }

    public void OnLoginButtonClicked()
    {
        if (string.IsNullOrEmpty(usernameInputField.text.Trim()) || string.IsNullOrEmpty(passwordInputField.text))
        {
            errorHandler.ShowError("Please input email and password");
            return;
        }

        string email = usernameInputField.text.Trim();
        string password = HashPassword(passwordInputField.text);

        AuthenticateUser(email, password);
    }

     private void AuthenticateUser(string email, string hashedPassword)
    {
        if (dbReference == null)
        {
            Debug.LogError("Database reference is not initialized.");
            return;
        }

        dbReference.Child("users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DataSnapshot snapshot = task.Result;
                bool loginSuccess = false;
                string userId = null; // Variable to store the UserID

                foreach (DataSnapshot userSnapshot in snapshot.Children)
                {
                    var userJson = userSnapshot.GetRawJsonValue();
                    UserData user = JsonUtility.FromJson<UserData>(userJson);

                    if (user.email == email && user.password == hashedPassword)
                    {
                        loginSuccess = true;
                        userId = userSnapshot.Key; // Fetch the UserID (key)
                        Debug.Log($"Login Success! UserID: {userId}");

                        // Store the UserID for later use (e.g., using PlayerPrefs or a session manager)
                        PlayerPrefs.SetString("LoggedInUserID", userId);
                        PlayerPrefs.Save();
                        UserSession.UserId = userId;

                        switchScene = true;
                        break;
                    }
                }

                if (!loginSuccess)
                {
                    errorHandler.ShowError("Invalid Email or password");
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve user data: " + task.Exception);
            }
        });
    }

    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }


    public void OnRegisterButtonClicked()
    {
        string selectedRole = PlayerPrefs.GetString("SelectedRole", "");

        if (selectedRole == "Student")
        {
            SceneManager.LoadScene("RegistrationPage");
        }
        else if (selectedRole == "Professor")
        {
            SceneManager.LoadScene("ProfessorRegistration");
        }
    }


    
}
