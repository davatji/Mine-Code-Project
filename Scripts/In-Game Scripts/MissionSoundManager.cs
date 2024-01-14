using UnityEngine;

public class MissionSoundManager : MonoBehaviour
{
    //bound with mission completion audio source component
    private AudioSource audioSource;
    void Awake(){
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
    }

    //will be called whenever a task is completed
    public void PlaySound(){
        audioSource.Play();
    }
}
