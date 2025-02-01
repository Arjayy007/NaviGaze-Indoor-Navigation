using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class NavigationPanel : MonoBehaviour
{
    public SceneManagerScript sceneManager;

    void Start()
    {
        
    }


    void Update()
    {
 
    }


    public void LoadHistoryScene()
    {
        SceneManager.LoadScene("HistoryPage");
    }

    public void LoadDashboardScene()
    {
        SceneManager.LoadScene("DashboardPage");
    }

    public void LoadCollectionScene()
    {
        SceneManager.LoadScene("CollectionPage");
    }

    public void LoadStoreScene()
    {
        SceneManager.LoadScene("StorePage");
    }

    public void LoadLocationScene()
    {
        SceneManager.LoadScene("LocationPage");
    }
    public void LoadNavigationScene(){
        SceneManager.LoadScene("NavigationCongress");
    }
    public void CloseNavigationScene(){
        SceneManager.LoadScene("DashboardPage");
    }
}
