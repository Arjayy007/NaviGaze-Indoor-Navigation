using userDataModel.Models;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;

public class StudentLoginController : MonoBehaviour
{

    public RectTransform panel; // The UI panel to move
    private float originalY;
    private bool keyboardVisible = false;

    public SceneManagerScript sceneManager;

    public InputField usernameInputField;
    public InputField passwordInputField;
    public UserData userData;
    public UIErrorHandler errorHandler;

    public bool switchScene = false;

    private FirebaseFirestore firestore;
    public GameObject loadingPanel;
    public Animator loadingAnimator;

    void Start()
    {


         originalY = panel.anchoredPosition.y;

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

    public void OnLoginButtonClicked()
    {
        if (string.IsNullOrEmpty(usernameInputField.text.Trim()) || string.IsNullOrEmpty(passwordInputField.text))
        {
            errorHandler.ShowError("Please input email and password");
            return;
        }

        loadingPanel.SetActive(true);
        loadingAnimator.SetTrigger("PlayLoading");
        string email = usernameInputField.text.Trim();
        string password = HashPassword(passwordInputField.text);
        
        AuthenticateUser(email, password);
    }

   private void AuthenticateUser(string email, string hashedPassword)
{
   firestore.CollectionGroup("information")
    .WhereEqualTo("email", email.ToLower()) // Add ToLower if your emails are lowercase
    .GetSnapshotAsync()
    .ContinueWithOnMainThread(task =>
{
    if (task.IsCompletedSuccessfully)
    {
        var snapshot = task.Result;

        if (snapshot.Count == 0)
        {
            Debug.LogWarning($"No user found with email: {email}");
            errorHandler.ShowError("Invalid Email or wrong password");
            return;
        }

        foreach (var doc in snapshot.Documents)
        {
            string fetchedPassword = doc.GetValue<string>("password");
            string role = doc.GetValue<string>("role");

            Debug.Log($"Fetched Password: {fetchedPassword}");
            Debug.Log($"Entered Password: {hashedPassword}");

            if (fetchedPassword == hashedPassword)
            {
                // Get the userId from the document path
                string path = doc.Reference.Path; // "users/{userId}/information/profile"
                string[] parts = path.Split('/');
                string userId = parts[1]; // parts[1] = userId

                UserSession.UserId = userId;

                Debug.Log("Login successful. User ID: " + userId);

                if (role == "Student")
                    SceneManager.LoadScene("DashboardPage");
                else
                    SceneManager.LoadScene("ProfessorDashboard");

                return;
            }
            else
            {
                errorHandler.ShowError("Invalid Email or password");
            }
        }
    }
    else
    {
        Debug.LogError("Error while querying Firestore: " + task.Exception);
        errorHandler.ShowError("An error occurred. Please try again.");
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
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }

    public void OnRegisterButtonClicked()
    {
        SceneManager.LoadScene("TermsAndCondition");
    }

    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("LandingPage");
    }

    public void OnTogglePasswordVisibility()
    {
        if (passwordInputField.contentType == InputField.ContentType.Password)
            passwordInputField.contentType = InputField.ContentType.Standard;
        else
            passwordInputField.contentType = InputField.ContentType.Password;

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

}