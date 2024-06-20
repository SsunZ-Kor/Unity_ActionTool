using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[UIManager.SystemUI]
public class Window_Intro_Main : WindowBase
{
    [SerializeField]
    private Slider _uiSlider_Each;
    [SerializeField]
    private Slider _uiSlider_Total;

    [SerializeField] 
    private ButtonEx _uiBtn_TouchToStart;
    [SerializeField]
    private GameObject _root_TouchToStart;

    protected override void Awake()
    {
        base.Awake();
        _uiSlider_Each.gameObject.SetActive(false);
        _uiSlider_Total.gameObject.SetActive(false);
        _root_TouchToStart.gameObject.SetActive(false);
    }

    public void SetEachProgress(float fProgress)
    {
        _uiSlider_Each.gameObject.SetActive(fProgress >= 0f);
        _uiSlider_Each.value = fProgress;
    }
    
    public void SetTotalProgress(float fProgress)
    {
        _uiSlider_Total.gameObject.SetActive(fProgress >= 0f);
        _uiSlider_Total.value = fProgress;
    }

    public void SetTouchToStart(System.Action touchCallback)
    {
        _uiSlider_Each.gameObject.SetActive(false);
        _uiSlider_Total.gameObject.SetActive(false);
        _root_TouchToStart.SetActive(true);
        _uiBtn_TouchToStart.onClick_NormalOnly.AddListener(touchCallback);
    }
}