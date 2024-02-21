using UnityEditor;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject quitButton;

    void Update()
    {
        if (GameManager.Instance.State == GameState.OnMenu)
        {
            restartButton.SetActive(false);
            startGameButton.SetActive(true);
            continueButton.SetActive(false);
            quitButton.SetActive(true);
        }
        else if (GameManager.Instance.State == GameState.Paused)
        {
            restartButton.SetActive(true);
            startGameButton.SetActive(false);
            continueButton.SetActive(true);
            quitButton.SetActive(true);
        }
        else if (GameManager.Instance.State == GameState.Playing)
        {
            restartButton.SetActive(false);
            startGameButton.SetActive(false);
            continueButton.SetActive(false);
            quitButton.SetActive(false);
        }
    }

    public void StartGame() => GameManager.Instance.NewGame();

    public void Continue() => GameManager.Instance.Unpause();

    public void QuitGame() => EditorApplication.ExitPlaymode();

    public void Restart() => GameManager.Instance.Restart();



}
