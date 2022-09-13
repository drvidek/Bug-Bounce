using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public enum PlayerState { walking, rising, falling, floating, slamming }
    [SerializeField] private PlayerState _state = PlayerState.walking;
    [SerializeField] private Joystick _stickMove;
    [SerializeField] private ActionButton _buttonJump;
    [SerializeField] private ActionButton _buttonSlam;
    [SerializeField] private CharacterController _charCon;
    [SerializeField] private float _maxSpd = 5f;
    [SerializeField] private float _jumpSpd = 20f, _grv = 40f, _hoverSpd = 5f, _hoverTimeMax = 1f, _slamDelayMax = 0.5f, _slamDownSpd = 15f, _slamUpSpd = 2f, _bufferJumpMax = 0.1f;
    private Vector3 moveDir;
    private float _hoverTime, _slamDelay, _bufferJump;
    private bool _hoverReady;

    private void Start()
    {
        if (_charCon == null)
            _charCon = GetComponent<CharacterController>();
        NextState();
    }

    void NextState()
    {
        switch (_state)
        {
            case PlayerState.walking:
                StartCoroutine("WalkState");
                break;
            case PlayerState.rising:
                StartCoroutine("RiseState");
                break;
            case PlayerState.falling:
                StartCoroutine("FallState");
                break;
            case PlayerState.slamming:
                StartCoroutine("SlamState");
                break;
            case PlayerState.floating:
                StartCoroutine("HoverState");
                break;
            default:
                break;
        }
    }

    #region Inputs
    Vector3 GetInputDirection()
    {
        Vector3 dir = new Vector3();
        dir.x = _stickMove.Horizontal;
        if (dir.x == 0)
            dir.x = Input.GetAxis("Horizontal");
        dir.z = _stickMove.Vertical;
        if (dir.z == 0)
            dir.z = Input.GetAxis("Vertical");
        return dir;
    }

    bool GetInputJump()
    {
        if (_buttonJump != null && _buttonJump.Pressed)
        {
            _buttonJump.Pressed = false;
            return true;
        }
        return Input.GetButtonDown("Jump");
    }

    bool GetInputSlam()
    {
        if (_buttonSlam != null && _buttonSlam.Pressed)
        {
            _buttonSlam.Pressed = false;
            return true;
        }
        return Input.GetButtonDown("Slam");
    }

    bool GetInputHover()
    {
        if (_buttonJump != null && _buttonJump.Held)
        {
            return true;
        }
        return Input.GetButton("Jump");
    }
    #endregion

    IEnumerator WalkState()
    {
        _hoverReady = true;
        while (_state == PlayerState.walking)
        {
            Vector3 inputDir = GetInputDirection();
            bool jump = GetInputJump();

            moveDir = new Vector3(inputDir.x, moveDir.y, inputDir.z);
            moveDir.x *= _maxSpd;
            moveDir.z *= _maxSpd;
            moveDir.y = Mathf.Clamp(moveDir.y, -2f, float.PositiveInfinity);
            if (jump || _bufferJump > 0)
            {
                moveDir.y = _jumpSpd;
                _state = PlayerState.rising;
            }

            moveDir.y -= _grv * Time.deltaTime;
            moveDir = transform.TransformDirection(moveDir);
            _charCon.Move(moveDir * Time.deltaTime);

            if (!_charCon.isGrounded && _state != PlayerState.rising)
                _state = PlayerState.falling;

            yield return null;

        }
        NextState();
    }

    IEnumerator RiseState()
    {
        while (_state == PlayerState.rising)
        {
            Vector3 inputDir = GetInputDirection();

            moveDir = new Vector3(inputDir.x, moveDir.y, inputDir.z);
            moveDir.x *= _maxSpd;
            moveDir.z *= _maxSpd;

            moveDir.y -= _grv * Time.deltaTime;
            moveDir = transform.TransformDirection(moveDir);
            _charCon.Move(moveDir * Time.deltaTime);

            if (moveDir.y < 0)
                _state = PlayerState.falling;

            if (_charCon.isGrounded)
                _state = PlayerState.walking;

            if (GetInputSlam())
                _state = PlayerState.slamming;

            yield return null;

        }
        NextState();
    }

    IEnumerator FallState()
    {
        while (_state == PlayerState.falling)
        {
            Vector3 inputDir = GetInputDirection();
            bool jump = GetInputJump();

            moveDir = new Vector3(inputDir.x, moveDir.y, inputDir.z);
            moveDir.x *= _maxSpd;
            moveDir.z *= _maxSpd;


            if (jump && _bufferJump == 0)
            {
                _bufferJump = _bufferJumpMax;
            }
            else
                _bufferJump = Mathf.MoveTowards(_bufferJump, 0, Time.deltaTime);


            moveDir.y -= _grv * Time.deltaTime;

            moveDir = transform.TransformDirection(moveDir);

            _charCon.Move(moveDir * Time.deltaTime);

            if (_charCon.isGrounded)
                _state = PlayerState.walking;

            if (GetInputHover() && _hoverReady && moveDir.y < -1)
            {
                _state = PlayerState.floating;
            }

            if (GetInputSlam())
                _state = PlayerState.slamming;

            yield return null;

        }
        NextState();
    }

    IEnumerator HoverState()
    {
        _hoverReady = false;
        _hoverTime = _hoverTimeMax;
        moveDir.y = Mathf.Clamp(moveDir.y, -2f, float.PositiveInfinity);
        _buttonJump.Pressed = false;
        while (_state == PlayerState.floating)
        {
            Vector3 inputDir = GetInputDirection();

            moveDir = new Vector3(inputDir.x, moveDir.y, inputDir.z);
            moveDir.x *= _maxSpd;
            moveDir.z *= _maxSpd;
            moveDir.y += _hoverSpd * Time.deltaTime;

            moveDir = transform.TransformDirection(moveDir);
            _charCon.Move(moveDir * Time.deltaTime);

            _hoverTime = Mathf.MoveTowards(_hoverTime, 0, Time.deltaTime);

            if (_hoverTime == 0 || !GetInputHover())
            {
                _state = PlayerState.rising;
            }

            if (GetInputSlam())
                _state = PlayerState.slamming;

            yield return null;
        }
        NextState();
    }

    IEnumerator SlamState()
    {
        moveDir.y = 0;
        _slamDelay = _slamDelayMax;
        _bufferJump = 0;
        while (_state == PlayerState.slamming)
        {
            moveDir.x = Mathf.MoveTowards(moveDir.x, 0, _maxSpd / _slamDelayMax * Time.deltaTime);
            moveDir.z = Mathf.MoveTowards(moveDir.z, 0, _maxSpd / _slamDelayMax * Time.deltaTime);
            if (_slamDelay > 0)
            {
                _slamDelay = Mathf.MoveTowards(_slamDelay, 0, Time.deltaTime);
                moveDir.y += _slamUpSpd * Time.deltaTime;
            }
            else
            {
                moveDir.y = -_slamDownSpd;
            }
            moveDir = transform.TransformDirection(moveDir);
            _charCon.Move(moveDir * Time.deltaTime);
            if (_charCon.isGrounded)
            {
                _state = PlayerState.walking;
            }
            yield return null;

        }
        NextState();
    }
}
