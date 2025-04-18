using System.Collections.Generic;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;

public class ProfileController : MonoBehaviour
{
    private FirebaseFirestore firestore;

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
        userId = UserSession.UserId;
        Debug.Log("User ID: " + userId);
        
        if (editButtonText == null)
        {
            editButtonText = editButton.GetComponentInChildren<Text>(); // Automatically assign the button's text
        }

        editButtonText.text = "Edit Profile";
        InitializeFirebase();
        ToggleEditing(false);

        

        DisplayFullName();
    }

    private void InitializeFirebase()
    {
        // Firebase initialization
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            firestore = FirebaseFirestore.GetInstance(app);
            LoadUserProfile(userId);
            Debug.Log("Firebase Initialized!");
        });
    }

private void LoadUserProfile(string userId)
{
    DocumentReference docRef = firestore
        .Collection("users")
        .Document(userId)
        .Collection("information")
        .Document("profile");

    docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompletedSuccessfully)
        {
            DocumentSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {
                // Populate input fields
                string firstName = snapshot.GetValue<string>("firstName");
                string lastName = snapshot.GetValue<string>("lastName");
                string email = snapshot.GetValue<string>("email");
                string department = snapshot.GetValue<string>("department");
                string program = snapshot.GetValue<string>("program");
                string yearSection = snapshot.GetValue<string>("yearSection");

                firstNameInputField.text = firstName;
                lastNameInputField.text = lastName;
                emailInputField.text = email;
                collegeDepartmentInputField.text = department;
                programInputField.text = program;
                yearAndSectionInputField.text = yearSection;

                Debug.Log($"User Profile Loaded: \n" +
                          $"First Name: {firstName}\n" +
                          $"Last Name: {lastName}\n" +
                          $"Email: {email}\n" +
                          $"Department: {department}\n" +
                          $"Program: {program}\n" +
                          $"Year & Section: {yearSection}");
                Debug.Log("Profile loaded successfully!");
                // Update Full Name
                DisplayFullName();
            }
            else
            {
                Debug.LogError("Profile does not exist for userId: " + userId);
            }
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
            editButtonText.text = isEditing ? "Cancel" : "Edit Profile";
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

        // Update Firestore with new data
        DocumentReference docRef = firestore
            .Collection("users")
            .Document(userId)
            .Collection("information")
            .Document("profile");

        docRef.UpdateAsync(new Dictionary<string, object>
        {
            { "firstName", firstNameInputField.text },
            { "lastName", lastNameInputField.text },
            { "email", emailInputField.text },
            { "department", selectedDepartment },
            { "program", selectedProgram },
            { "yearSection", yearAndSectionInputField.text }
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
