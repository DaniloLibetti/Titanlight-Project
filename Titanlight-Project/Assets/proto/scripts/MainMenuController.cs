using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Painéis configuráveis pelo inspetor
    [Header("Painéis do Menu")]
    [SerializeField] private GameObject mainMenuPanel;    // Painel com Start, Options e Quit
    [SerializeField] private GameObject optionsPanel;     // Painel de opções

    // Nome da cena para iniciar o jogo (defina no inspetor ou altere aqui)
    [Header("Configurações de Cena")]
    [SerializeField] private string gameSceneName = "GameScene";

    // Quando o jogo inicia, mostra o menu principal
    void Start()
    {
        ShowMainMenu();
    }

    // Método chamado pelo botão Start para carregar a próxima cena
    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Método chamado pelo botão Options para abrir o menu de opções
    public void OpenOptions()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    // Método chamado pelo botão Back, no menu de opções, para voltar ao menu principal
    public void BackToMainMenu()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    // Método chamado pelo botão Quit para fechar o jogo
    public void QuitGame()
    {
        Application.Quit();

        // Se estiver executando na Unity Editor, encerra o modo de jogo.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Exibe o menu principal e garante que as opções estejam fechadas
    private void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }
}
