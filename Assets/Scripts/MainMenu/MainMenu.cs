using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneTransition.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
