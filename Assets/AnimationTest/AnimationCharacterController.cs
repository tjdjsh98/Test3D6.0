using Fusion;
using Fusion.Addons.KCC;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class AnimationCharacterController : PrototypeCharacterController
{
    // Animation
    Animator _animator;
    AnimatorHelper _animatorHelper;

    // AnimationState
    [Networked] protected NetworkBool _isPlayTurnAnimation { get; set; }
    [Networked] protected NetworkBool _isPlayAttackAnimation { get; set; }
    [Networked] protected NetworkBool _isPlayHitAnimation { get; set; }
    protected Coroutine _damagedAnimationCoroutine;
    protected Coroutine _turnAnimationCoroutine;
    protected Coroutine _attackAnimationCoroutine;

    public Action AttackAnimationStarted { get; set; }
    public Action AttackAnimationEnded { get; set; }
    public Action AttackStarted { get; set; }
    public Action AttackEnded { get; set; }

    // 행동제어
    public bool IsEnableAnimationMove { get; private set; } = true;
    public bool IsEnableAnimationRotate { get; private set; } = true;

    [SerializeField] Transform _leftHandPos;
    [SerializeField] Transform _rightHandPos;

    AnimationClip _preAnimationClip;

    protected override void Awake()
    {
        base.Awake();
        _animator = _model.GetComponent<Animator>();
        _animatorHelper = _model.GetComponent<AnimatorHelper>();
        _animatorHelper.AnimatorMoved += OnAnimatorMoved;
        _character.Damaged  += OnDamaged;
    }

    void OnAnimatorMoved()
    {
        if (_animator != null && HasInputAuthority)
        {
            PlayerInputData data = _playerInputHandler.AccumulatedInput;
            data.animatorDeltaAngle += _animator.deltaRotation.eulerAngles;
            data.animatorVelocity = _animator.deltaPosition / Time.deltaTime;

            _playerInputHandler.AccumulatedInput = data;
        }
    }

    public override void Render()
    {
        base.Render();
        if(Input.GetKeyDown(KeyCode.F1))
        {
            Application.targetFrameRate = 10;
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Application.targetFrameRate = 60;
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Application.targetFrameRate = 100000;
        }
        PlayerInputData data = _playerInputHandler.AccumulatedInput;
        data.IsEnableInputMove = IsEnableInputMove;
        data.IsEnableInputRotate = IsEnableInputRotate;
        data.IsEnableAnimationMove = IsEnableAnimationMove;
        data.IsEnableAnimationRotate = IsEnableAnimationRotate;
        _playerInputHandler.AccumulatedInput = data;
    }

    void OnDamaged(DamageInfo info)
    {
        if (info.knockbackPower <= 0) return;

        _isPlayHitAnimation = true;
        IsEnableInputMove = false;
        IsEnableInputRotate = false;

        if (_damagedAnimationCoroutine != null)
            StopCoroutine(_damagedAnimationCoroutine);
        if(_attackAnimationCoroutine != null)
            StopCoroutine(_attackAnimationCoroutine);
        if(_turnAnimationCoroutine != null)
            StopCoroutine(_turnAnimationCoroutine);

        _character.AnimatorSetTrigget("Hit");
        _damagedAnimationCoroutine = StartCoroutine(Utils.WaitAniationAndPlayCoroutine(_animator, "Hit", () =>
        {
            _isPlayHitAnimation = false;
            _isPlayAttackAnimation = false;
            _isPlayTurnAnimation = false;

            IsEnableInputRotate = true;
            IsEnableInputMove = true;
            IsEnableAnimationMove = true;
            IsEnableAnimationRotate = true;
            _model.transform.localPosition = Vector3.zero;
            _model.transform.localRotation = Quaternion.identity;
        }));
    }
    protected override void ProcessInputData()
    {
        base.ProcessInputData();

        if(!_animator.GetCurrentAnimatorClipInfo(0)[0].clip.Equals(_preAnimationClip))
        {
            _preAnimationClip = _animator.GetCurrentAnimatorClipInfo(0)[0].clip;
            _model.transform.localRotation = Quaternion.identity;
        }
    }
    protected override void ProcessMove()
    {
        float speed = 0;
        _animator.SetBool("IsGrounded", _character.IsGrounded);
        

        Vector3 totalVelocity = Vector3.zero;

        if (IsEnableAnimationMove)
        {
            totalVelocity += _currentPlayerInputData.animatorVelocity;
            if (!_character.IsEnableMoveYAxis)
            {
                // Y축으로 이동을 전제하지 않은 애니메이션을 경우
                // Collider랑 모델이랑 일치되지 않아
                // Y축은 모델에서 처리합니다.
                _model.transform.localPosition = Vector3.up * totalVelocity.y * Runner.DeltaTime;
                totalVelocity.y = 0;
            }
        }
        if (_moveDirection != Vector3.zero)
            speed = 1;


        // Move
        if (IsEnableInputMove)
        {
            if (!IsHoldRope)
            {
                // Jump
                if (_character.IsGrounded && _currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.Jump))
                {
                    _character.Jump(Vector3.up, 10);
                    if(Runner.IsForward)
                        _character.AnimatorSetTrigget("Jump");
                }
                if (_currentPlayerInputData.buttons.IsSet(InputButton.Run))
                {
                    _character.IsUseStamina = true;
                    speed = 2;
                }
                else
                {
                    _character.IsUseStamina = false;
                }
                if(!_character.IsGrounded)
                {
                    totalVelocity = _moveDirection;
                }
            }
            // RopeMove
            else
            {
                if (_currentPlayerInputData.movementInput.y > 0)
                {
                    _moveDirection = (_holdRope.StartPos.position - _holdRope.EndPos.position).normalized;
                }
                else if (_currentPlayerInputData.movementInput.y < 0)
                {
                    _moveDirection = (_holdRope.EndPos.position - _holdRope.StartPos.position).normalized;
                }
                else
                {
                    _moveDirection = Vector3.zero;
                }
                totalVelocity *= 3.0f;

                // RopeStopCondition
                if(_holdRope.StartPos.position.y <_leftHandPos.position.y||
                    _holdRope.StartPos.position.y < _rightHandPos.position.y)
                {
                    if (speed > 0)
                    {
                        _character.Teleport(_holdRope.transform.position);
                        ReleaseRope();
                        speed = 0;
                        totalVelocity = Vector3.zero;
                    }
                }

                // RopeJump
                // Jump
                if ( _currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.Jump))
                {
                    ReleaseRope();
                    Vector3 jumpDirection = -transform.forward;
                    jumpDirection.y = 1;
                    jumpDirection.Normalize();
                    _character.Jump(jumpDirection, 10);
                    _character.AddLookAngle(180);
                    if (Runner.IsForward)
                        _character.AnimatorSetTrigget("Jump");
                }
            }
        }
        _character.Move(totalVelocity);
        _animator.SetFloat("Speed", speed, 0.2f, Runner.DeltaTime);

    }
    protected override void ProcessRotate()
    {
        Vector3 totalDeltaAngle = Vector3.zero;

        // Rotate
        if (IsEnableInputRotate)
        {
            totalDeltaAngle += (Vector3)_currentPlayerInputData.lookRotationDelta;
            if (!_isPlayTurnAnimation &&_character.IsGrounded && Mathf.Abs(totalDeltaAngle.y) > 150)
            {
                if (Runner.IsForward)
                {
                    _character.AnimatorSetTrigget("Turn");
                    _turnAnimationCoroutine = StartCoroutine(Utils.WaitAniationAndPlayCoroutine(_animator, new string[] { "Walking Turn 180", "Running Turn 180" }, () =>
                    {
                        IsEnableInputRotate = true;
                        _isPlayTurnAnimation = false;
                        _model.transform.localPosition = Vector3.zero;
                        _model.transform.localRotation = Quaternion.identity;
                    }));
                }
                _isPlayTurnAnimation = true;
                IsEnableInputRotate = false;
                
                totalDeltaAngle = Vector3.zero;
            }
        }

        if (IsEnableAnimationRotate)
        {
            if (Runner.IsForward)
            {
                // 실제적으로 몸을 움직일 때 EX) 뒤돌기
                if (_isPlayTurnAnimation)
                {
                    totalDeltaAngle += _currentPlayerInputData.animatorDeltaAngle;
                }
                else
                {
                    _model.transform.localRotation = Quaternion.Euler(_model.transform.localRotation.eulerAngles + _currentPlayerInputData.animatorDeltaAngle);
                }
            }
        }

        _character.AddLookAngle(totalDeltaAngle.y);
    }
    protected override void ProcessRope()
    {
        base.ProcessRope();

        if(_holdRope)
        {
            if(_leftHandPos != null)
            {
                Vector3 ropeToLeftHand = _leftHandPos.transform.position- _holdRope.RopeModel.transform.position ;
                Vector3 ropeToRightHand = _rightHandPos.transform.position- _holdRope.RopeModel.transform.position ;

               
                Vector3 result = transform.position - 
                    ropeToLeftHand * _animatorHelper.UseLeftHandRatio-
                    ropeToRightHand * _animatorHelper.UseRightHandRatio;
                result.y = transform.position.y;

                _character.Teleport(result);
            }
        }
    }
    protected override void HoldRope(NetworkId ropeId)
    {
        NetworkObject networkObject = Runner.FindObject(ropeId);
        if (networkObject != null)
        {
            _holdRope = networkObject.GetComponentInParent<Rope>();
            if (_holdRope != null)
            {
                IsHoldRope = true;
                _character.IsEnableMoveYAxis = true;


                Vector3 lVec = _holdRope.EndPos.position - _holdRope.StartPos.position;
                Vector3 vec = transform.position - _holdRope.StartPos.position;
                float t = Vector3.Dot(lVec, vec) / Vector3.Dot(lVec, lVec);
                t = Mathf.Clamp01(t);
                Vector3 lookAtRopeDirection = (_holdRope.transform.position - transform.position);
                lookAtRopeDirection.y = 0;
                lookAtRopeDirection.Normalize();
                float deltaAngle = Vector3.SignedAngle(transform.forward, lookAtRopeDirection, Vector3.up);
                _character.AddLookAngle(deltaAngle);


                _animator.SetBool("HoldRope", true);
            }
        }
    }
    protected override void ReleaseRope()
    {
        IsHoldRope = false;
        _character.IsEnableMoveYAxis = false;
        _holdRope = null;
        
        _animator.SetBool("HoldRope", false);
    }
    public override void PlayAttack()
    {
        if (_isPlayAttackAnimation) return;

        _character.AnimatorSetTrigget("Attack");
        IsEnableInputRotate = false;
        IsEnableInputMove = false;
        _isPlayAttackAnimation = true;
        AttackAnimationStarted?.Invoke();

        _attackAnimationCoroutine = StartCoroutine(Utils.WaitAniationAndPlayCoroutine(_animator, "Attack", () =>
        {
            IsEnableInputMove = true;
            IsEnableInputRotate = true;
            _isPlayAttackAnimation = false;
            _model.transform.localPosition = Vector3.zero;
            _model.transform.localRotation = Quaternion.identity;
            AttackAnimationEnded?.Invoke();
        }));
    }
    public override void StartAttack()
    {
        WeaponItem weaponItem = _leftItem as WeaponItem;
        if(weaponItem)
        {
            weaponItem.StartAttack();
        }
            AttackStarted?.Invoke();
    }
    public override void StopAttack()
    {
        WeaponItem weaponItem = _leftItem as WeaponItem;
        if (weaponItem)
        {
            weaponItem.EndAttack();
        }
            AttackEnded?.Invoke();
    }
    protected override void StartWorking()
    {
        if (_currentWorkingInputData.isWorking)
        {
            NetworkObject networkObject = Runner.FindObject(_currentWorkingInputData.workingTargetID);
            _workingBlock = networkObject.GetComponent<WorkingBlock>();
            WorkingTime = _workingBlock.RequireTime;
            WorkingTimer = TickTimer.CreateFromSeconds(Runner, WorkingTime);
            IsWorking = true;
            IsEnableInputRotate = false;
            IsEnableInputMove = false;
            _animator.SetBool("Working", true);
        }
    }
    protected override void CancelWorking()
    {
        if (IsWorking)
        {
            if (_currentWorkingInputData.isCancelWorking)
            {
                IsWorking = false;
                IsEnableInputRotate = true;
                IsEnableInputMove = true;
                CancelWorkingFrame = Time.frameCount;
               _animator.SetBool("Working", false);
            }
        }
    }
    protected override void Working()
    {
        if (IsWorking)
        {
            if (WorkingTimer.Expired(Runner))
            {
                if (_workingBlock)
                {
                    if (Object.HasStateAuthority)
                    {
                        Runner.Spawn(_workingBlock.SpawnObject, _workingBlock.transform.position);
                        Runner.Despawn(_workingBlock.Object);
                    }
                }
                IsWorking = false;
                IsEnableInputRotate = true;
                IsEnableInputMove = true;
                CancelWorkingFrame = Time.frameCount;
               _animator.SetBool("Working", false);
            }
        }
    }
}
