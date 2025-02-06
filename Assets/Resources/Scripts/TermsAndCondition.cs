using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Data;

public class TermsAndCondition : MonoBehaviour
{
    public Button agreeButton;           // "I Agree" button
    public Button createAccountButton;   // "Create Account" button
    public Button backButton;

    public SceneManagerScript sceneManager;
    private string selectedRole;


    void Start()
    {
        // Disable "Create Account" button initially
        createAccountButton.interactable = false;

        // Add listener to "I Agree" button
        agreeButton.onClick.AddListener(AgreeToTerms);
    }

    private void AgreeToTerms()
    {
        // Temporarily save "I Agree" in PlayerPrefs (for session)
        PlayerPrefs.SetInt("AgreedToTerms", 1);
        PlayerPrefs.Save();

        // Enable the "Create Account" button
        createAccountButton.interactable = true;
    }


    public void OnClickCreateAccountButton()
    {
        string selectedRole = PlayerPrefs.GetString("SelectedRole", "");

        Debug.Log("Role selected: " + selectedRole);

        if (selectedRole == "Student")
        {
            SceneManager.LoadScene("RegistrationPage");
        }
        else if (selectedRole == "Professor")
        {
            SceneManager.LoadScene("ProfessorRegistration");
        }
    }

    public void OnClickBackButton() 
    {
        SceneManager.LoadScene("LandingPage");
    }
}
