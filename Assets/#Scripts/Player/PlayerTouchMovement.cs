using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

public class PlayerTouchMovement : MonoBehaviour
{
    [SerializeField] Vector2 _joystickSize;
    [SerializeField] FloatingJoystick _joystick;
    //[SerializeField] NavMeshAgent _navMeshAgent;
    [SerializeField] Animator _animator;

    [SerializeField] Transform _fwd;

    [SerializeField] float _moveSpeed = 5f;

    Rigidbody _rigidbody;
    Finger _movementFinger;
    Vector2 _movementAmount;

    Vector3 cameraForward, cameraRight;

    public bool _canMove = true;
    bool _isMoving = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        cameraForward = _fwd.forward;
        cameraRight = _fwd.right;
        cameraForward.y = 0;
        cameraForward = cameraForward.normalized;
        cameraRight.y = 0;
        cameraRight = cameraRight.normalized;
    }

    //private void Update()
    //{
    //    if (!_canMove) return;
    //    Vector3 scaledMovement = new Vector3(_movementAmount.x, 0, _movementAmount.y) * _navMeshAgent.speed * Time.deltaTime;

    //    Vector3 moveDir = cameraForward * scaledMovement.z + cameraRight * scaledMovement.x;
    //    _navMeshAgent.transform.LookAt(_navMeshAgent.transform.position + moveDir, Vector3.up);
    //    //_navMeshAgent.Move(moveDir);
    //    _navMeshAgent.SetDestination(transform.position + moveDir);

    //    if (!_isMoving)
    //    {
    //        _rigidbody.velocity = Vector3.zero;
    //    }
    //}

    private void Update()
    {
        if (!_canMove) return;

        Vector3 moveDir = new Vector3(_movementAmount.x, 0, _movementAmount.y);
        moveDir = Camera.main.transform.TransformDirection(moveDir);
        moveDir.y = 0;
        moveDir.Normalize();

        if (moveDir != Vector3.zero)
        {

            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            _animator.transform.rotation = Quaternion.Slerp(_animator.transform.rotation, targetRotation, Time.deltaTime * 1000);


            float speed = moveDir.magnitude * _moveSpeed;
            _rigidbody.velocity = moveDir * speed;

            // Optionally, you can also control the character's animation here
            _animator.SetBool("isRunning", true);
        }
        else
        {
            _rigidbody.velocity = Vector3.zero;
            _animator.transform.rotation = _animator.transform.rotation;
            // Optionally, you can also control the character's animation here
            _animator.SetBool("isRunning", false);
        }
    }




    private void OnEnable()
    {
        ETouch.EnhancedTouchSupport.Enable();
        ETouch.Touch.onFingerDown += HandleFingerDown;
        ETouch.Touch.onFingerUp += HandleFingerUp;
        ETouch.Touch.onFingerMove += HandleFingerMove;
    }

    private void OnDisable()
    {
        ETouch.Touch.onFingerDown -= HandleFingerDown;
        ETouch.Touch.onFingerUp -= HandleFingerUp;
        ETouch.Touch.onFingerMove -= HandleFingerMove;
        ETouch.EnhancedTouchSupport.Disable();
    }

    private void HandleFingerMove(Finger finger)
    {
        if (!_canMove) return;
        if (_movementFinger != finger) return;
        Vector2 knobPosition;
        float maxMovement = _joystickSize.x / 2;
        ETouch.Touch currentTouch = finger.currentTouch;

        if (Vector2.Distance(currentTouch.screenPosition, _joystick._rectTransform.anchoredPosition) > maxMovement)
        {
            knobPosition = (currentTouch.screenPosition - _joystick._rectTransform.anchoredPosition).normalized * maxMovement;
        }
        else
        {
            knobPosition = currentTouch.screenPosition - _joystick._rectTransform.anchoredPosition;
        }
        _joystick._knob.anchoredPosition = knobPosition;

        Vector2 moveDir = new Vector2(_fwd.forward.x, _fwd.forward.z);
        _movementAmount = (moveDir + knobPosition) / maxMovement;
    }

    private void HandleFingerUp(Finger finger)
    {
        if (_movementFinger != finger) return;
        _isMoving = false;
        _movementFinger = null;
        _movementAmount = Vector2.zero;
        _joystick._knob.anchoredPosition = Vector2.zero;
        _joystick.gameObject.SetActive(false);
        _animator.SetBool("isRunning", false);
    }

    private void HandleFingerDown(Finger finger)
    {
        if (!_canMove) return;
        if (_movementFinger != null) return;
        _isMoving = true;
        _movementFinger = finger;
        _movementAmount = Vector2.zero;
        _joystick.gameObject.SetActive(true);
        _joystick._rectTransform.sizeDelta = _joystickSize;
        _joystick._rectTransform.anchoredPosition = ClampStartPosition(finger.screenPosition);
        _animator.SetBool("isRunning", true);
    }

    private Vector2 ClampStartPosition(Vector2 startPosition)
    {
        if (startPosition.x < _joystickSize.x / 2)
        {
            startPosition.x = _joystickSize.x / 2;
        }
        else if (startPosition.x > Screen.width - (_joystickSize.x / 2))
        {
            startPosition.x = Screen.width - (_joystickSize.x / 2);
        }

        if (startPosition.y < _joystickSize.y / 2)
        {
            startPosition.y = _joystickSize.y / 2;
        }
        else if (startPosition.y > Screen.height - (_joystickSize.y / 2))
        {
            startPosition.y = Screen.height - (_joystickSize.y / 2);
        }
        return startPosition;
    }
}
