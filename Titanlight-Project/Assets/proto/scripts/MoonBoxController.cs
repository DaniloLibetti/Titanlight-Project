using UnityEngine;
using UnityEngine.SceneManagement;

public class MoonBoxController : MonoBehaviour
{
    // Método chamado pelo botão que volta para o menu inicial.
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Método chamado pelo botão que inicia a run do jogo.
    public void StartRun()
    {
        SceneManager.LoadScene("RunScene");
    }
}
