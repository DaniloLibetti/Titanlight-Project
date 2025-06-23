using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Player1 Options Canvases")]
    [SerializeField] private GameObject range1OptionsP1;
    [SerializeField] private GameObject range2OptionsP1;
    [SerializeField] private GameObject chip1OptionsP1;
    [SerializeField] private GameObject chip2OptionsP1;
    [SerializeField] private GameObject grenadeOptionsP1;
    [SerializeField] private GameObject healOptionsP1;

    [Header("Player2 Options Canvases")]
    [SerializeField] private GameObject range1OptionsP2;
    [SerializeField] private GameObject range2OptionsP2;
    [SerializeField] private GameObject chip1OptionsP2;
    [SerializeField] private GameObject chip2OptionsP2;
    [SerializeField] private GameObject grenadeOptionsP2;
    [SerializeField] private GameObject healOptionsP2;

    public bool OpenCloseInput { get; private set; }

    private PlayerInput _playerInput;
    private InputAction _navigateAction;

    private void Awake()
    {
        // Obtém o componente PlayerInput
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput == null)
        {
            Debug.LogError("[InputManager] PlayerInput não encontrado no GameObject.");
            enabled = false;
            return;
        }

        // Busca a ação de navegação no InputActionAsset
        /*_navigateAction = _playerInput.actions.FindAction("Navigate", true);
        if (_navigateAction == null)
        {
            Debug.LogError("[InputManager] Ação 'Navigate' não encontrada no Input Actions.");
            enabled = false;
            return;
        }*/

        // Seleciona o primeiro botão do menu de P1, se existir
        if (EventSystem.current == null)
        {
            Debug.LogWarning("[InputManager] EventSystem não encontrado na cena.");
        }
        else if (range1OptionsP1 == null)
        {
            Debug.LogWarning("[InputManager] range1OptionsP1 não atribuído no Inspector.");
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(range1OptionsP1);
        }
    }

    private void Update()
    {
        if (_navigateAction != null)
        {
            OpenCloseInput = _navigateAction.WasPressedThisFrame();
        }
    }
}
