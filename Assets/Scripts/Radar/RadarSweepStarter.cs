using UnityEngine;

public class RadarSweepStarter : MonoBehaviour
{
    [SerializeField] private AudioSource sweepSource;

    private void OnDisable()
    {
        if (sweepSource != null && !sweepSource.isPlaying)
        {
            sweepSource.Play();
        }
    }
}