using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InternetChecker : MonoBehaviour
{
    public static InternetChecker Instance;

    public GameObject noInternetPanel;
    public float checkInterval = 2f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make persistent
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (noInternetPanel != null)
            noInternetPanel.SetActive(false);

        StartCoroutine(CheckInternetLoop());
    }

    IEnumerator CheckInternetLoop()
    {
        while (true)
        {
            yield return StartCoroutine(CheckInternet());
            yield return new WaitForSeconds(checkInterval);
        }
    }

    IEnumerator CheckInternet()
    {
        bool isConnected = Application.internetReachability != NetworkReachability.NotReachable;

        if (!isConnected && noInternetPanel != null)
        {
            noInternetPanel.SetActive(true);
            yield return new WaitForSeconds(2f);
            noInternetPanel.SetActive(false);
        }
    }
}
