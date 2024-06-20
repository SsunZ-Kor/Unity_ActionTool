using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Game
{
    public class GameButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField]
        private bool bExcutePointerExit = true;

        private int _touchId = -1;

        public bool IsPressed { get; private set; }
        public bool IsDown { get; private set; }
        public bool IsUp { get; private set; }

        private void OnEnable()
        {
            this._touchId = -1;
            this.IsDown = false;
            this.IsPressed = false;
            this.IsUp = false;
        }

        private void OnDisable()
        {
            this._touchId = -1;
            this.IsDown = false;
            this.IsPressed = false;
            this.IsUp = false;

            this.StopAllCoroutines();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (this._touchId != -1)
                return;

            // 터치 아이디 저장
#if UNITY_EDITOR
            this._touchId = 1;
#else
        this._touchId = eventData.pointerId;
#endif
            IsPressed = true;
            IsDown = true;
            IsUp = false;

            StartCoroutine(Cor_ReleaseDown());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!bExcutePointerExit)
                return;

            OnPointerUp(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (this._touchId == -1)
                return;

            // 터치 아이디 해제
            this._touchId = -1;

            IsPressed = false;
            IsDown = false;
            IsUp = true;
            StartCoroutine(Cor_ReleaseUp());
        }

        IEnumerator Cor_ReleaseDown()
        {
            yield return new WaitForEndOfFrame();

            IsDown = false;
        }

        IEnumerator Cor_ReleaseUp()
        {
            yield return new WaitForEndOfFrame();

            IsUp = false;
        }
    }
}