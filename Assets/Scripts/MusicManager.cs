using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public AudioClip music;
    [Range(0f, 1f)]
    public float volume = 0.5f;
    public bool loop = true;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = music;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.playOnAwake = false;
        audioSource.Play();
    }
}