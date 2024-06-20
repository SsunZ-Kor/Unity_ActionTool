using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameJoystickCode
{
    Move,
    Cam,
}

public enum GameSwipeCode
{
    Cam,
}

public enum GameKeyCode
{
    None,
    Move,
    Cam,
    Act,
    Jump,
    Skill01,
    Skill02,
    Skill03,
    Any
}

public enum GameKeyState
{
    Down,
    Up,
    Pressed,
}

public class GameInputManager : IManager
{
    private IGameInput _crrGameInput;

    public IEnumerator Init()
    {
        yield break;
    }

    public void Release()
    {
    }

    public void RegistGameInput(IGameInput gameInput)
    {
        _crrGameInput = gameInput;
    }

    public void UnregistGameInput(IGameInput gameInput)
    {
        if (_crrGameInput == gameInput)
            _crrGameInput = null;
    }

    public Vector2 GetJoystickAxis(GameJoystickCode joystickCode)
    {
        if (_crrGameInput == null)
            return Vector2.zero;
        
        return _crrGameInput.GetJoystickAxis(joystickCode);
    }
    
    public Vector2 GetJoystickDir(GameJoystickCode joystickCode)
    {
        if (_crrGameInput == null)
            return Vector2.zero;
        
        return _crrGameInput.GetJoystickDir(joystickCode);
    }
    
    public float GetJoystickPower(GameJoystickCode joystickCode)
    {
        if (_crrGameInput == null)
            return 0f;
        
        return _crrGameInput.GetJoystickPower(joystickCode);
    }

    public Vector2 GetSwipeDelta(GameSwipeCode swipeCode)
    {
        if (_crrGameInput == null)
            return Vector2.zero;
        
        return _crrGameInput.GetSwipeDelta(swipeCode);
    }

    public bool GetKeyDown(GameKeyCode keyCode)
    {
        return _crrGameInput != null && _crrGameInput.GetKeyDown(keyCode);
    }

    public bool GetKeyUp(GameKeyCode keyCode)
    {
        return _crrGameInput != null && _crrGameInput.GetKeyUp(keyCode);
    }

    public bool GetKey(GameKeyCode keyCode)
    {
        return _crrGameInput != null && _crrGameInput.GetKey(keyCode);
    }
    
    public bool GetKey(GameKeyCode keyCode, GameKeyState keyState)
    {
        return keyState switch
        {
            GameKeyState.Down    => GetKeyDown(keyCode),
            GameKeyState.Up      => GetKeyUp(keyCode),
            GameKeyState.Pressed => GetKey(keyCode),
            _ => false,
        };
    }

    public bool GetAnyKey()
    {
        for (var keyCode = GameKeyCode.None + 1; keyCode < GameKeyCode.Any; ++keyCode)
        {
            if (GetKey(keyCode))
                return true;
        }

        return false;
    }
}

public interface IGameInput
{
    Vector2 GetJoystickAxis(GameJoystickCode joystickCode);
    Vector2 GetJoystickDir(GameJoystickCode joystickCode);
    float GetJoystickPower(GameJoystickCode joystickCode);

    Vector2 GetSwipeDelta(GameSwipeCode swipeCode);
    
    bool GetKeyDown(GameKeyCode keyCode);
    bool GetKeyUp(GameKeyCode keyCode);
    bool GetKey(GameKeyCode keyCode);
}