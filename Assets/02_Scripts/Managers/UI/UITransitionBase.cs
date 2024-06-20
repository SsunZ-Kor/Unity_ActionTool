using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UITransitionBase : MonoBehaviour
{
    public abstract void PlayAnim_Open(System.Action endCallback);
    public abstract void PlayAnim_Close(System.Action endCallback);
}