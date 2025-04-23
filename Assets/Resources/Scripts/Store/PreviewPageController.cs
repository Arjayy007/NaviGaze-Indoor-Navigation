using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PreviewPageController : MonoBehaviour
{

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
