using userDataModel.Models;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;
using Firebase.Firestore;

public class RegistrationPage : MonoBehaviour
{


    public RectTransform panel; // The UI panel to move
    private float originalY;
    private bool keyboardVisible = false;




    public SceneManagerScript sceneManager;
    private DatabaseReference dbReference;
    private FirebaseFirestore firestore;



    public InputField firstNameInput;
    public InputField lastNameInput;
    public InputField yearSectionInput;
    public InputField emailInput;
    public InputField passwordInput;
    public InputField confirmPasswordInput;
    public DropdownController dropdownController;
    public UserData userData;
    [SerializeField] private Button registerButton;

    public Button backButton;



    private bool switchScene = false;

    private string GoogleUserEmail, GoogleUserFirstName, GoogleUserLastName;
    private bool isGoogleSignUp = false;

    void Start()
    {

         originalY = panel.anchoredPosition.y;
        registerButton.interactable = false; // Disable at the start



        // Retrieve Google Sign-Up data from PlayerPrefs
        GoogleUserEmail = PlayerPrefs.GetString("userEmail", "");
        GoogleUserFirstName = PlayerPrefs.GetString("userFirstName", "");
        GoogleUserLastName = PlayerPrefs.GetString("userLastName", "");
        isGoogleSignUp = PlayerPrefs.GetInt("isGoogleSignUp", 0) == 1;


        Debug.Log($"Google Sign-Up Data Retrieved: Email: {GoogleUserEmail}, First Name: {GoogleUserFirstName}, Last Name: {GoogleUserLastName}, Is Google Sign-Up: {isGoogleSignUp}");

       FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
{
    if (task.Result == DependencyStatus.Available)
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
        Debug.Log("Firestore initialized successfully");

        registerButton.interactable = true;
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


    float targetY = originalY;

if (TouchScreenKeyboard.visible)
{
    if (firstNameInput.isFocused || lastNameInput.isFocused || yearSectionInput.isFocused)
    {
        targetY = originalY + 100;
    }
    else if (emailInput.isFocused)
    {
        targetY = originalY + 500;
    }
    else if (passwordInput.isFocused)
    {
        targetY = originalY + 500;
    }
    else if (confirmPasswordInput.isFocused)
    {
        targetY = originalY + 600;
    }
}
else
{
    targetY = originalY; // reset when keyboard is hidden
}

if (panel.anchoredPosition.y != targetY)
{
    panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, targetY);
}




        if (switchScene)
        {
            switchScene = false;
            SceneManager.LoadScene("AddSchedulePage");
        }
    }

  public void SaveToDatabase()
{
    if (firestore == null)
    {
        Debug.LogError("Firestore is not initialized.");
        return;
    }

    // Validate UI references
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

    // Get values from input
    string firstName = firstNameInput.text.Trim();
    string lastName = lastNameInput.text.Trim();
    string email = emailInput.text.Trim();
    string password = passwordInput.text;
    string confirmPassword = confirmPasswordInput.text;
    string yearSection = yearSectionInput.text.Trim();
    string department = dropdownController.collegeDepartment.options[dropdownController.collegeDepartment.value].text;
    string program = dropdownController.collegeProgram.options[dropdownController.collegeProgram.value].text;
    string selectedRole = PlayerPrefs.GetString("SelectedRole", "");

    // Validate
    string validationError = Validation.ValidateRegistrationInputs(firstName, lastName, email, password, confirmPassword, yearSection, department, program, isGoogleSignUp);
    if (validationError != null)
    {
        Debug.LogError(validationError);
        return;
    }

    // Hash password if needed
    if (!isGoogleSignUp)
    {
        password = HashPassword(password);
    }
    else
    {
        password = "";
    }

    // Create userData object
    userData = new UserData(firstName, lastName, email, password, department, program, yearSection, selectedRole);

    // Generate new user ID
    string userId = firestore.Collection("users").Document().Id;
    UserSession.UserId = userId;

    // Save to: users > userId > information > profile
    firestore.Collection("users").Document(userId)
        .Collection("information").Document("profile")
        .SetAsync(userData).ContinueWithOnMainThread(task =>
    {
        if (task.IsCompletedSuccessfully)
        {
            Debug.Log("User profile saved to Firestore under users > userId > information > profile");
            ClearPlayerPrefs();
            switchScene = true;
        }
        else
        {
            Debug.LogError("Failed to save user profile: " + task.Exception);
        }
    });
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

    public void OnTogglePasswordVisibility()
{
    if (passwordInput == null)
    {
        Debug.LogError("passwordInput is NULL!");
        return;
    }

    if (passwordInput.contentType == InputField.ContentType.Password)
    {
        passwordInput.contentType = InputField.ContentType.Standard; // Show text
    }
    else
    {
        passwordInput.contentType = InputField.ContentType.Password; // Hide text
    }
    passwordInput.ForceLabelUpdate(); 
}

public void OnConfirmTogglePasswordVisibility()
{
    if (confirmPasswordInput == null)
    {
        Debug.LogError("passwordInput is NULL!");
        return;
    }

    if (confirmPasswordInput.contentType == InputField.ContentType.Password)
    {
        confirmPasswordInput.contentType = InputField.ContentType.Standard; // Show text
    }
    else
    {
        confirmPasswordInput.contentType = InputField.ContentType.Password; // Hide text
    }
    confirmPasswordInput.ForceLabelUpdate(); 
}

  public void OnClickBackButton() 
    {
        SceneManager.LoadScene("StudentLogin");
    }

}
