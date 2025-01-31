using UnityEngine;

public class CollectionVisibility : MonoBehaviour
{
    public GameObject collectionPanel;
    public GameObject badgesPanel;
    public GameObject missionsPanel;
    public GameObject userAccessoriesPanel;

    public void ShowCollection() 
    {
        collectionPanel.SetActive(true);
        badgesPanel.SetActive(false);
        missionsPanel.SetActive(false);
    }

    public void ShowBadges() 
    {
        badgesPanel.SetActive(true);
        missionsPanel.SetActive(false);
        collectionPanel.SetActive(false);
    }

    public void ShowMissions() 
    {
        missionsPanel.SetActive(true);
        collectionPanel.SetActive(false);
        badgesPanel.SetActive(false);
    }

    public void ShowUserAccessories() 
    {
        userAccessoriesPanel.SetActive(true);
    }


}
