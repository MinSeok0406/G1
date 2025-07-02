using System.Collections;
using System.Collections.Generic;
using FunkyCode.Buffers;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Minseok
{
    public class UI_Joystick : UI_Popup, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField]
        private GameObject _background;

        [SerializeField]
        private GameObject _cursor;

        private float _radius;
        private Vector2 _touchPos;

        void Start()
        {
            _radius = _background.GetComponent<RectTransform>().sizeDelta.y / 3;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _background.transform.position = eventData.position;
            _cursor.transform.position = eventData.position;
            _touchPos = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _cursor.transform.position = _touchPos;

            Managers.Game.JoystickDir = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 touchDir = (eventData.position - _touchPos);

            float moveDist = Mathf.Min(touchDir.magnitude, _radius);
            Vector2 moveDir = touchDir.normalized;
            Vector2 newPosition = _touchPos + moveDir * moveDist;
            _cursor.transform.position = newPosition;

            Managers.Game.JoystickDir = moveDir;
        }
    }
}