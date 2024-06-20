using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game
{
    public class SwipePanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Vector2 Swipe { get; private set; }
        public bool IsPressed { get; private set; }
        public bool IsDown { get; private set; }
        public bool IsUp { get; private set; }

        private Canvas _myCanvas;

        private int _touchId = -2;
        private Vector2 localTouchPos;

        private RectTransform _rttr;

        public void OnEnable()
        {
            this._touchId = -2;
            
            // 캔버스 찾아오기
            this._myCanvas = this.GetComponentInParent(typeof(Canvas)) as Canvas;
            _rttr = this.transform as RectTransform;
        }

        private void Update()
        {
            // 이미 터치 되어있는지 체크
            if (this._touchId == -2)
                return;

            var prevlocalTouchPos = localTouchPos;

            if (_touchId != -1)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (this._touchId == touch.fingerId)
                    {
                        localTouchPos = touch.position;
                        break;
                    }
                }
            }
            else
            {
                localTouchPos = Input.mousePosition;
            }
            
            // 터치 포지션 -> 터치 바운드의 로컬 포지션
            switch (this._myCanvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    {
                        localTouchPos = this._rttr.InverseTransformPoint(localTouchPos);
                    }
                    break;
                default:
                    {
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                                this._rttr, localTouchPos, this._myCanvas.worldCamera, out localTouchPos);
                    }
                    break;
            }

            this.Swipe = localTouchPos - prevlocalTouchPos;
        }

        private void OnDisable()
        {
            Swipe = Vector2.zero;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // 이미 터치 되어있는지 체크
            if (this._touchId != -2)
                return;

            // 터치 포지션 -> 터치 바운드의 로컬 포지션
            switch (this._myCanvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    {
                        localTouchPos = this.transform.InverseTransformPoint(eventData.position);
                    }
                    break;
                default:
                    {
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                                this._rttr, eventData.position, this._myCanvas.worldCamera, out localTouchPos);
                    }
                    break;
            }


            // 터치 아이디 저장
            _touchId = eventData.pointerId;
            IsPressed = true;
            IsDown = true;
            IsUp = false;
            
            this.Swipe = Vector2.zero;

            StartCoroutine(Cor_ReleaseDown());
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (this._touchId == -2)
                return;

            // 터치 아이디 해제
            this._touchId = -2;

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
            Swipe = Vector2.zero;
        }
    }
}

