using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ContentManager : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform contentPanel;
    public Button nextButton;
    public Button prevButton;

    private int totalPages = 4; // Number of images
    private int currentPage = 0;
    private float pageWidth;
    private bool isScrolling = false;

    void Start()
    {
        // Calculate the page width based on screen size
        pageWidth = contentPanel.rect.width / totalPages;

        // Assign button click events
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PreviousPage);

        // Check button states
        UpdateButtons();
    }

    void NextPage()
    {
        if (!isScrolling && currentPage < totalPages - 1)
        {
            currentPage++;
            StartCoroutine(ScrollToPage());
        }
    }

    void PreviousPage()
    {
        if (!isScrolling && currentPage > 0)
        {
            currentPage--;
            StartCoroutine(ScrollToPage());
        }
    }

    IEnumerator ScrollToPage()
    {
        isScrolling = true;
        float targetX = currentPage * (1f / (totalPages - 1));

        while (Mathf.Abs(scrollRect.horizontalNormalizedPosition - targetX) > 0.001f)
        {
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(scrollRect.horizontalNormalizedPosition, targetX, Time.deltaTime * 10);
            yield return null;
        }

        scrollRect.horizontalNormalizedPosition = targetX;
        isScrolling = false;
        UpdateButtons();
    }

    void UpdateButtons()
    {
        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < totalPages - 1;
    }

      public void BackButtonClicked()
    {
        SceneManager.LoadScene("LandingPage");
    }
}
