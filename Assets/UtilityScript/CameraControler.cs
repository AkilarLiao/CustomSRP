using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class CameraControler : MonoBehaviour
{
    public float KeyboardMoveSpeed { set { _MoveSpeed = value; } }
    public float DragMoveSpeed { set { _Speed = value; } }
    public float AccumulateMoveRatio { set { _AccumulateMoveRatio = value; } }
    [SerializeField]
    private float _DestRotationScaleValue = 15.0f;
    [SerializeField]
    private float _RotateSpeed = 0.8f;
    //keyboard move speed
    [SerializeField]    
    private float _MoveSpeed = 10.0f;
    //drag move speed
    [SerializeField]
    private float _Speed = 0.1f;
    [SerializeField]
    private float _AccumulateMoveRatio = 1.0f;
    [SerializeField]
    private bool _useBornInfo = false;
    [SerializeField]
    private Vector3 _BornPosition = Vector3.zero;
    [SerializeField]
    private Vector3 _Rotation = Vector3.zero;
    //=========================for touche movement=========================    
    private Transform _SelfTransform = null;
    //The touch press position
    private Vector2 _TouchPressPosition = Vector2.zero;
    //The delegate declartion
    private delegate void TouchEventHandler();
    private float _PressTime = 0.0f;
    private Vector2 _DeltaMovement = Vector2.zero;
    //=========================for mouse rotate=========================    
    private Vector3 _oldMousePosition;
    private bool _mouseRightButtonPress;
    private float _accYawAngle = 0.0f;
    private float _accPitchAngle = 0.0f;
    //=========================for key movement=========================
    private enum MoveState
    {
        MoveState_Forward,
        MoveState_Back,
        MoveState_Left,
        MoveState_Right,
        MoveState_Max
    }    
    private bool[] _theMoveStates = new bool[(int)MoveState.MoveState_Max];

    private Vector2 _oldTouchPosition1;
    private Vector2 _oldTouchPosition2;

    //roate:        frist touch press. sceond move.
    //drage mov:    first touch move, sceond none.
    //room in out:  first touch move, second move.

    //MonoBehaviour Event Implement
    // Use this for initialization
    private void Start()
    {
        _SelfTransform = transform;
        if (_useBornInfo)
        {
            _SelfTransform.position = _BornPosition;
            _SelfTransform.localEulerAngles = _Rotation;
        }

        if (CollisionManager.Instance != null)
        {
            CollisionManager.Instance._pressEventHandler += ProcessTouchPress;
            CollisionManager.Instance._moveEventHandler += ProcessTouchMove;
            CollisionManager.Instance._releaseEventHandler += ProcessTouchRelease;
        }
    }

    private void OnDestroy()
    {
        if ((!CollisionManager.IsApplicationIsQuitting()) && (CollisionManager.Instance != null))
        {
            CollisionManager.Instance._releaseEventHandler -= ProcessTouchRelease;
            CollisionManager.Instance._moveEventHandler -= ProcessTouchMove;
            CollisionManager.Instance._pressEventHandler -= ProcessTouchPress;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        ProcessRotate();
        ProcessKeyboardMovement();
        if (_Pressed)
            _PressTime += Time.deltaTime;

        if (_DeltaMovement.sqrMagnitude <= c_fMinDeltaSquareMagnitude)
        {
            _DeltaMovement = Vector2.zero;
            return;
        }

        float fDestRatio = Mathf.Min(c_fDeltaMoveScaleRatio * Time.deltaTime, 1.0f);
        Vector2 DestVelocity = _DeltaMovement * fDestRatio;
        if ((Mathf.Abs(DestVelocity.x) > 0.0f) ||
            (Mathf.Abs(DestVelocity.y) > 0.0f))
        {
            Vector3 FaceDirection, RightDirection;
            GetMoveDirection(out FaceDirection, out RightDirection);
            Vector3 FixVelocity = Vector2.zero;
            FixVelocity += DestVelocity.y * FaceDirection;
            FixVelocity += DestVelocity.x * RightDirection;
            if (FixVelocity.magnitude <= 100.0f)
                _SelfTransform.position -= FixVelocity;
            else
                Debug.Log(string.Format("Happen Huge Distance: {0}", FixVelocity.magnitude));
        }
        _DeltaMovement -= DestVelocity;
    }
    
    void ProcessRotate()
    {
        if (Input.GetMouseButtonDown(1))
        {
//#if UNITY_EDITOR || UNITY_STANDALONE
            //_mouseRightButtonPress = true;
            //_oldMousePosition = Input.mousePosition;
//#else
            //Touch theTouch = Input.GetTouch(0);
            //if (theTouch.phase == TouchPhase.Began)
            {
                _mouseRightButtonPress = true;
                _oldMousePosition = Input.mousePosition;
            }
//#endif
        }
        else if (Input.GetMouseButtonUp(1))
        {
            _mouseRightButtonPress = false;
        }
        if (_mouseRightButtonPress)
        {
            _mouseRightButtonPress = true;
            Vector3 Delta = Input.mousePosition - _oldMousePosition;
            Vector3.Magnitude(Delta);            
            _oldMousePosition = Input.mousePosition;
            _accYawAngle += _RotateSpeed * Delta.x;
            _accPitchAngle += _RotateSpeed * Delta.y;
        }
        float fScaleValue = Time.deltaTime * _DestRotationScaleValue;
        fScaleValue = Mathf.Min(fScaleValue, 1.0f);
        Vector3 eulerAngles = _SelfTransform.eulerAngles;
        if (Mathf.Abs(_accYawAngle) > 0.0f)
        {
            float fRotAngle = _accYawAngle * fScaleValue;
            _accYawAngle -= fRotAngle;
            eulerAngles.y += fRotAngle;
            _SelfTransform.eulerAngles = eulerAngles;            
        }

        if (Mathf.Abs(_accPitchAngle) > 0.0f)
        {
            float fRotAngle = _accPitchAngle * fScaleValue;
            _accPitchAngle -= fRotAngle;
            eulerAngles.x -= fRotAngle;
            _SelfTransform.eulerAngles = eulerAngles;
        }
    }
    private void ProcessKeyState()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            _theMoveStates[(int)MoveState.MoveState_Forward] = true;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            _theMoveStates[(int)MoveState.MoveState_Left] = true;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            _theMoveStates[(int)MoveState.MoveState_Back] = true;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            _theMoveStates[(int)MoveState.MoveState_Right] = true;
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            _theMoveStates[(int)MoveState.MoveState_Forward] = false;
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            _theMoveStates[(int)MoveState.MoveState_Left] = false;
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            _theMoveStates[(int)MoveState.MoveState_Back] = false;
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            _theMoveStates[(int)MoveState.MoveState_Right] = false;
        }
    }
    private void ProcessKeyboardMovement()
    {
        ProcessKeyState();
        Vector3 moveMent = Vector3.zero;
        for (int index = 0; index < _theMoveStates.Length; ++index)
        {
            if (!_theMoveStates[index])
                continue;
            switch ((MoveState)index)
            {
                case MoveState.MoveState_Forward:
                    moveMent += _MoveSpeed * Vector3.forward;
                    break;
                case MoveState.MoveState_Left:
                    moveMent += _MoveSpeed * Vector3.left;
                    break;
                case MoveState.MoveState_Right:
                    moveMent += _MoveSpeed * Vector3.right;
                    break;
                case MoveState.MoveState_Back:
                    moveMent += _MoveSpeed * Vector3.back;
                    break;
            }
        }
        _SelfTransform.Translate(moveMent * Time.deltaTime);
    }

    private const float c_fMoveCheckRange = 1.0f;
    private const float c_fReleaseMoveTime = 0.1f;
    private const float c_fDeltaMoveScaleRatio = 5.0f;
    private const float c_fMinDeltaSquareMagnitude = 0.1f * 0.1f;
    private const float c_fMaxPressTime = 0.5f;

    private bool _Pressed = false;

    //The process touch founcitons
    private void ProcessTouchPress(CollisionManager.TouchInformation touchInfo, CollisionManager.TouchInformation touchInfo1,
        bool injectGUI, GameObject hitGameObject)
    {
        if (injectGUI == true)
            return;
        ProcessPressMovement(touchInfo);
        _PressTime = 0.0f;
        _TouchPressPosition = touchInfo._TouchPosition;
        _DeltaMovement = Vector2.zero;
        _Pressed = true;
        //it's a rotate event

        _oldTouchPosition1 = touchInfo._TouchPosition;
        _oldTouchPosition2 = touchInfo1._TouchPosition;

        if (touchInfo1._Type == CollisionManager.TOUCH_EVENT_TYPE.MOVE)
        {
        }
    }    

    private void ProcessTouchRelease(CollisionManager.TouchInformation touchInfo, CollisionManager.TouchInformation touchInfo1,
        bool injectGUI, GameObject hitGameObject)
    {
        if (_Pressed)
        {
            Vector2 DeletaRange = _TouchPressPosition - touchInfo._TouchPosition;
            if ((_PressTime < c_fMaxPressTime) && (DeletaRange.sqrMagnitude >= c_fMoveCheckRange * c_fMoveCheckRange))
                _DeltaMovement += (DeletaRange / _PressTime) * c_fReleaseMoveTime * _AccumulateMoveRatio;
        }
        _Pressed = false;
        _PressTime = 0.0f;
    }

    private void ProcessTouchMove(CollisionManager.TouchInformation touchInfo, CollisionManager.TouchInformation touchInfo1,
        bool injectGUI, GameObject hitGameObject)
    {
        if (_Pressed == false)
            return;        
        ProcessPressMovement(touchInfo);

        /*
        //it's a room in out event
        if (touchInfo1._Type == CollisionManager.TOUCH_EVENT_TYPE.MOVE)
        {
            //if (IsEnlarge(_oldTouchPosition1, _oldTouchPosition2, touchInfo._TouchPosition, touchInfo1._TouchPosition))
            //{
            //}
            float oldSQRDistance = (_oldTouchPosition2 - _oldTouchPosition1).magnitude;
            float newSQRDistance = (touchInfo1._TouchPosition - touchInfo._TouchPosition).magnitude;
            float scaleDelta = newSQRDistance - oldSQRDistance;


            //Vector3 FaceDirection, RightDirection;
            //GetMoveDirection(out FaceDirection, out RightDirection);
            //Vector3 DestVelocity = Vector3.zero;
            //DestVelocity += touchInfo._TouchDelta.y * _Speed * FaceDirection;
            //DestVelocity += touchInfo._TouchDelta.x * _Speed * RightDirection;
            //_SelfTransform.position -= DestVelocity;

            _SelfTransform.position += _SelfTransform.forward * scaleDelta * 0.1f;

            _oldTouchPosition1 = touchInfo._TouchPosition;
            _oldTouchPosition2 = touchInfo1._TouchPosition;
        }*/
    }

    private void GetMoveDirection(out Vector3 FaceDirection, out Vector3 RightDirection)
    {
        FaceDirection = _SelfTransform.forward;
        FaceDirection.y = 0.0f;
        FaceDirection = FaceDirection.normalized;
        RightDirection = Quaternion.AngleAxis(90.0f, Vector3.up) * FaceDirection;
    }

    //函数返回真为放大，返回假为缩小
    private bool IsEnlarge(Vector2 oP1 , Vector2 oP2, Vector2 nP1, Vector2 nP2)
    {       
        //函数传入上一次触摸两点的位置与本次触摸两点的位置计算出用户的手势
        var leng1 =Mathf.Sqrt((oP1.x-oP2.x)*(oP1.x-oP2.x)+(oP1.y-oP2.y)*(oP1.y-oP2.y));
        var leng2 =Mathf.Sqrt((nP1.x-nP2.x)*(nP1.x-nP2.x)+(nP1.y-nP2.y)*(nP1.y-nP2.y));
        if(leng1<leng2)
        {        
            //放大手势
             return true;
        }
        else
        {   
            //缩小手势
            return false;
        }
    }

    private void ProcessPressMovement(CollisionManager.TouchInformation touchInfo)
    {
        Vector3 FaceDirection, RightDirection;
        GetMoveDirection(out FaceDirection, out RightDirection);
        Vector3 DestVelocity = Vector3.zero;
        DestVelocity += touchInfo._TouchDelta.y * _Speed * FaceDirection;
        DestVelocity += touchInfo._TouchDelta.x * _Speed * RightDirection;
        _SelfTransform.position -= DestVelocity;
    }
}