using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LandingPageController : MonoBehaviour
{
    public Button studentButton;
    public Button professorButton;
    public Button guestButton;

    private string selectedRole;

    void Start()
    {
        studentButton.onClick.AddListener(() => SelectRole("Student"));
        guestButton.onClick.AddListener(() => SelectRole("Guest"));
        professorButton.onClick.AddListener(() => SelectRole("Professor"));
    }

    private void SelectRole(string role)
    {
        selectedRole = role;  
        PlayerPrefs.SetString("SelectedRole", selectedRole);  
        Debug.Log("Role selected: " + selectedRole);

        if (role == "Student" || role == "Professor")
        {
            SceneManager.LoadScene("StudentLogin");
        }
        else if (role == "Guest")
        {
            SceneManager.LoadScene("GuestLanding");  
        }
    }
}
