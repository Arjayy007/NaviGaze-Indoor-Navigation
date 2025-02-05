using userDataModel.Models;
using ProfessorDataModel.Models;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;

public class ProfRegController : MonoBehaviour
{
    public SceneManagerScript sceneManager;
    private DatabaseReference dbReference;

    public InputField firstNameInput;
    public InputField lastNameInput;
    public InputField emailInput;
    public InputField passwordInput;
    public InputField confirmPasswordInput;
    public Dropdown collegeDepartment;
    public ProfessorData professorData;


    private bool switchScene = false;


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

    // Update is called once per frame
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

        if (firstNameInput == null || lastNameInput == null || emailInput == null || passwordInput == null || confirmPasswordInput == null)
        {
            Debug.LogError("One or more input fields are not assigned.");
            return;
        }

        string firstName = firstNameInput.text.Trim();
        string lastName = lastNameInput.text.Trim();
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;
        string department = collegeDepartment.options[collegeDepartment.value].text;
        string selectedRole = PlayerPrefs.GetString("SelectedRole", "");

        string validationError = Validation.ValidateProfessorRegistrationInputs(firstName, lastName, email, password, confirmPassword, department);

        // Use Validation to Check all the Inputs
        if (validationError != null)
        {
            Debug.LogError(validationError);
            return;
        }

        // Hash password before saving
        string hashedPassword = HashPassword(password);

        // Create professor data object
        professorData = new ProfessorData(firstName, lastName, email, hashedPassword, department, selectedRole);
        string json = JsonUtility.ToJson(professorData);

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
