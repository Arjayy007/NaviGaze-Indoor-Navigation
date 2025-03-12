using UnityEngine;
using Firebase.Database;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class InAppNotification : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationMessage;

    [Header("Audio")]

    public AudioClip notificationSound;
    public AudioSource audioSource;

    private void ToggleNotificationPanel(bool show)
    {
        Animator animator = notificationPanel.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("Show", show);
        }
    }

    public void ShowSystemNotification(string message)
    {
        Debug.Log("Showing system notification: " + message);

        // Update UI
        notificationMessage.text = message;
        ToggleNotificationPanel(true);
        PlayNotificationSound();

        StartCoroutine(HideAfterDelay(3f));
    }

     private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ToggleNotificationPanel(false);
        Debug.Log("Notification hidden after " + delay + " seconds");
    }

    private void HideNotificationPanel()
    {
        ToggleNotificationPanel(false);
    }

    public void testNotification()
    {
        SceneManager.LoadScene("NotificationPage");
    }

        private void PlayNotificationSound()
    {
        if (notificationSound != null)
        {
            audioSource.PlayOneShot(notificationSound);
            Debug.Log("Playing notification sound: " + notificationSound.name);
        }
        else
        {
            Debug.LogError("Notification sound is missing!");
        }
    }
}


