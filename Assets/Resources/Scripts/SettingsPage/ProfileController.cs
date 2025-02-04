using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;


public class ProfileController : MonoBehaviour
{
    private DatabaseReference dbReference;

    public InputField firstNameInputField;
    public InputField lastNameInputField;
    public InputField fullNameInputField;
    public InputField collegeDepartmentInputField;
    public InputField programInputField;
    public InputField emailInputField;
    public InputField yearAndSectionInputField;
    public Dropdown collegeDepartmentDropdown;
    public Dropdown programDropdown;
    public Button saveButton;
    public Button editButton;
     public Text editButtonText;

    private string userId;
    private bool isEditing = false;

    void Start()
    {

         if (editButtonText == null) 
    {
        editButtonText = editButton.GetComponentInChildren<Text>(); // Automatically assign the button's text
    }

    editButtonText.text = "Edit Profile";
         
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Disable editing at start
        ToggleEditing(false);

        // Load user profile
        userId = PlayerPrefs.GetString("LoggedInUserID", "");
        if (!string.IsNullOrEmpty(userId))
        {
            LoadUserProfile(userId);
        }

        DisplayFullName();
    }


    private void LoadUserProfile(string userId)
    {
        dbReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DataSnapshot snapshot = task.Result;

                // Populate input fields
                firstNameInputField.text = snapshot.Child("firstName").Value.ToString();
                lastNameInputField.text = snapshot.Child("lastName").Value.ToString();
                emailInputField.text = snapshot.Child("email").Value.ToString();
                collegeDepartmentInputField.text = snapshot.Child("department").Value.ToString();
                programInputField.text = snapshot.Child("program").Value.ToString();
                yearAndSectionInputField.text = snapshot.Child("yearSection").Value.ToString();
            }
            else
            {
                Debug.LogError("Failed to load profile data: " + task.Exception);
            }
        });
    }

    public void OnEditButtonClicked()
    {
        isEditing = !isEditing;
        ToggleEditing(isEditing);

    if (editButtonText != null)
    {
        editButtonText.text = isEditing ? "Cancel" : "Edit Profile  ";
        Debug.Log("Button Text Changed to: " + editButtonText.text);
    }
    else
    {
        Debug.LogError("editButtonText is NULL! Make sure it is assigned in the Inspector.");
    }
    }

    private void ToggleEditing(bool enable)
    {

        firstNameInputField.gameObject.SetActive(enable);
        lastNameInputField.gameObject.SetActive(enable);

        fullNameInputField.gameObject.SetActive(!enable);

        collegeDepartmentDropdown.gameObject.SetActive(enable);
        programDropdown.gameObject.SetActive(enable);

        collegeDepartmentInputField.gameObject.SetActive(!enable);
        programInputField.gameObject.SetActive(!enable);

        firstNameInputField.interactable = enable;
        lastNameInputField.interactable = enable;
        emailInputField.interactable = enable;
        collegeDepartmentInputField.interactable = enable;
        programInputField.interactable = enable;
        yearAndSectionInputField.interactable = enable;
        saveButton.gameObject.SetActive(enable);
    }

    public void OnSaveButtonClicked()
    {
        string selectedDepartment = collegeDepartmentDropdown.options[collegeDepartmentDropdown.value].text;
        string selectedProgram = programDropdown.options[programDropdown.value].text;

        // Update Firebase with new data
        dbReference.Child("users").Child(userId).UpdateChildrenAsync(new System.Collections.Generic.Dictionary<string, object>
        {
            { "firstName", firstNameInputField.text },
            { "lastName", lastNameInputField.text },
            { "email", emailInputField.text },
            { "department", selectedDepartment},
            { "program", selectedProgram},
            { "yearSection", yearAndSectionInputField.text}
        }).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Profile updated successfully!");
                ToggleEditing(false); // Disable editing after saving
                collegeDepartmentInputField.text = selectedDepartment;
                programInputField.text = selectedProgram;
                editButton.GetComponentInChildren<Text>().text = "Edit Profile"; // Reset button label
            }
            else
            {
                Debug.LogError("Failed to update profile: " + task.Exception);
            }
        });
    }

    public void DisplayFullName()
    {
        string fullName = firstNameInputField.text + " " + lastNameInputField.text;
        fullNameInputField.text = fullName;  // Display combined name in Full Name InputField
        fullNameInputField.interactable = false;
    }

}
