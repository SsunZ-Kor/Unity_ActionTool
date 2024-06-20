using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderEx : Slider
{
    protected override void Awake()
    {
        base.Awake();
        
        if (fillRect != null)
            fillRect.gameObject.SetActive(value > 0f);
    }

    public override float value
    {
        get
        {
            return base.value;
        }
        set
        {
            base.value = value;
            if (fillRect != null)
                fillRect.gameObject.SetActive(value > 0f);
        }
    }
}