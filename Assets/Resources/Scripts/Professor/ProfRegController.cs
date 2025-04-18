using userDataModel.Models;
using ProfessorDataModel.Models;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;

public class ProfRegController : MonoBehaviour
{
    public SceneManagerScript sceneManager;

    public InputField firstNameInput;
    public InputField lastNameInput;
    public InputField emailInput;
    public InputField passwordInput;
    public InputField confirmPasswordInput;
    public Dropdown collegeDepartment;
    public ProfessorData professorData;

    private FirebaseFirestore firestore;
    private bool switchScene = false;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                firestore = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firestore Initialized Successfully");
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
            SceneManager.LoadScene("AddSchedulePage");
        }
    }

    public void SaveToDatabase()
    {
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

        if (validationError != null)
        {
            Debug.LogError(validationError);
            return;
        }

        string hashedPassword = HashPassword(password);

        professorData = new ProfessorData(firstName, lastName, email, hashedPassword, department, selectedRole);

        // Generate a unique user ID manually
        string userId = firestore.Collection("users").Document().Id;
        if (!string.IsNullOrEmpty(userId))
        {
            Debug.Log($"Generated User ID: {userId}");
            UserSession.UserId = userId;

            DocumentReference profileDocRef = firestore
                .Collection("users")
                .Document(userId)
                .Collection("information")
                .Document("profile");

            profileDocRef.SetAsync(professorData).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log("User profile saved to Firestore!");
                    switchScene = true;
                }
                else
                {
                    Debug.LogError("Failed to save user profile: " + task.Exception);
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
