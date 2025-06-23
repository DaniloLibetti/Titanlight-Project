using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentMenuController : MonoBehaviour
{
    [Header("Player1 UI")]
    [SerializeField] private GameObject range1OptionsP1;
    [SerializeField] private GameObject range2OptionsP1;
    [SerializeField] private GameObject chip1OptionsP1;
    [SerializeField] private GameObject chip2OptionsP1;
    [SerializeField] private GameObject granadeOptionsP1;
    [SerializeField] private GameObject healOptionsP1;
    [SerializeField] private Animator animP1;
    [SerializeField] private Button readyButtonP1;

    [Header("Player2 UI")]
    [SerializeField] private GameObject range1OptionsP2;
    [SerializeField] private GameObject range2OptionsP2;
    [SerializeField] private GameObject chip1OptionsP2;
    [SerializeField] private GameObject chip2OptionsP2;
    [SerializeField] private GameObject granadeOptionsP2;
    [SerializeField] private GameObject healOptionsP2;
    [SerializeField] private Animator animP2;
    [SerializeField] private Button readyButtonP2;

    [Header("Attack Change Buttons (exemplo)")]
    [SerializeField] private Button button1;
    [SerializeField] private Button button2;
    [SerializeField] private Button button3;

    [Header("Door Animator (opcional)")]
    [SerializeField] private Animator doorAnimator;

    // Valor modificado pelos botões de ataque
    public int buttonValue;

    private const string READY_PARAM = "isReady";
    private float lastP1ClickTime = -Mathf.Infinity;
    private float lastP2ClickTime = -Mathf.Infinity;
    private const float CLICK_COOLDOWN = 0.3f;

    private void Awake()
    {
        Debug.Log($"[EquipmentMenuController] Awake: GameObject='{gameObject.name}'");
        ValidateInspectorFields();
    }

    private void Start()
    {
        Debug.Log("[EquipmentMenuController] Start: configurando listeners e UI");
        // Registrar botões Ready de forma genérica
        RegisterReady(readyButtonP1, animP1, 1, range1OptionsP1);
        RegisterReady(readyButtonP2, animP2, 2, range1OptionsP2);

        // Listeners para mudança de ataque
        if (button1 != null) button1.onClick.AddListener(() => ChangeRangeAttack(1));
        if (button2 != null) button2.onClick.AddListener(() => ChangeRangeAttack(2));
        if (button3 != null) button3.onClick.AddListener(() => ChangeRangeAttack(3));

        // Anima porta opcional
        if (doorAnimator != null)
        {
            doorAnimator.Play("DoorOpening");
            Debug.Log("[EquipmentMenuController] DoorOpening enviado ao doorAnimator");
        }
    }

    private void RegisterReady(Button btn, Animator anim, int playerIndex, GameObject firstOption)
    {
        if (btn == null)
        {
            Debug.LogWarning($"[EquipmentMenuController] readyButtonP{playerIndex} não atribuído");
            return;
        }

        // Use o GameManager como fonte da verdade
        bool isMultiplayer = GameManager.Instance != null && GameManager.Instance.IsMultiplayer;

        bool hide = (playerIndex == 2 && !isMultiplayer);
        btn.gameObject.SetActive(!hide);
        Debug.Log($"[EquipmentMenuController] P{playerIndex} Ready ativo? {!hide}, isMultiplayer={isMultiplayer}");

        if (!hide)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnReadyClicked(anim, playerIndex, firstOption));
            Debug.Log($"[EquipmentMenuController] Listener Player{playerIndex}Ready registrado");
        }
    }

    private void OnReadyClicked(Animator anim, int playerIndex, GameObject firstOption)
    {
        Debug.Log($"[EquipmentMenuController] 👉 Entrou em Player{playerIndex}Ready()");
        float now = Time.unscaledTime;
        ref float lastClick = ref (playerIndex == 1 ? ref lastP1ClickTime : ref lastP2ClickTime);
        if (now - lastClick < CLICK_COOLDOWN)
        {
            Debug.Log($"[EquipmentMenuController] Ignorando clique P{playerIndex} devido a cooldown");
            return;
        }
        lastClick = now;

        bool next = true;
        if (anim != null && anim.HasBoolParameter(READY_PARAM))
        {
            bool current = anim.GetBool(READY_PARAM);
            next = !current;
            anim.SetBool(READY_PARAM, next);
            Debug.Log($"[EquipmentMenuController] animP{playerIndex}.SetBool('isReady', {next})");
        }

        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.SetPlayerReady(playerIndex, next);
            Debug.Log($"[EquipmentMenuController] Notificou GameManager: Player{playerIndex}Ready = {next}");
        }

        if (next && firstOption != null)
        {
            EventSystem.current.SetSelectedGameObject(firstOption);
            Debug.Log($"[EquipmentMenuController] Selecionado {firstOption.name} após Ready P{playerIndex}");
        }
    }

    // Métodos para exibir/ocultar grupos de opções do Player1
    public void ShowRange1OptionsP1() => ShowOptions(range1OptionsP1, true);
    public void ShowRange2OptionsP1() => ShowOptions(range2OptionsP1, true);
    public void Showchip1OptionsP1() => ShowOptions(chip1OptionsP1, true);
    public void Showchip2OptionsP1() => ShowOptions(chip2OptionsP1, true);
    public void ShowGranadeOptionsP1() => ShowOptions(granadeOptionsP1, true);
    public void ShowHealOptionsP1() => ShowOptions(healOptionsP1, true);
    // Métodos para Player2
    public void ShowRange1OptionsP2() => ShowOptions(range1OptionsP2, false);
    public void ShowRange2OptionsP2() => ShowOptions(range2OptionsP2, false);
    public void Showchip1OptionsP2() => ShowOptions(chip1OptionsP2, false);
    public void Showchip2OptionsP2() => ShowOptions(chip2OptionsP2, false);
    public void ShowGranadeOptionsP2() => ShowOptions(granadeOptionsP2, false);
    public void ShowHealOptionsP2() => ShowOptions(healOptionsP2, false);

    private void HideOptionsP1()
    {
        range1OptionsP1?.SetActive(false);
        range2OptionsP1?.SetActive(false);
        chip1OptionsP1?.SetActive(false);
        chip2OptionsP1?.SetActive(false);
        granadeOptionsP1?.SetActive(false);
        healOptionsP1?.SetActive(false);
    }

    private void HideOptionsP2()
    {
        range1OptionsP2?.SetActive(false);
        range2OptionsP2?.SetActive(false);
        chip1OptionsP2?.SetActive(false);
        chip2OptionsP2?.SetActive(false);
        granadeOptionsP2?.SetActive(false);
        healOptionsP2?.SetActive(false);
    }

    private void ShowOptions(GameObject group, bool isPlayer1)
    {
        if (isPlayer1) HideOptionsP1();
        else HideOptionsP2();
        if (group != null)
        {
            group.SetActive(true);
            Debug.Log($"[EquipmentMenuController] ShowOptions: {(isPlayer1 ? "P1" : "P2")} ativou {group.name}");
        }
    }

    private void ChangeRangeAttack(int value)
    {
        buttonValue = value;
        Debug.Log($"[EquipmentMenuController] ChangeRangeAttack: buttonValue = {value}");
    }

    private void ValidateInspectorFields()
    {
        if (animP1 == null)
            Debug.LogWarning("[EquipmentMenuController] animP1 não atribuído!");
        else if (!animP1.HasBoolParameter(READY_PARAM))
            Debug.LogWarning("[EquipmentMenuController] animP1 falta parâmetro 'isReady'!");

        if (animP2 == null)
            Debug.LogWarning("[EquipmentMenuController] animP2 não atribuído!");
        else if (!animP2.HasBoolParameter(READY_PARAM))
            Debug.LogWarning("[EquipmentMenuController] animP2 falta parâmetro 'isReady'!");
    }
}

public static class AnimatorExtensions
{
    public static bool HasBoolParameter(this Animator animator, string paramName)
    {
        if (animator == null) return false;
        foreach (var p in animator.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Bool && p.name == paramName)
                return true;
        }
        return false;
    }
}