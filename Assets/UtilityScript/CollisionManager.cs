using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CollisionManager : Singleton<CollisionManager>
{
    //The touch eventType
    public enum TOUCH_EVENT_TYPE
    {
        PRESS,
        RELEASE,
        MOVE,
        NONE
    };    

    //The touch information declartion
    public struct TouchInformation
    {
        public TOUCH_EVENT_TYPE _Type;
        public Vector2 _TouchPosition;
        public Vector2 _TouchDelta;
        public void Clear()
        {
            _Type = TOUCH_EVENT_TYPE.NONE;
            _TouchPosition = Vector2.zero;
            _TouchDelta = Vector2.zero;
        }
    };

    // event 為關鍵字，可提供delegate成員無法使用=及呼叫invok，可確保不會被外部使用，造成bug.
    public delegate void TouchEventHandler(TouchInformation touchInfo, TouchInformation touchInfo1,
        bool injectGUI, GameObject hitGameObject);
    public event TouchEventHandler _pressEventHandler = null;
    public event TouchEventHandler _moveEventHandler = null;
    public event TouchEventHandler _releaseEventHandler = null;

    private TouchInformation _touchInformation;
    private TouchInformation _touchInformation1;
    private bool _firstTouch = false;

    private void SetupPerTouchInformation(ref Touch theTouch, ref TouchInformation touchInformation)
    {
        touchInformation._TouchDelta -= theTouch.deltaPosition;
        touchInformation._TouchPosition = theTouch.position;
        TouchPhase TheTouchPhase = theTouch.phase;
        if (TheTouchPhase == TouchPhase.Began)
        {
            touchInformation._Type = TOUCH_EVENT_TYPE.PRESS;
            _firstTouch = true;
        }
        else if (TheTouchPhase == TouchPhase.Ended)
        {
            touchInformation._Type = TOUCH_EVENT_TYPE.RELEASE;
            _firstTouch = false;
        }
        else if (TheTouchPhase == TouchPhase.Moved)
            touchInformation._Type = TOUCH_EVENT_TYPE.MOVE;
    }


    private void SetupTouchInformation()
    {
        _touchInformation.Clear();
        _touchInformation1.Clear();
#if UNITY_EDITOR || UNITY_STANDALONE
        _touchInformation._TouchPosition = Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            if (!_firstTouch)
            {
                _firstTouch = true;
                _touchInformation._Type = TOUCH_EVENT_TYPE.PRESS;
            }
            else
            {
                _touchInformation._TouchDelta.x -= Input.GetAxis("Mouse X");
                _touchInformation._TouchDelta.y -= Input.GetAxis("Mouse Y");
                if ((Mathf.Abs(_touchInformation._TouchDelta.x) > 0.0f) ||
                    (Mathf.Abs(_touchInformation._TouchDelta.y) > 0.0f))
                    _touchInformation._Type = TOUCH_EVENT_TYPE.MOVE;
                else
                    _touchInformation._Type = TOUCH_EVENT_TYPE.NONE;
            }
        }
        else
        {
            if (_firstTouch)
            {
                _firstTouch = false;
                _touchInformation._Type = TOUCH_EVENT_TYPE.RELEASE;
            }
            else
                _touchInformation._Type = TOUCH_EVENT_TYPE.NONE;
        }
#else
        if ((Input.touchCount <= 0))
            return;
        Touch theTouch = Input.GetTouch(0);
        SetupPerTouchInformation(ref theTouch, ref _touchInformation);
        if (Input.touchCount >= 2)
        {
            theTouch = Input.GetTouch(1);
            SetupPerTouchInformation(ref theTouch, ref _touchInformation1);
        }
#endif
    }

    private void Update()
    {        
        SetupTouchInformation();
        if (_touchInformation._Type != TOUCH_EVENT_TYPE.NONE)
        {
            GameObject hitGameObject = null;
            bool injectGUI = false;
            if (EventSystem.current != null)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                injectGUI = EventSystem.current.IsPointerOverGameObject();
#else           
                injectGUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
#endif
            }
            if(injectGUI == false)
            {
                if (Camera.main)
                {
                    Ray testRay = Camera.main.ScreenPointToRay(
                    new Vector3(_touchInformation._TouchPosition.x, _touchInformation._TouchPosition.y, 0.0f));
                    RaycastHit[] theHits = Physics.RaycastAll(testRay);
                    int hitCounts = theHits.Length;
                    if (hitCounts > 0)
                    {
                        hitGameObject = theHits[0].collider.gameObject;
                    }
                }
            }

            switch (_touchInformation._Type)
            {
                case TOUCH_EVENT_TYPE.PRESS:
                    if (_pressEventHandler != null)
                        _pressEventHandler(_touchInformation, _touchInformation1, injectGUI, hitGameObject);
                    break;
                case TOUCH_EVENT_TYPE.RELEASE:
                    if (_releaseEventHandler != null)
                        _releaseEventHandler(_touchInformation, _touchInformation1, injectGUI, hitGameObject);
                    break;
                case TOUCH_EVENT_TYPE.MOVE:
                    if (_moveEventHandler != null)
                        _moveEventHandler(_touchInformation, _touchInformation1, injectGUI, hitGameObject);
                    break;
            }
        }
    }
}
