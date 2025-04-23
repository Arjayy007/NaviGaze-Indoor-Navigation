using userDataModel.Models;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;
using System.Data;

public class StudentLoginController : MonoBehaviour
{

    public RectTransform panel; // The UI panel to move
    private float originalY;
    private bool keyboardVisible = false;

    public SceneManagerScript sceneManager;

    public InputField usernameInputField;
    public InputField passwordInputField;
    private DatabaseReference dbReference;
    public UserData userData;
    public UIErrorHandler errorHandler;
    public Button viewPassButton; 

    public Button backButton;

    public bool switchScene = false;

    void Start()
    {

         originalY = panel.anchoredPosition.y;

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

    public void OnLoginButtonClicked()
    {
        string email = usernameInputField.text.Trim();
        string rawPassword = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(rawPassword))
        {
            errorHandler.ShowError("Please input email and password");
            return;
        }

        if (!email.EndsWith("@gmail.com"))
        {
            errorHandler.ShowError("Email should be valid");
            return;
        }

        if (rawPassword.Length < 8)
        {
            errorHandler.ShowError("Password should have 8 minimum characters");
            return;
        }

        string hashedPassword = HashPassword(rawPassword);
        AuthenticateUser(email, hashedPassword);
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
                string userId = null;
                string correctRole = null;

                foreach (DataSnapshot userSnapshot in snapshot.Children)
                {
                    var userJson = userSnapshot.GetRawJsonValue();
                    UserData user = JsonUtility.FromJson<UserData>(userJson);

                    if (user.email == email && user.password == hashedPassword)
                    {
                        loginSuccess = true;
                        userId = userSnapshot.Key;
                        correctRole = user.role;

                        Debug.Log($"Login Success! UserID: {userId}, Role: {correctRole}");

                        PlayerPrefs.SetString("LoggedInUserID", userId);
                        PlayerPrefs.Save();
                        UserSession.UserId = userId;

                        switchScene = true;
                        break;
                    }
                }

                if (loginSuccess)
                {
                    string selectedRole = PlayerPrefs.GetString("SelectedRole", "");

                    if (correctRole != selectedRole)
                    {
                        Debug.Log($"Role mismatch detected! Correcting role to {correctRole}");
                        PlayerPrefs.SetString("SelectedRole", correctRole);
                        PlayerPrefs.Save();
                    }

                    switchScene = false;
                    if (correctRole == "Student")
                    {
                        SceneManager.LoadScene("DashboardPage");
                    }
                    else if (correctRole == "Professor")
                    {
                        SceneManager.LoadScene("ProfessorDashboard");
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
        SceneManager.LoadScene("TermsAndCondition");
    }
    private bool isPasswordVisible = false;

public void OnViewPasswordButtonClicked()
{
    isPasswordVisible = !isPasswordVisible;

    passwordInputField.contentType = isPasswordVisible 
        ? InputField.ContentType.Standard 
        : InputField.ContentType.Password;

    // Force the InputField to update its display
    passwordInputField.ForceLabelUpdate();
}

 void Update()
{
    float targetY = originalY;

    if (TouchScreenKeyboard.visible)
    {
        if (usernameInputField.isFocused)
        {
            targetY = originalY + 100;
        }
        else if (passwordInputField.isFocused)
        {
            targetY = originalY + 200;
        }
    }

    if (panel.anchoredPosition.y != targetY)
    {
        panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, targetY);
    }

    if (!TouchScreenKeyboard.visible && panel.anchoredPosition.y != originalY)
    {
        panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, originalY);
    }
}

  public void OnClickBackButton() 
    {
        SceneManager.LoadScene("LandingPage");
    }
}