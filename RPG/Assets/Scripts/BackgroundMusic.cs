using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    [SerializeField] private AudioClip music;

    private void Start()
    {
        AudioSource src = GetComponent<AudioSource>();
        src.clip = music;
        src.loop = true;
        src.Play();
    }
}
