using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window_World_Main : WindowBase, IGameInput
{
    [SerializeField] 
    private VirtualJoypad _joypad = null;

    private void Update()
    {
        var vAxis = Managers.Input.GetJoystickAxis(GameJoystickCode.Move);
        var vSwipe = Managers.Input.GetSwipeDelta(GameSwipeCode.Cam);
    }

    public override void CloseWindow()
    {
    }

    public override void OnWindow_InTopStack(bool isOpened)
    {
        base.OnWindow_InTopStack(isOpened);
        Managers.Input.RegistGameInput(this);
    }

    public override void OnWindow_OutTopStack(bool isClosed)
    {
        base.OnWindow_OutTopStack(isClosed);
        Managers.Input.UnregistGameInput(this);
    }

    public Vector2 GetJoystickAxis(GameJoystickCode joystickCode)
    {
        return _joypad.GetJoystickAxis(joystickCode);
    }

    public Vector2 GetJoystickDir(GameJoystickCode joystickCode)
    {
        return _joypad.GetJoystickDir(joystickCode);
    }

    public float GetJoystickPower(GameJoystickCode joystickCode)
    {
        return _joypad.GetJoystickPower(joystickCode);
    }

    public Vector2 GetSwipeDelta(GameSwipeCode swipeCode)
    {
        return _joypad.GetSwipeDelta(swipeCode);
    }

    public bool GetKeyDown(GameKeyCode keyCode)
    {
        return _joypad.GetKeyDown(keyCode);
    }

    public bool GetKeyUp(GameKeyCode keyCode)
    {
        return _joypad.GetKeyDown(keyCode);
    }

    public bool GetKey(GameKeyCode keyCode)
    {
        return _joypad.GetKeyDown(keyCode);
    }
}
