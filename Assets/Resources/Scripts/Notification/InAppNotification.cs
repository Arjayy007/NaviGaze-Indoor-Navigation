using UnityEngine;
using Firebase.Database;
using TMPro;

public class InAppNotification : MonoBehaviour
{

    [Header("UI Elements")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationMessage;


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

    // Assuming you are using a custom in-app notification panel
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
