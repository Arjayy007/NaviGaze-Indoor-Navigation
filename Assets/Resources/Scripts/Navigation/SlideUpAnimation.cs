using UnityEngine;


public class SlideUpAnimation : MonoBehaviour
{
    public  GameObject slideUpPanel;
 
    public void SlideUp()
    {
        if (slideUpPanel != null)
        {
            Animator animator = slideUpPanel.GetComponent<Animator>();
            if (animator != null)
            {
                bool isOpen = animator.GetBool("Open");
                animator.SetBool("Open", !isOpen);
            }
        }
    }

    void Update()
    {
        
    }
}
