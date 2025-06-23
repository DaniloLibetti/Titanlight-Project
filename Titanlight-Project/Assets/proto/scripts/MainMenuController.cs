using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenuController : MonoBehaviour
{
    [Header("Painéis do Menu")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject modeSelectPanel;
    [SerializeField] private GameObject creditsPanel; // Novo painel de créditos

    [Header("Configurações de Cena")]
    [SerializeField] private string runSceneName = "RunScene";

    void Start()
    {
        ShowMainMenu();
    }

    // Inicia seleção de modo
    public void StartGame()
    {
        SoundManager.PlaySound(SoundType.BUTTON);
        ShowModeSelection();
    }

    public void OnSinglePlayerChosen()
    {
        SoundManager.PlaySound(SoundType.BUTTON);
        PlayerPrefs.SetInt(GameSettings.MultiplayerKey, 0);
        LoadRunScene();
    }

    public void OnMultiplayerChosen()
    {
        SoundManager.PlaySound(SoundType.BUTTON);
        PlayerPrefs.SetInt(GameSettings.MultiplayerKey, 1);
        LoadRunScene();
    }

    private void LoadRunScene()
    {
        MusicaManager.PlayMusic();
        SceneManager.LoadScene(runSceneName);
    }

    // Abre menu de opções
    public void OpenOptions()
    {
        SoundManager.PlaySound(SoundType.BUTTON);
        HideAllPanels();
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        SoundManager.PlaySound(SoundType.BUTTON);
        ShowMainMenu();
    }

    // Botão de voltar de seleção de modo
    public void BackFromModeSelection()
    {
        SoundManager.PlaySound(SoundType.BUTTON);
        ShowMainMenu();
    }

    // Exibe créditos
    public void OpenCredits()
    {
        SoundManager.PlaySound(SoundType.BUTTON);
        HideAllPanels();
        creditsPanel.SetActive(true);
    }

    // Botão de voltar de créditos
    public void BackFromCredits()
    {
        SoundManager.PlaySound(SoundType.BUTTON);
        ShowMainMenu();
    }

    public void QuitGame()
    {
        SoundManager.PlaySound(SoundType.BUTTON);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Mostra apenas painel principal
    private void ShowMainMenu()
    {
        HideAllPanels();
        mainMenuPanel.SetActive(true);
    }

    // Exibe seleção de modo
    private void ShowModeSelection()
    {
        HideAllPanels();
        modeSelectPanel.SetActive(true);
    }

    // Esconde todos os paineis
    private void HideAllPanels()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        modeSelectPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }
}