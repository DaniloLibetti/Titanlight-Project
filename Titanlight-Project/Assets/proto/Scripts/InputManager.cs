using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Player1")]
    [SerializeField] GameObject range1OptionsP1;
    [SerializeField] GameObject range2OptionsP1;
    [SerializeField] GameObject chip1OptionsP1;
    [SerializeField] GameObject chip2OptionsP1;
    [SerializeField] GameObject granadeOptionsP1;
    [SerializeField] GameObject healOptionsP1;


    [Header("Player2")]
    [SerializeField] GameObject range1OptionsP2;
    [SerializeField] GameObject range2OptionsP2;
    [SerializeField] GameObject chip1OptionsP2;
    [SerializeField] GameObject chip2OptionsP2;
    [SerializeField] GameObject granadeOptionsP2;
    [SerializeField] GameObject healOptionsP2;


    public bool OpenCloseInput { get; private set; }

    private PlayerInput playerInput;
    private InputAction uiInput;


    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        uiInput = playerInput.actions["Navigate"];
        EventSystem.current.SetSelectedGameObject(range1OptionsP1);
    }

    void Update()
    {
        OpenCloseInput = uiInput.WasPressedThisFrame();
    }
}
