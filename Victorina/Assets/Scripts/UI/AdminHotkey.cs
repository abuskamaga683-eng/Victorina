using UnityEngine;
using UnityEngine.InputSystem;
using Victorina.Core.Interfaces;
using Victorina.Infrastructure;

public class AdminHotkey : MonoBehaviour
{
    private ISceneNavigator _navigator;

    private void Start()
    {
        Debug.Log("AdminHotkey START");

        _navigator =
            GameBootstrapper.Services
            .Resolve<ISceneNavigator>();
    }

    private void Update()
    {
        bool ctrlPressed =
            Keyboard.current.leftCtrlKey.isPressed ||
            Keyboard.current.rightCtrlKey.isPressed;

        bool altPressed =
            Keyboard.current.leftAltKey.isPressed ||
            Keyboard.current.rightAltKey.isPressed;

        bool f12Pressed =
            Keyboard.current.f12Key.wasPressedThisFrame;

        if (ctrlPressed && altPressed && f12Pressed)
        {
            Debug.Log("ADMIN HOTKEY");

            _navigator.GoToAdmin();
        }
    }
}