using UnityEngine;
using Firebase.Database;
using TMPro;

public class InAppNotification : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationMessage;

    [Header("Audio")]
    public AudioSource notificationAudioSource;
    public AudioClip notificationSound;

    private void ToggleNotificationPanel(bool show)
    {
        Animator animator = notificationPanel.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("Show", show);
        }

        if (show && notificationAudioSource != null && notificationSound != null)
        {
            notificationAudioSource.PlayOneShot(notificationSound);
        }
    }

    public void ShowSystemNotification(string message)
    {
        Debug.Log("Showing system notification: " + message);

        // Update UI
        notificationMessage.text = message;
        ToggleNotificationPanel(true);
    }

    private void HideNotificationPanel()
    {
        ToggleNotificationPanel(false);
    }

    public void testNotification()
    {
        ToggleNotificationPanel(true);
    }
}
