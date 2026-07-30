#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;

public static class DevFakeResults
{
    public const string ResultsSceneName = "ResultsScreen";

    public static void FillAll()
    {
        FillLandingRoutes();
        FillCargoLogistics();
        FillRadarWatch();

        Debug.Log("[DevFakeResults] Filled fake results for all three games.");
    }

    public static void ClearAll()
    {
        TrailMakingResults.Clear();
        CargoLogisticsResults.Clear();
        RadarWatchResults.Clear();

        Debug.Log("[DevFakeResults] Cleared all results.");
    }

    public static void FillLandingRoutes()
    {
        TrailMakingResults.Clear();
        TrailMakingResults.Record("A", 39.6f, 1);
        TrailMakingResults.Record("B", 87.3f, 3);
    }

    public static void FillCargoLogistics()
    {
        CargoLogisticsResults.Clear();

        CargoLogisticsResults.Record(0, true, 3, 3, 0, 4.0f, 1.0f, 1.0f, 6.0f);
        CargoLogisticsResults.Record(1, false, 4, 4, 0, 5.0f, 2.0f, 1.2f, 8.2f);
        CargoLogisticsResults.Record(2, false, 6, 5, 1, 7.0f, 4.0f, 1.5f, 12.5f);
        CargoLogisticsResults.Record(3, false, 6, 6, 0, 9.0f, 3.0f, 1.8f, 13.8f);
        CargoLogisticsResults.Record(4, false, 9, 7, 2, 11.0f, 6.0f, 2.0f, 19.0f);

        CargoLogisticsScoring.ComputeFinalScores();
    }

    public static void FillRadarWatch()
    {
        RadarWatchResults.Clear();
        RadarWatchResults.Record(74, 21, 7, 4, 3, true, 0.436f);
        RadarWatchResults.SetReactionTimeScore(66);
    }

    public static void OpenResultsScreen()
    {
        Time.timeScale = 1f;
        SceneTransition.LoadScene(ResultsSceneName);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallHotkeys()
    {
        GameObject host = new GameObject("[DevFakeResultsHotkeys]");
        host.AddComponent<DevFakeResultsHotkeys>();
        Object.DontDestroyOnLoad(host);

        Debug.Log("[DevFakeResults] F9 = fake results + results screen, " +
                  "F10 = empty results + results screen.");
    }
}

public class DevFakeResultsHotkeys : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            DevFakeResults.FillAll();
            DevFakeResults.OpenResultsScreen();
            return;
        }

        if (Keyboard.current.f10Key.wasPressedThisFrame)
        {
            DevFakeResults.ClearAll();
            DevFakeResults.OpenResultsScreen();
        }
    }
}
#endif
