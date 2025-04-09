using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Pain�is configur�veis pelo inspetor
    [Header("Pain�is do Menu")]
    [SerializeField] private GameObject mainMenuPanel;    // Painel com Start, Options e Quit
    [SerializeField] private GameObject optionsPanel;     // Painel de op��es

    // Nome da cena para iniciar o jogo (defina no inspetor ou altere aqui)
    [Header("Configura��es de Cena")]
    [SerializeField] private string gameSceneName = "GameScene";

    // Quando o jogo inicia, mostra o menu principal
    void Start()
    {
        ShowMainMenu();
    }

    // M�todo chamado pelo bot�o Start para carregar a pr�xima cena
    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // M�todo chamado pelo bot�o Options para abrir o menu de op��es
    public void OpenOptions()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    // M�todo chamado pelo bot�o Back, no menu de op��es, para voltar ao menu principal
    public void BackToMainMenu()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    // M�todo chamado pelo bot�o Quit para fechar o jogo
    public void QuitGame()
    {
        Application.Quit();

        // Se estiver executando na Unity Editor, encerra o modo de jogo.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Exibe o menu principal e garante que as op��es estejam fechadas
    private void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }
}
