using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PreviewPageController : MonoBehaviour
{

    // Reference to all item models in the Preview Page
    public GameObject CatHat;
    public GameObject CatGlasses;


    void Start()
    {
        string itemName = ItemPreviewData.itemName;
        string itemPrice = ItemPreviewData.itemPrice;

        Debug.Log("Preview: " + itemName);

        ApplyAccessoryToCat(itemName);
    }

    void ApplyAccessoryToCat(string itemName)
    {
        // Disable all accessories first
        CatHat.SetActive(false);
        CatGlasses.SetActive(false);

        // Enable only the selected accessory on the Cat Character
        switch (itemName)
        {
            case "Cat Hat":
                CatHat.SetActive(true);
                break;
            case "Cat Glasses":
                CatGlasses.SetActive(true);
                break;
            default:
                Debug.LogWarning("Accessory not found!");
                break;
        }
    }
    public void BacktoStore()
    {
        SceneManager.LoadScene("StorePage");
    }

}
