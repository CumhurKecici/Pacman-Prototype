using UnityEditor;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject quitButton;

    void Update()
    {
        if (GameManager.Instance.State == GameState.OnMenu)
        {
            pauseButton.SetActive(false);
            restartButton.SetActive(false);
            startGameButton.SetActive(true);
            continueButton.SetActive(false);
            quitButton.SetActive(true);
        }
        else if (GameManager.Instance.State == GameState.Paused)
        {
            pauseButton.SetActive(false);
            restartButton.SetActive(true);
            startGameButton.SetActive(false);
            continueButton.SetActive(true);
            quitButton.SetActive(true);
        }
        else if (GameManager.Instance.State == GameState.Playing)
        {
            pauseButton.SetActive(true);
            restartButton.SetActive(false);
            startGameButton.SetActive(false);
            continueButton.SetActive(false);
            quitButton.SetActive(false);
        }
    }

    public void Pause() => GameManager.Instance.State = GameState.Paused;

    public void StartGame() => GameManager.Instance.NewGame();

    public void Continue() => GameManager.Instance.Unpause();

    public void QuitGame()
    {

#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif

    }

    public void Restart() => GameManager.Instance.Restart();



}
