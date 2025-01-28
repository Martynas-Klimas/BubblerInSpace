using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFinishedScreen : MonoBehaviour
{
    public void GameFinished()
    {   
        //show game menu
        gameObject.SetActive(true);
    }

    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
