using UnityEngine;
using UnityEngine.UI;

public class StoreVisibility : MonoBehaviour
{
    public GameObject accessoriesUI; 
    public Button openAccessoriesButton;

    void Start()
    {
        accessoriesUI.SetActive(false);

        openAccessoriesButton.onClick.AddListener(ToggleShopVisibility);
    }

    void ToggleShopVisibility()
    {
        accessoriesUI.SetActive(!accessoriesUI.activeSelf);
    }
}
