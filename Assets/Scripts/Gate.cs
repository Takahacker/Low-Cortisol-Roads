using UnityEngine;

public class Gate : MonoBehaviour
{
    public int gateNumber;
    public GameManager gameManager;
    private bool scored = false;

    private static AudioClip gateClip;

    void Awake()
    {
        if (gateClip == null)
            gateClip = CreateGateSound();
    }

    void OnTriggerEnter(Collider other)
    {
        if (scored) return;
        if (other.CompareTag("Player"))
        {
            scored = true;
            if (gameManager != null)
                gameManager.AddScore(gateNumber);

            // Play sound
            AudioSource.PlayClipAtPoint(gateClip, transform.position, 0.6f);
        }
    }

    static AudioClip CreateGateSound()
    {
        // Bright chime/ping sound
        int sampleRate = 44100;
        float duration = 0.4f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        float freq1 = 880f;  // A5
        float freq2 = 1318f; // E6
        float freq3 = 1760f; // A6

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 8f); // quick decay

            float s = Mathf.Sin(2f * Mathf.PI * freq1 * t) * 0.4f;
            s += Mathf.Sin(2f * Mathf.PI * freq2 * t) * 0.3f;
            s += Mathf.Sin(2f * Mathf.PI * freq3 * t) * 0.2f;

            data[i] = s * env;
        }

        AudioClip clip = AudioClip.Create("GateChime", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}