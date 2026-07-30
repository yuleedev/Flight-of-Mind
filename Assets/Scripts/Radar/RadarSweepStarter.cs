using UnityEngine;

public class RadarSweepStarter : MonoBehaviour
{
    [SerializeField] private AudioSource sweepSource;
    [SerializeField] private bool playSweepSound = false;

    private void OnDisable()
    {
        if (!playSweepSound)
        {
            return;
        }

        if (sweepSource != null && !sweepSource.isPlaying)
        {
            sweepSource.Play();
        }
    }
}
