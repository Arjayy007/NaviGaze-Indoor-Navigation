using UnityEngine;
using UnityEngine.SceneManagement;

public class GuestController : MonoBehaviour
{

    void Start()
    {
        
    }


    public void LoadGuestDashboard() 
    {
        SceneManager.LoadScene("GuestLanding");
    }
}
