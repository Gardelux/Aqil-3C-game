using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  //Class
    [SerializeField] InputManager _input;
    [SerializeField] CameraManager _cameraManager;
    [SerializeField] PlayerStance _playerStance;

  //Variable Field
    [SerializeField] Transform cam;
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] Animator _animator;
    [SerializeField] CapsuleCollider _collider;

    [Header("Movement")]
    [SerializeField] float _walkSpeed;
    [SerializeField] float _rotationSmoothTime = 0.1f;
                     float _rotationSmoothVelocity;
    [SerializeField] float _speed;
    [SerializeField] float _sprintSpeed;
    [SerializeField] float _crouchSpeed;
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

    [Header("Glide")]
    [SerializeField] float _glideSpeed;
    [SerializeField] float _airDrag;
    [SerializeField] Vector3 _glideRotaSpeed;
    [SerializeField] float _minGlideRotaX;
    [SerializeField] float _maxGlideRotaX;

    [Header("Punch")]
    [SerializeField] int _combo;
    [SerializeField] float _resetComboInteval;
    [SerializeField] Transform _hitDetector;
    [SerializeField] float _hitDetectorRad;
    [SerializeField] LayerMask _hitLayer;
                     Coroutine _resetCombo;
                     bool _isPunching;
    public Action objectPunched;





    private void Awake()
    {
        _speed = _walkSpeed;
        _playerStance = PlayerStance.Stand;
        HideAndLockCursor();
    }

    private void Start()
    {
        _input.OnMoveInput += Move;
        _input.OnSprintInput += Sprint;
        _input.OnJumpInput += Jump;
        _input.OnClimbInput += StartClimb;
        _input.OnCancelClimb += CancelClimb;
        _input.OnCrouchInput += Crouch;
        _input.OnGlideInput += StartGlide;
        _input.OnCancelGlide += CancelGlide;
        _input.OnPunchInput += Punch;
        _cameraManager.OnChangePerspective += ChangePerspective;
    }

    private void OnDestroy()
    {
        _input.OnMoveInput -= Move;
        _input.OnSprintInput -= Sprint;
        _input.OnJumpInput -= Jump;
        _input.OnClimbInput -= StartClimb;
        _input.OnCancelClimb -= CancelClimb;
        _input.OnCrouchInput -= Crouch;
        _input.OnGlideInput -= StartGlide;
        _input.OnCancelGlide -= CancelGlide;
        _input.OnPunchInput -= Punch;
        _cameraManager.OnChangePerspective -= ChangePerspective;
    }

    private void Update()
    {
        CheckIsGrounded();
        CheckStep();
        Glide();
    }

    private void HideAndLockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Move(Vector2 axisDirection)
    {
        bool isPlayerStanding = _playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = _playerStance == PlayerStance.Climb;
        bool isPlayerCrouch = _playerStance == PlayerStance.Crouch;
        bool isPlayerGliding = _playerStance == PlayerStance.Glide;

        Vector3 movementDirection = Vector3.zero;

        if((isPlayerStanding || isPlayerCrouch) && !_isPunching)
        {
            switch (_cameraManager.cameraState)
            {
                case CameraState.ThirdPerson:
                    if (axisDirection.magnitude >= 0.1)
                    {
                        float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
                        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle,
                                                                  ref _rotationSmoothVelocity, _rotationSmoothTime);
                        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

                        movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                        _rigidbody.AddForce(movementDirection.normalized * _speed * 10 * Time.deltaTime);
                    }
                    break;

                case CameraState.FirstPerson:
                    transform.rotation = Quaternion.Euler(0f, cam.eulerAngles.y, 0f);
                    Vector3 verticalDirection = axisDirection.y * transform.forward;
                    Vector3 horizontalDirection = axisDirection.x * transform.right;
                    movementDirection = verticalDirection + horizontalDirection;
                    _rigidbody.AddForce(movementDirection * _speed * 10 * Time.deltaTime);
                    break;

                default:
                    break;
            }
            
          //Animasi Walk and Run
            Vector3 velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            _animator.SetFloat("velocity", velocity.magnitude * axisDirection.magnitude);
            _animator.SetFloat("velocityX", velocity.magnitude * axisDirection.x);
            _animator.SetFloat("velocityZ", velocity.magnitude * axisDirection.y);
        }

        else if(isPlayerClimbing)
        {
            Vector3 horizontal = axisDirection.x * transform.right;
            Vector3 vertical = axisDirection.y * transform.up;
            movementDirection = horizontal + vertical;
            _rigidbody.AddForce(movementDirection.normalized * _speed * 10 * Time.deltaTime);

            Vector3 velocity = new Vector3(_rigidbody.velocity.x, _rigidbody.velocity.y, 0);
            _animator.SetFloat("climbVeloX", velocity.magnitude * axisDirection.x);
            _animator.SetFloat("climbVeloY", velocity.magnitude * axisDirection.y);
        }

        else if (isPlayerGliding)
        {
            Vector3 rotationDeegree = transform.rotation.eulerAngles;
            rotationDeegree.x += _glideRotaSpeed.x * axisDirection.y * Time.deltaTime;
            rotationDeegree.x = Mathf.Clamp(rotationDeegree.x, _minGlideRotaX, _maxGlideRotaX);
            rotationDeegree.z += _glideRotaSpeed.z * axisDirection.x * Time.deltaTime;
            rotationDeegree.y += _glideRotaSpeed.y * axisDirection.x * Time.deltaTime;
            transform.rotation = Quaternion.Euler(rotationDeegree);
        }
        
    }

    private void Sprint(bool isSprint)
    {
        if (_playerStance == PlayerStance.Stand)
        {
            if (isSprint)
            {
                if (_speed < _sprintSpeed)
                {
                    _speed += _walkToSprintTransition * Time.deltaTime;

                    //biar angka lebih konstan & akurat sama dengan nilai _sprintSpeed setelah player mencapai full speed
                    if (_speed > _sprintSpeed)
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
    }

    private void Jump()
    {
        if(_isGrounded)
        {
            _animator.SetBool("isJumping", true);
            Vector3 jumpDirection = Vector3.up;
            _rigidbody.AddForce(jumpDirection * _jumpForce * 100);
        }
    }

    private void CheckIsGrounded()
    {
        _isGrounded = Physics.CheckSphere(_groundDetector.position, _detectorRadius, _groundLayer);
        _animator.SetBool("isGrounded", _isGrounded);

        if (!_isGrounded)
        {
            _animator.SetBool("isJumping", false);
        }

        else if (_isGrounded)
        {
            CancelGlide();
        }
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
            _cameraManager.SetFirstPersonClamped(true, transform.rotation.eulerAngles);
            _cameraManager.ThirdPersonFOV(70);
            _animator.SetBool("isClimbing", true);
            _collider.center = Vector3.up * 1.3f;
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
            _cameraManager.SetFirstPersonClamped(false, transform.rotation.eulerAngles);
            _cameraManager.ThirdPersonFOV(40);
            _animator.SetBool("isClimbing", false);
            _collider.center = Vector3.up * 0.9f;
        }
    }

    private void ChangePerspective()
    {
        _animator.SetTrigger("changePerspective");
    }

    private void Crouch()
    {
        if(_playerStance == PlayerStance.Stand)
        {
            _playerStance = PlayerStance.Crouch;
            _animator.SetBool("isCrouching", true);
            _speed = _crouchSpeed;
            _collider.height = 1.2f;
            _collider.center = Vector3.up * 0.62f;
        }

        else if(_playerStance == PlayerStance.Crouch)
        {
            _playerStance = PlayerStance.Stand;
            _animator.SetBool("isCrouching", false);
            _speed = _walkSpeed;
            _collider.height = 1.8f;
            _collider.center = Vector3.up * 0.9f;
        }
    }

    private void Glide()
    {
        if (_playerStance == PlayerStance.Glide)
        {
            Vector3 playerRotation = transform.rotation.eulerAngles;
            float lift = playerRotation.x;
            Vector3 upForce = transform.up * (lift + _airDrag);
            Vector3 forwardForce = transform.forward * _glideSpeed;
            Vector3 totalForce = upForce + forwardForce;
            _rigidbody.AddForce(totalForce * Time.deltaTime);

        }
    }

    private void StartGlide()
    {
        if(_playerStance != PlayerStance.Glide && !_isGrounded)
        {
            _playerStance = PlayerStance.Glide;
            Debug.Log(_playerStance);
            _animator.SetBool("isGliding", true);
            _cameraManager.SetFirstPersonClamped(true, transform.rotation.eulerAngles);
        }
    }

    private void CancelGlide()
    {
        if (_playerStance == PlayerStance.Glide)
        {
            _playerStance = PlayerStance.Stand;
            _animator.SetBool("isGliding", false);
            _cameraManager.SetFirstPersonClamped(false, transform.rotation.eulerAngles);
        }
    }

    private void Punch()
    {
        if(!_isPunching && _playerStance == PlayerStance.Stand)
        {
            _isPunching = true;
            if(_combo < 3)
            {
                _combo += 1;
            }

            else
            {
                _combo = 1;
            }

            _animator.SetInteger("combo", _combo);
            _animator.SetTrigger("punch");
        }
    }

    private void EndPunch()
    {
        _isPunching = false;
        if(_resetCombo != null)
        {
            StopCoroutine(_resetCombo);
        }
        _resetCombo = StartCoroutine(ResetCombo());
    }

    IEnumerator ResetCombo()
    {
        yield return new WaitForSeconds(_resetComboInteval);
        _combo = 0;
    }

    private void Hit()
    {
        Collider[] hitObjects = Physics.OverlapSphere(_hitDetector.position, _hitDetectorRad, _hitLayer);
        for (int i = 0; i < hitObjects.Length; i++)
        {
            if (hitObjects[i].gameObject != null)
            {
                Destroy(hitObjects[i].gameObject);
                objectPunched();
            }
            
        }
    }
}
