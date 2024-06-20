using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Game;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class VirtualJoypad : MonoBehaviour
{
    [SerializeField]
    private GameJoystick _stick_Move;
    [SerializeField]
    private SwipePanel _swipe_Cam;
    
    [SerializeField]
    private GameButton _btn_Act;
    [SerializeField]
    private GameButton _btn_Jump;
    [SerializeField]
    private GameButton _btn_Skill01;
    [SerializeField]
    private GameButton _btn_Skill02;
    [SerializeField]
    private GameButton _btn_Skill03;

    private void OnEnable()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (Cursor.visible)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public Vector2 GetJoystickAxis(GameJoystickCode joystickCode)
    {
        if (!isActiveAndEnabled || !_stick_Move || joystickCode != GameJoystickCode.Move)
            return Vector2.zero;
        
        Vector2 vResult;
        vResult.x = Input.GetAxis("MoveX");
        vResult.y = Input.GetAxis("MoveY");
                
        var fScala = vResult.magnitude;
        if (fScala > 1f)
            return vResult * (1f / fScala);
        if (fScala > float.Epsilon)
            return vResult;

        return _stick_Move.Asix;
    }

    public Vector2 GetJoystickDir(GameJoystickCode joystickCode)
    {
        if (!isActiveAndEnabled || !_stick_Move || joystickCode != GameJoystickCode.Move)
            return Vector2.zero;
        
        Vector2 vResult;
        vResult.x = Input.GetAxis("MoveX");
        vResult.y = Input.GetAxis("MoveY");
                
        var sqrMag = vResult.sqrMagnitude;
        if (sqrMag > float.Epsilon)
        {
            vResult *= 1f / Mathf.Sqrt(sqrMag);
            return vResult;
        }

        return _stick_Move.Dir;
    }

    public float GetJoystickPower(GameJoystickCode joystickCode)
    {
        if (!isActiveAndEnabled || !_stick_Move || joystickCode != GameJoystickCode.Move)
            return 0f;
        
        Vector2 vResult;
        vResult.x = Input.GetAxis("MoveX");
        vResult.y = Input.GetAxis("MoveY");
                
        var sqrMag = vResult.sqrMagnitude;
        if (sqrMag > float.Epsilon)
            return Mathf.Sqrt(sqrMag);

        return _stick_Move.Power;
    }

    public Vector2 GetSwipeDelta(GameSwipeCode swipeCode)
    {
        if (!isActiveAndEnabled || !_stick_Move || swipeCode != GameSwipeCode.Cam)
            return Vector2.zero;

        if (!Cursor.visible)
        {
            Vector2 vResult;
            vResult.x = Input.GetAxis("CamX");
            vResult.y = Input.GetAxis("CamY");
        
            var sqrMag = vResult.sqrMagnitude;
            if (sqrMag > float.Epsilon)
                return vResult * 15f;   
        }

        return _swipe_Cam.Swipe;
    }

    public bool GetKeyDown(GameKeyCode keyCode)
    {
        var activeAndEnabled = isActiveAndEnabled;
        return keyCode switch
        {
            GameKeyCode.Move    => activeAndEnabled && _stick_Move  != null && _stick_Move .IsDown,
            GameKeyCode.Cam     => activeAndEnabled && _swipe_Cam   != null && _swipe_Cam  .IsDown,
            GameKeyCode.Act     => activeAndEnabled && _btn_Act     != null && (_btn_Act    .IsDown || Input.GetKeyDown(KeyCode.Mouse0)),
            GameKeyCode.Jump    => activeAndEnabled && _btn_Jump    != null && (_btn_Jump   .IsDown || Input.GetKeyDown(KeyCode.Mouse1)),
            GameKeyCode.Skill01 => activeAndEnabled && _btn_Skill01 != null && (_btn_Skill01.IsDown || Input.GetKeyDown(KeyCode.Alpha1)),
            GameKeyCode.Skill02 => activeAndEnabled && _btn_Skill02 != null && (_btn_Skill02.IsDown || Input.GetKeyDown(KeyCode.Alpha2)),
            GameKeyCode.Skill03 => activeAndEnabled && _btn_Skill03 != null && (_btn_Skill03.IsDown || Input.GetKeyDown(KeyCode.Alpha3)),
            _                   => false,
        };
    }

    public bool GetKeyUp(GameKeyCode keyCode)
    {
        var activeAndEnabled = isActiveAndEnabled;
        return keyCode switch
        {
            GameKeyCode.Move    => activeAndEnabled && _stick_Move  != null && _stick_Move .IsUp,
            GameKeyCode.Cam     => activeAndEnabled && _swipe_Cam   != null && _swipe_Cam  .IsUp,
            GameKeyCode.Act     => activeAndEnabled && _btn_Act     != null && (_btn_Act    .IsUp || Input.GetKeyUp(KeyCode.Mouse0)),
            GameKeyCode.Jump    => activeAndEnabled && _btn_Jump    != null && (_btn_Jump   .IsUp || Input.GetKeyUp(KeyCode.Mouse1)),
            GameKeyCode.Skill01 => activeAndEnabled && _btn_Skill01 != null && (_btn_Skill01.IsUp || Input.GetKeyUp(KeyCode.Alpha1)),
            GameKeyCode.Skill02 => activeAndEnabled && _btn_Skill02 != null && (_btn_Skill02.IsUp || Input.GetKeyUp(KeyCode.Alpha2)),
            GameKeyCode.Skill03 => activeAndEnabled && _btn_Skill03 != null && (_btn_Skill03.IsUp || Input.GetKeyUp(KeyCode.Alpha3)),
            _                   => false,
        };
    }

    public bool GetKey(GameKeyCode keyCode)
    {
        var activeAndEnabled = isActiveAndEnabled;
        return keyCode switch
        {
            GameKeyCode.Move    => activeAndEnabled && _stick_Move  != null && _stick_Move .IsPressed,
            GameKeyCode.Cam     => activeAndEnabled && _swipe_Cam   != null && _swipe_Cam  .IsPressed,
            GameKeyCode.Act     => activeAndEnabled && _btn_Act     != null && (_btn_Act    .IsPressed || Input.GetKey(KeyCode.Mouse0)),
            GameKeyCode.Jump    => activeAndEnabled && _btn_Jump    != null && (_btn_Jump   .IsPressed || Input.GetKey(KeyCode.Mouse1)),
            GameKeyCode.Skill01 => activeAndEnabled && _btn_Skill01 != null && (_btn_Skill01.IsPressed || Input.GetKey(KeyCode.Alpha1)),
            GameKeyCode.Skill02 => activeAndEnabled && _btn_Skill02 != null && (_btn_Skill02.IsPressed || Input.GetKey(KeyCode.Alpha2)),
            GameKeyCode.Skill03 => activeAndEnabled && _btn_Skill03 != null && (_btn_Skill03.IsPressed || Input.GetKey(KeyCode.Alpha3)),
            _                   => false,
        };
    }
}