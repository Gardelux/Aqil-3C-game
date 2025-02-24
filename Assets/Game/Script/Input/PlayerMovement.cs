using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  //Class
    [SerializeField] InputManager _input;
    PlayerStance _playerStance;

  //Variable Field
    [SerializeField] Transform cam;
    [SerializeField] Rigidbody _rigidbody;

    [Header("Movement")]
    [SerializeField] float _walkSpeed;
    [SerializeField] float _rotationSmoothTime = 0.1f;
                     float _rotationSmoothVelocity;
    [SerializeField] float _speed;
    [SerializeField] float _sprintSpeed;
    [SerializeField] float _walkToSprintTransition;
    [SerializeField] float _sprintToWalkTransition;

    [Header("Jump")]
    [SerializeField] float _jumpForce;
                     bool _isGrounded;
    [SerializeField] float _detectorRadius;
    [SerializeField] Transform _groundDetector;
    [SerializeField] LayerMask _groundLayer;

    [Header("Stairs")]
    [SerializeField] float _stepCheckerDistance;
    [SerializeField] float _stepForce;
    [SerializeField] Vector3 _upperStepOffset;

    [Header("Climb")]
    [SerializeField] float _climbSpeed;
    [SerializeField] float _climbCheckDistance;
    [SerializeField] Vector3 _climbOffset;
    [SerializeField] Transform _climbDetector;
    [SerializeField] LayerMask _climbableLayer;




    private void Awake()
    {
        _speed = _walkSpeed;
        _playerStance = PlayerStance.Stand;
    }

    private void Start()
    {
        _input.OnMoveInput += Move;
        _input.OnSprintInput += Sprint;
        _input.OnJumpInput += Jump;
        _input.OnClimbInput += StartClimb;
        _input.OnCancelClimb += CancelClimb;
    }

    private void OnDestroy()
    {
        _input.OnMoveInput -= Move;
        _input.OnSprintInput -= Sprint;
        _input.OnJumpInput -= Jump;
        _input.OnClimbInput -= StartClimb;
        _input.OnCancelClimb -= CancelClimb;
    }

    private void Update()
    {
        CheckIsGrounded();
        CheckStep();
    }

    private void Move(Vector2 axisDirection)
    {
        bool isPlayerStanding = _playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = _playerStance == PlayerStance.Climb;

        Vector3 movementDirection = Vector3.zero;

        if(isPlayerStanding)
        {
            if (axisDirection.magnitude >= 0.1)
            {
                float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle,
                                                          ref _rotationSmoothVelocity, _rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

                movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                _rigidbody.AddForce(movementDirection.normalized * _speed * 10 * Time.deltaTime);
            }
        }

        else if(isPlayerClimbing)
        {
            Vector3 horizontal = axisDirection.x * transform.right;
            Vector3 vertical = axisDirection.y * transform.up;
            movementDirection = horizontal + vertical;
            _rigidbody.AddForce(movementDirection.normalized * _speed * 10 * Time.deltaTime);
        }
        
    }

    private void Sprint(bool isSprint)
    {
        if (isSprint)
        {
            if(_speed < _sprintSpeed)
            {
                _speed += _walkToSprintTransition * Time.deltaTime;

              //biar angka lebih konstan & akurat sama dengan nilai _sprintSpeed setelah player mencapai full speed
                if(_speed > _sprintSpeed)
                {
                    _speed = _sprintSpeed;
                }
            }
        }

        else
        {
            if (_speed > _walkSpeed)
            {
                _speed -= _sprintToWalkTransition * Time.deltaTime;

              //biar angka lebih konstan & akurat sama dengan nilai _walkSpeed setelah player berhenti sprint
                if (_speed < _walkSpeed)
                {
                    _speed = _walkSpeed;
                }
            }
        }
    }

    private void Jump()
    {
        if(_isGrounded)
        {
            Vector3 jumpDirection = Vector3.up;
            _rigidbody.AddForce(jumpDirection * _jumpForce * 100);
        }
    }

    private void CheckIsGrounded()
    {
        _isGrounded = Physics.CheckSphere(_groundDetector.position, _detectorRadius, _groundLayer);
    }

    private void CheckStep()
    {
        bool isHitLowerStep = Physics.Raycast(_groundDetector.position, transform.forward, _stepCheckerDistance);
        bool isHitUpperStep = Physics.Raycast(_groundDetector.position + _upperStepOffset, transform.forward, _stepCheckerDistance);

        if(isHitLowerStep && !isHitUpperStep)
        {
            _rigidbody.AddForce(0, _stepForce * Time.deltaTime, 0);
        }
    }

    private void StartClimb()
    {
        bool isInFrontofClimbingWall = Physics.Raycast(_climbDetector.position, transform.forward, out RaycastHit hit,
                                                       _climbCheckDistance, _climbableLayer);

        bool isNotClimbing = _playerStance != PlayerStance.Climb;

        if(isInFrontofClimbingWall && _isGrounded && isNotClimbing)
        {
            Vector3 offset = (transform.forward * _climbOffset.z) + (Vector3.up * _climbOffset.y);
            transform.position = hit.point - offset;

          //biar rotasi object Player menghadap dinding yang di-Climb
            if(transform.eulerAngles.y > 45 && transform.eulerAngles.y < 135)
            {
                transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            }

            else if (transform.eulerAngles.y > 135f && transform.eulerAngles.y < 225)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }

            else if (transform.eulerAngles.y > 225 && transform.eulerAngles.y < 315)
            {
                transform.rotation = Quaternion.Euler(0f, 270f, 0f);
            }

            else
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }

            _playerStance = PlayerStance.Climb;
            _rigidbody.useGravity = false;
            _speed = _climbSpeed;
        }
    }

    private void CancelClimb()
    {
        if(_playerStance == PlayerStance.Climb)
        {
            _playerStance = PlayerStance.Stand;
            _rigidbody.useGravity = true;
            transform.position -= transform.forward * 0.75f;
            _speed = _walkSpeed;
        }
    }
}
