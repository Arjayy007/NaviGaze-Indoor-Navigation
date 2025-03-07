using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource audioSource;
    public AudioClip buttonClickSFX;

    void Awake()
    {
        // Singleton pattern: Ensures only one AudioManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this across all scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayButtonClick()
    {
        if (audioSource && buttonClickSFX)
        {
            audioSource.PlayOneShot(buttonClickSFX);
        }
    }
}
