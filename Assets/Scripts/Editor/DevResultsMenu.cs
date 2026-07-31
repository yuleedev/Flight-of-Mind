using UnityEditor;
using UnityEngine;

public static class DevResultsMenu
{
    private const string MenuRoot = "Flight of Mind/Results Screen/";

    [MenuItem(MenuRoot + "Fake Results + Open Results Screen (F9)")]
    private static void FakeResultsAndOpen()
    {
        if (!RequirePlayMode())
        {
            return;
        }

        DevFakeResults.FillAll();
        DevFakeResults.OpenResultsScreen();
    }

    [MenuItem(MenuRoot + "Empty Results + Open Results Screen (F10)")]
    private static void EmptyResultsAndOpen()
    {
        if (!RequirePlayMode())
        {
            return;
        }

        DevFakeResults.ClearAll();
        DevFakeResults.OpenResultsScreen();
    }

    [MenuItem(MenuRoot + "Fake Results Only (stay in this scene)")]
    private static void FakeResultsOnly()
    {
        if (!RequirePlayMode())
        {
            return;
        }

        DevFakeResults.FillAll();
    }

    private static bool RequirePlayMode()
    {
        if (Application.isPlaying)
        {
            return true;
        }

        EditorUtility.DisplayDialog(
            "Enter Play Mode first",
            "Results are stored in memory for the current play session, so they only " +
            "survive while the game is running.\n\nPress Play, then run this again.",
            "OK");

        return false;
    }
}
