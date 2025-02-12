using userDataModel.Models;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;

public class RegistrationPage : MonoBehaviour
{
    public SceneManagerScript sceneManager;
    private DatabaseReference dbReference;

    public InputField firstNameInput;
    public InputField lastNameInput;
    public InputField yearSectionInput;
    public InputField emailInput;
    public InputField passwordInput;
    public InputField confirmPasswordInput;
    public DropdownController dropdownController;
    public UserData userData;

    private bool switchScene = false;

    private string GoogleUserEmail, GoogleUserFirstName, GoogleUserLastName;
    private bool isGoogleSignUp = false;

    void Start()
    {
        // Retrieve Google Sign-Up data from PlayerPrefs
        GoogleUserEmail = PlayerPrefs.GetString("userEmail", "");
        GoogleUserFirstName = PlayerPrefs.GetString("userFirstName", "");
        GoogleUserLastName = PlayerPrefs.GetString("userLastName", "");
        isGoogleSignUp = PlayerPrefs.GetInt("isGoogleSignUp", 0) == 1; // Convert int to bool

        // Debug logs for verification
        Debug.Log($"Google Sign-Up Data Retrieved: Email: {GoogleUserEmail}, First Name: {GoogleUserFirstName}, Last Name: {GoogleUserLastName}, Is Google Sign-Up: {isGoogleSignUp}");

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

        // If Google Sign-Up, pre-fill fields and disable password inputs
        if (isGoogleSignUp)
        {
            emailInput.text = GoogleUserEmail;
            firstNameInput.text = GoogleUserFirstName;
            lastNameInput.text = GoogleUserLastName;

            passwordInput.interactable = false;
            confirmPasswordInput.interactable = false;
        }
    }

    void Update()
    {
        if (switchScene)
        {
            switchScene = false;
            SceneManager.LoadScene("AddSchedulePage");
        }
    }

    public void SaveToDatabase()
    {
        if (dbReference == null)
        {
            Debug.LogError("Database reference is not initialized.");
            return;
        }

        if (firstNameInput == null || lastNameInput == null || emailInput == null || yearSectionInput == null)
        {
            Debug.LogError("One or more input fields are not assigned.");
            return;
        }

        if (dropdownController == null || dropdownController.collegeDepartment == null || dropdownController.collegeProgram == null)
        {
            Debug.LogError("DropdownController or dropdown fields are not assigned.");
            return;
        }

        // Get input values
        string firstName = firstNameInput.text.Trim();
        string lastName = lastNameInput.text.Trim();
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;
        string yearSection = yearSectionInput.text.Trim();
        string department = dropdownController.collegeDepartment.options[dropdownController.collegeDepartment.value].text;
        string program = dropdownController.collegeProgram.options[dropdownController.collegeProgram.value].text;
        string selectedRole = PlayerPrefs.GetString("SelectedRole", "");

        // Validate user inputs
        string validationError = Validation.ValidateRegistrationInputs(firstName, lastName, email, password, confirmPassword, yearSection, department, program, isGoogleSignUp);
        if (validationError != null)
        {
            Debug.LogError(validationError);
            return;
        }

        // Skip password hashing if signing up with Google
        if (!isGoogleSignUp)
        {
            password = HashPassword(password);
        }
        else
        {
            password = ""; // No password required
        }

        // Create user data object
        userData = new UserData(firstName, lastName, email, password, department, program, yearSection, selectedRole);
        string json = JsonUtility.ToJson(userData);

        string userId = dbReference.Child("users").Push().Key;
        if (userId != null)
        {
            Debug.Log($"Generated User ID: {userId}");
            UserSession.UserId = userId;

            dbReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log("User data saved to Firebase!");
                    ClearPlayerPrefs();
                    switchScene = true;
                }
                else
                {
                    Debug.LogError("Failed to save user data: " + task.Exception);
                }
            });
        }
        else
        {
            Debug.LogError("Failed to generate a unique ID for the user.");
        }
    }

 private void ClearPlayerPrefs()
{
    string userId = PlayerPrefs.GetString("UserSession.UserId", ""); // Preserve UserSession.UserId
    string selectedRole = PlayerPrefs.GetString("SelectedRole", ""); // Preserve SelectedRole

    PlayerPrefs.DeleteAll(); // Clear all PlayerPrefs

    // Restore preserved values
    PlayerPrefs.SetString("UserSession.UserId", userId);
    PlayerPrefs.SetString("SelectedRole", selectedRole);
    PlayerPrefs.Save(); // Ensure changes are saved

    Debug.Log("Cleared PlayerPrefs except UserSession.UserId and SelectedRole.");
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
}
