using UnityEngine;


public class SlideUpAnimation : MonoBehaviour
{
    public  GameObject slideUpPanel;
 
    public void ToggleNotificationPanel(bool open)
{
    Animator animator = slideUpPanel.GetComponent<Animator>();
    if (animator != null)
    {
        animator.SetBool("Open", open);
    }
}

}
