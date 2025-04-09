using UnityEngine;
using UnityEngine.SceneManagement;

public class MoonBoxController : MonoBehaviour
{
    // M�todo chamado pelo bot�o que volta para o menu inicial.
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // M�todo chamado pelo bot�o que inicia a run do jogo.
    public void StartRun()
    {
        SceneManager.LoadScene("RunScene");
    }
}
