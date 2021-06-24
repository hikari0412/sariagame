
using Invector.IK;
using Invector.vShooter;
using System.Collections;
using UnityEngine;

namespace Invector.vCharacterController.AI
{
    [vClassHeader(" AI SHOOTER CONTROLLER", iconName = "AI-icon")]
    public partial class vControlAIShooter : vControlAICombat, vIControlAIShooter, vIShooterIKController
    {
        [vEditorToolbar("Shooter Settings", order = 10)]
        [Header("Shooter Settings")]
        public float minTimeShooting = 2f;
        public float maxTimeShooting = 5f;
        public float minShotWaiting = 3f;
        public float maxShotWaiting = 6f;
        public float aimTargetHeight = .35f;
        public bool doReloadWhileWaiting = true;
        public float aimSmoothDamp = 10f;
        public float smoothArmAlignmentWeight = 4f;
        public float aimTurnAngle = 60f;
        public float maxAngleToShot = 60f;

        public bool IsReloading { get; protected set; }
        public bool IsEquipping { get; protected set; }
        public bool IsInShotAngle { get; protected set; }
        public vAIShooterManager shooterManager { get; set; }
        protected float _timeShotting;

        protected float _waitingToShot;
        protected float _upperBodyID;
        protected float _shotID;      
        protected Quaternion handRotationAlignment;
        protected Quaternion upperArmRotationAlignment;
        protected float armAlignmentWeight;

        #region IKController Interface Properties

        public vIKSolver LeftIK { get; set; }

        public vIKSolver RightIK { get; set; }

        public vWeaponIKAdjustList WeaponIKAdjustList
        {
            get
            {
                if (shooterManager)
                {
                    return shooterManager.weaponIKAdjustList;
                }


                return null;
            }
            set
            {
                if (shooterManager)
                {
                    shooterManager.weaponIKAdjustList = value;
                }
            }
        }

        public vWeaponIKAdjust CurrentWeaponIK
        {
            get
            {
                if (shooterManager)
                {
                    return shooterManager.CurrentWeaponIK;
                }


                return null;
            }
        }

        public void SetIKAdjust(vWeaponIKAdjust iKAdjust)
        {
            if (shooterManager) shooterManager.SetIKAdjust(iKAdjust);
        }

        public void LoadIKAdjust(string weaponCategory)
        {
            if (shooterManager)
            {
                shooterManager.LoadIKAdjust(CurrentActiveWeapon.weaponCategory);
            }
        }

        public bool LockAiming
        {
            get
            {
                return lockAimDebug;
            }
            set
            {
                lockAimDebug = value;
            }
        }

        public bool IsCrouching
        {
            get
            {
                return isCrouching;
            }
            set
            {
                isCrouching = value;
            }
        }

        public bool IsLeftWeapon
        {
            get
            {

                return shooterManager && shooterManager.IsLeftWeapon;
            }
        }
        public bool IsAiming
        {
            get
            {
                return isAiming || lockAimDebug;
            }
        }
        #endregion

        private Transform leftUpperArm, rightUpperArm, leftHand, rightHand;
        private GameObject aimAngleReference;
        private Quaternion upperArmRotation, handRotation;
        private readonly float rightRotationWeight;
        private float _onlyArmsLayerWeight;
        private float handIKWeight;
        private float weaponIKWeight;
        private float aimTime;
        private float delayEnableAimAfterRagdolled;
        private int onlyArmsLayer;
        private int _moveSetID;
        private int _attackID;
        private bool aimEnable;
        [vEditorToolbar("Debug", overrideChildOrder: true, order = 100)]
        [SerializeField, vReadOnly(false)] protected bool _canAiming;
        [SerializeField, vReadOnly(false)] protected bool _canShot;
        [SerializeField, vReadOnly(false)] protected bool _waitingReload;
        [SerializeField, vReadOnly(false)] protected int shots;
      
        public bool debugAim;

        public bool lockAimDebug;
        [SerializeField]
        [vHideInInspector("lockAimDebug")]
        private Transform aimDebugTarget = null;
        [SerializeField]
        [vHideInInspector("lockAimDebug")]
        private bool debugShoots = false;
        private Vector3 aimVelocity;
        private Vector3 aimTarget;
        Vector3 _lastaValidAimLocal;
        protected bool forceCanShot;      

        public event IKUpdateEvent onStartUpdateIK;
        public event IKUpdateEvent onFinishUpdateIK;
        public Vector3 AimPosition { get; protected set; }

        public override void CreateSecondaryComponents()
        {
            base.CreateSecondaryComponents();
            if (GetComponent<vAIShooterManager>() == null) gameObject.AddComponent<vAIShooterManager>();
            if (GetComponent<vAIHeadtrack>() == null) gameObject.AddComponent<vAIHeadtrack>();
        }

        protected virtual int MoveSetID
        {
            get
            {
                return _moveSetID;
            }
            set
            {
                if (value != _moveSetID || animator.GetFloat("MoveSet_ID") != value)
                {
                    _moveSetID = value;
                    animator.SetFloat("MoveSet_ID", (float)_moveSetID, 0.25f, Time.deltaTime);
                }
            }
        }

        protected virtual int AttackID
        {
            get
            {
                return _attackID;
            }
            set
            {
                if (value != _attackID)
                {
                    _attackID = value;
                    animator.SetInteger("AttackID", _attackID);
                }
            }
        }

        public virtual void CheckCanShot()
        {

            if (isAiming && _waitingToShot < Time.time && !inTurn && (isStrafing || debugShoots || input.magnitude < 0.1f))
            {
                _timeShotting = Random.Range(minTimeShooting, maxTimeShooting) + Time.time;
            }
            _canShot = _timeShotting > Time.time;
            if (_canShot)
            {
                _waitingToShot = Time.time + Random.Range(minShotWaiting, maxShotWaiting);
            }

        }

        protected override void Start()
        {
            base.Start();
            _lastaValidAimLocal = defaultValidAimLocal;
            _waitingReload = false;
            InitShooter();
        }

        public  Vector3 _debugAimPosition;

        public virtual Vector3 defaultValidAimLocal
        {
            get
            {
                return Vector3.forward * 10f + Vector3.up * ((_capsuleCollider.height * 0.5f) + aimTargetHeight);
            }
        }

        protected virtual float UpperBodyID
        {
            get { return _upperBodyID; }
            set
            {
                if (_upperBodyID != value || animator.GetFloat("UpperBody_ID") != value)
                {
                    _upperBodyID = value;
                    animator.SetFloat("UpperBody_ID", _upperBodyID);
                }
            }
        }

        protected virtual float ShotID
        {
            get { return _shotID; }
            set
            {
                if (_shotID != value || animator.GetFloat("Shot_ID") != value)
                {
                    _shotID = value;
                    animator.SetFloat("Shot_ID", _shotID);
                }
            }
        }

        protected virtual Vector3 DebugAimPosition
        {
            get
            {
                return !aimDebugTarget ? (transform.position + transform.forward * (2f + _debugAimPosition.z) + transform.right * _debugAimPosition.x + transform.up * (1.5f + _debugAimPosition.y)) : aimDebugTarget.position;
            }
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if (lockAimDebug)
            {
                Gizmos.DrawSphere(DebugAimPosition, 0.1f);
            }
            if (debugAim && currentTarget.transform)
            {
                Gizmos.DrawSphere(AimPosition, 0.1f);
                if(currentTarget.collider)
                    Gizmos.DrawWireCube(currentTarget.collider.bounds.center, currentTarget.collider.bounds.size);
                else Gizmos.DrawWireCube(currentTarget.transform.position, Vector3.one*0.5f);
            }

        }

        public virtual void SetShooterHitLayer(LayerMask mask)
        {
            if (shooterManager)
            {
                shooterManager.SetDamageLayer(mask);
            }
        }

        public override void Attack(bool strongAttack = false, int attackID = -1, bool forceCanAttack = false)
        {
            if (ragdolled) return;
            if (shooterManager && attackID != -1)
                AttackID = attackID;
            else
                AttackID = shooterManager.GetAttackID();

            if (currentTarget.transform || (debugShoots && lockAimDebug) || forceCanAttack)
            {
                forceCanShot = forceCanAttack;
                if (_canShot || forceCanShot)
                {
                    if (shots == 0)
                        shots++;
                }
            }
        }

        public override void InitAttackTime()
        {
            base.InitAttackTime();
            _waitingToShot = Time.time + Random.Range(minShotWaiting, maxShotWaiting);
            _waitingReload = false;
        }

        public override void ResetAttackTime()
        {
            base.ResetAttackTime();
            _waitingToShot = Time.time + Random.Range(minShotWaiting, maxShotWaiting);
        }

        protected virtual void InitShooter()
        {
            if (_headtrack)
            {
                _headtrack.onPreUpdateSpineIK.AddListener(HandleAim);
                _headtrack.onPosUpdateSpineIK.AddListener(IKBehaviour);
            }
            shooterManager = GetComponent<vAIShooterManager>();
            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            onlyArmsLayer = animator.GetLayerIndex("OnlyArms");
            aimAngleReference = new GameObject("aimAngleReference");
            aimAngleReference.transform.rotation = transform.rotation;
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            aimAngleReference.transform.SetParent(head);
            aimAngleReference.transform.localPosition = Vector3.zero;
            AimPosition = DebugAimPosition;
        }

        protected virtual void HandleAim()
        {
            if (ragdolled)
            {
                aimTime = 0;
                isAiming = false;
                delayEnableAimAfterRagdolled = 2f;
                return;
            }
            else if (delayEnableAimAfterRagdolled <= 0)
            {
                ControlAimTime();
                if (isAiming) _headtrack.LookAtPoint(AimPositionClamped(), 1f, 0);
            }
            else
            {
                aimTime = 0;
                isAiming = false;
                delayEnableAimAfterRagdolled -= Time.deltaTime;
            }
        }

        protected virtual void IKBehaviour()
        {
            if (lockAimDebug)
            {
                if (!IsStrafingAnim)
                {
                    isStrafing = true;
                    IsStrafingAnim = true;
                }

                AimTo(DebugAimPosition, .5f);
            }
            UpdateAimBehaviour();
            if (lockAimDebug && debugShoots) Attack();
        }

        protected override void UpdateAnimator()
        {
            base.UpdateAnimator();
            UpdateCombatAnimator();
        }

        protected override void UpdateCombatAnimator()
        {
            base.UpdateCombatAnimator();
            UpdateShooterAnimator();
        }

        protected virtual void UpdateShooterAnimator()
        {
            if (shooterManager.CurrentWeapon)
            {
                IsReloading = IsAnimatorTag("IsReloading");

                // find states with the IsEquipping tag
                IsEquipping = IsAnimatorTag("IsEquipping");
                var _isAiming = isAiming && !IsReloading;
                if (_isAiming && !aimEnable)
                {
                    shooterManager.CurrentWeapon.onEnableAim.Invoke();
                    aimEnable = true;
                }
                else if (!_isAiming && aimEnable)
                {
                    shooterManager.CurrentWeapon.onDisableAim.Invoke();
                    aimEnable = false;
                }
                animator.SetBool("CanAim", _isAiming && _canAiming);

                ShotID = shooterManager.GetShotID();
                UpperBodyID = shooterManager.GetUpperBodyID();
                MoveSetID = shooterManager.GetMoveSetID();
                animator.SetBool("IsAiming", _isAiming);
            }
            else
            {
                IsReloading = false;
                animator.SetBool("IsAiming", false);
                animator.SetBool("CanAim", false);
                if (aimEnable)
                {
                    shooterManager.CurrentWeapon.onDisableAim.Invoke();
                    aimEnable = false;
                }
            }
            _onlyArmsLayerWeight = Mathf.Lerp(_onlyArmsLayerWeight, isAiming || isRolling ? 0f : shooterManager && shooterManager.CurrentWeapon ? 1f : 0f, 6f * Time.deltaTime);
            animator.SetLayerWeight(onlyArmsLayer, _onlyArmsLayerWeight);
        }

        protected virtual void UpdateAimBehaviour()
        {
            if (isDead) return;
            UpdateHeadTrack();
            CheckCanAiming();
            CheckCanShot();
            HandleShots();
            UpdateValidAim();
            ValidateShotAngle();
        }

        protected virtual void HandleShots()
        {
            onStartUpdateIK?.Invoke();
            if (!IsAnimatorTag("IgnoreIK"))
            {
                if (shooterManager && shooterManager.rWeapon && shooterManager.rWeapon.gameObject.activeSelf)
                {
                    UpdateIKAdjust(false);
                    RotateAimArm();
                    RotateAimHand();
                    if (!shooterManager.lWeapon || !shooterManager.lWeapon.gameObject.activeSelf)
                        UpdateSupportHandIK();
                }
                if (shooterManager && shooterManager.lWeapon && shooterManager.lWeapon.gameObject.activeSelf)
                {
                    UpdateIKAdjust(true);
                    RotateAimArm(true);
                    RotateAimHand(true);
                    if (!shooterManager.rWeapon || !shooterManager.rWeapon.gameObject.activeSelf)
                        UpdateSupportHandIK(true);
                }
              
                if (shots > 0)
                {
                    Shot();
                }
            }
            onFinishUpdateIK?.Invoke();
        }

        protected virtual void UpdateIKAdjust(bool isUsingLeftHand)
        {
            vWeaponIKAdjust weaponIKAdjust = shooterManager.CurrentWeaponIK;
            if (!weaponIKAdjust || IsAnimatorTag("IgnoreIK"))
            {
                weaponIKWeight = 0;
                return;
            }
            weaponIKWeight = Mathf.Lerp(weaponIKWeight, IsReloading || IsEquipping ? 0 : 1, 25f * Time.deltaTime);

            if (weaponIKWeight <= 0) return;
            // create left arm ik solver if equal null
            if (LeftIK == null)
            {
                LeftIK = new vIKSolver(animator, AvatarIKGoal.LeftHand);
                LeftIK.UpdateIK();
            }
            if (RightIK == null)
            {
                RightIK = new vIKSolver(animator, AvatarIKGoal.RightHand);
                RightIK.UpdateIK();
            }
            if (isUsingLeftHand)
            {
                ApplyOffsets(weaponIKAdjust, LeftIK, RightIK);
            }
            else
            {
                ApplyOffsets(weaponIKAdjust, RightIK, LeftIK);
            }
        }

        protected virtual void ApplyOffsets(vWeaponIKAdjust weaponIKAdjust, vIKSolver weaponHand, vIKSolver supportHand)
        {
            bool isValid = weaponIKAdjust != null;
            weaponHand.SetIKWeight(weaponIKWeight);
            IKAdjust ikAdjust = isValid ? weaponIKAdjust.GetIKAdjust(isAiming, isCrouching) : null;

            //Apply Offset to Weapon Arm
            ApplyOffsetToTargetBone(isValid ? ikAdjust.weaponHandOffset : null, weaponHand.endBoneOffset, isValid);
            ApplyOffsetToTargetBone(isValid ? ikAdjust.weaponHintOffset : null, weaponHand.middleBoneOffset, isValid);
            //Apply offset to Support Weapon Arm
           
            ApplyOffsetToTargetBone(isValid ? ikAdjust.supportHandOffset : null, supportHand.endBoneOffset, isValid);
            ApplyOffsetToTargetBone(isValid ? ikAdjust.supportHintOffset : null, supportHand.middleBoneOffset, isValid);

            //Convert Animatorion To IK with offsets applied
            if (isValid)
            {
                weaponHand.AnimationToIK();             
                supportHand.SetIKWeight(weaponIKWeight - handIKWeight);
                supportHand.AnimationToIK();
            }
        }

        protected virtual void ApplyOffsetToTargetBone(IKOffsetTransform iKOffset, Transform target, bool isValid)
        {
            target.localPosition = Vector3.Lerp(target.localPosition, isValid ? iKOffset.position : Vector3.zero, 10f * Time.deltaTime);
            target.localRotation = Quaternion.Lerp(target.localRotation, isValid ? Quaternion.Euler(iKOffset.eulerAngles) : Quaternion.Euler(Vector3.zero), 10f * Time.deltaTime);
        }

        protected virtual void UpdateValidAim()
        {
            if (isAiming && _canAiming)
            {

                AimPosition = Vector3.SmoothDamp(AimPosition, aimTarget, ref aimVelocity, aimSmoothDamp * Time.deltaTime);
                _lastaValidAimLocal = transform.InverseTransformPoint(AimPosition);
            }
            else
            {
                if (!isAiming) _lastaValidAimLocal = defaultValidAimLocal;
                AimPosition = transform.TransformPoint(_lastaValidAimLocal);
            }
        }

        protected virtual Vector3 AimPositionClamped()
        {
            var _localAim = defaultValidAimLocal;
            if (_canAiming)
            {
                _localAim = transform.InverseTransformPoint(AimPosition);
                if (_localAim.z < .5f) _localAim.z = .5f;
            }         
            return  transform.TransformPoint(_localAim);
        }

        protected virtual void UpdateHeadTrack()
        {
            if (!shooterManager || !_headtrack)
            {
                if (_headtrack)
                {
                    _headtrack.offsetSpine = Vector2.Lerp(_headtrack.offsetSpine, Vector2.zero, _headtrack.smooth * Time.deltaTime);
                    _headtrack.offsetHead = Vector2.Lerp(_headtrack.offsetHead, Vector2.zero, _headtrack.smooth * Time.deltaTime);
                }
                return;
            }
            if (!CurrentActiveWeapon || !_headtrack || !shooterManager.CurrentWeaponIK)
            {
                if (_headtrack)
                {
                    _headtrack.offsetSpine = Vector2.Lerp(_headtrack.offsetSpine, Vector2.zero, _headtrack.smooth * Time.deltaTime);
                    _headtrack.offsetHead = Vector2.Lerp(_headtrack.offsetHead, Vector2.zero, _headtrack.smooth * Time.deltaTime);
                }
                return;
            }
            if (isAiming)
            {
                var offsetSpine = isCrouching ? shooterManager.CurrentWeaponIK.crouchingAiming.spineOffset.spine : shooterManager.CurrentWeaponIK.standingAiming.spineOffset.spine;
                var offsetHead = isCrouching ? shooterManager.CurrentWeaponIK.crouchingAiming.spineOffset.head : shooterManager.CurrentWeaponIK.standingAiming.spineOffset.head;
                _headtrack.offsetSpine = Vector2.Lerp(_headtrack.offsetSpine, offsetSpine, _headtrack.smooth * Time.deltaTime);
                _headtrack.offsetHead = Vector2.Lerp(_headtrack.offsetHead, offsetHead, _headtrack.smooth * Time.deltaTime);
            }
            else
            {
                var offsetSpine = isCrouching ? shooterManager.CurrentWeaponIK.crouching.spineOffset.spine : shooterManager.CurrentWeaponIK.standing.spineOffset.spine;
                var offsetHead = isCrouching ? shooterManager.CurrentWeaponIK.crouching.spineOffset.head : shooterManager.CurrentWeaponIK.standing.spineOffset.head;
                _headtrack.offsetSpine = Vector2.Lerp(_headtrack.offsetSpine, offsetSpine, _headtrack.smooth * Time.deltaTime);
                _headtrack.offsetHead = Vector2.Lerp(_headtrack.offsetHead, offsetHead, _headtrack.smooth * Time.deltaTime);
            }
        }

        /// <summary>
        /// Current active weapon (if weapon gameobject is disabled this return null)
        /// </summary>
        public virtual vShooter.vShooterWeapon CurrentActiveWeapon
        {
            get
            {
                return shooterManager.CurrentWeapon && shooterManager.CurrentWeapon.gameObject.activeInHierarchy ? shooterManager.CurrentWeapon : null;
            }
        }

        protected virtual void ValidateShotAngle()
        {
            if (shooterManager && isAiming && _canAiming)
            {
                var weapon = shooterManager.rWeapon ? shooterManager.rWeapon : shooterManager.lWeapon;
                if (weapon)
                {
                    var angle = Vector3.Angle(weapon.aimReference.forward, (aimTarget - weapon.aimReference.position).normalized);

                    IsInShotAngle = angle <= maxAngleToShot;
                    if (debugAim)
                    {
                        Debug.DrawRay(weapon.aimReference.position, weapon.aimReference.forward * 100f, IsInShotAngle ? Color.green : Color.red);
                    }
                    return;
                }
            }
            IsInShotAngle = false;
        }
     
        protected virtual void ControlAimTime()
        {
            if (aimTime > 0)
            {

                aimTime -= Time.deltaTime;
            }
            else if (isAiming) isAiming = false;
        }

        protected virtual void UpdateSupportHandIK(bool isUsingLeftHand = false)
        {
            if (ragdolled) return;
            var weapon = shooterManager.rWeapon ? shooterManager.rWeapon : shooterManager.lWeapon;
            if (!shooterManager || !weapon || !weapon.gameObject.activeInHierarchy || !shooterManager.useLeftIK) return;
            if (IsAnimatorTag("Shot") && weapon.disableIkOnShot) { handIKWeight = 0; return; }

            bool useIkConditions = false;
            var animatorInput = isStrafing ? new Vector2(animator.GetFloat("InputVertical"), animator.GetFloat("InputHorizontal")).magnitude : animator.GetFloat("InputVertical");
            if (!isAiming)
            {
                if (animatorInput < 0.1f)
                    useIkConditions = weapon.useIkOnIdle;
                else if (isStrafing)
                    useIkConditions = weapon.useIkOnStrafe;
                else
                    useIkConditions = weapon.useIkOnFree;
            }
            else if (isAiming)
                useIkConditions = weapon.useIKOnAiming;

            // create left arm ik solver if equal null
            if (LeftIK == null || !LeftIK.isValidBones) LeftIK = new IK.vIKSolver(animator, AvatarIKGoal.LeftHand);
            if (RightIK == null || !RightIK.isValidBones) RightIK = new IK.vIKSolver(animator, AvatarIKGoal.RightHand);

            IK.vIKSolver targetIK = null;
          
            if (isUsingLeftHand)
            {
                targetIK = RightIK;               
            }
            else
            {
                targetIK = LeftIK;
               
            }

            if (targetIK != null)
            {
                Vector3 ikRotationOffset = Vector3.zero;
                Vector3 ikPositionOffset = Vector3.zero;
                if (shooterManager.weaponIKAdjustList)
                {
                    if (isUsingLeftHand)
                    {
                        ikRotationOffset = shooterManager.weaponIKAdjustList.ikRotationOffsetR;
                        ikPositionOffset = shooterManager.weaponIKAdjustList.ikPositionOffsetR;
                    }
                    else
                    {
                        ikRotationOffset = shooterManager.weaponIKAdjustList.ikRotationOffsetL;
                        ikPositionOffset = shooterManager.weaponIKAdjustList.ikPositionOffsetL;
                    }
                }

                // control weight of ik
                if (weapon && weapon.handIKTarget && Time.timeScale > 0 && !IsReloading && !actions && !customAction && !IsEquipping && (isGrounded || isAiming) && !lockMovement && useIkConditions)
                    handIKWeight = Mathf.Lerp(handIKWeight, 1, 10f * Time.deltaTime);
                else
                    handIKWeight = Mathf.Lerp(handIKWeight, 0, 10f * Time.deltaTime);

                if (handIKWeight <= 0) return;
                // update IK
                targetIK.SetIKWeight(handIKWeight);
             
                if (shooterManager && weapon && weapon.handIKTarget)
                {
                    var _offset = (weapon.handIKTarget.forward * ikPositionOffset.z) + (weapon.handIKTarget.right * ikPositionOffset.x) + (weapon.handIKTarget.up * ikPositionOffset.y);
                    targetIK.SetIKPosition(weapon.handIKTarget.position + _offset);
                    var _rotation = Quaternion.Euler(ikRotationOffset);
                    targetIK.SetIKRotation(weapon.handIKTarget.rotation * _rotation);
                    if (shooterManager.CurrentWeaponIK)
                    {
                        targetIK.AnimationToIK();                       
                    }
                       
                }
            }
        }

        protected virtual bool CanRotateAimArm()
        {
            return IsAnimatorTag("Upperbody Pose");
        }

        protected virtual void RotateAimArm(bool isUsingLeftHand = false)
        {
            if (!shooterManager) return;

            armAlignmentWeight = (isAiming) && !IsReloading && CanRotateAimArm()&&_canAiming ? Mathf.Lerp(armAlignmentWeight, 1f, smoothArmAlignmentWeight * (Time.deltaTime)) : 0f;
            if (CurrentActiveWeapon && armAlignmentWeight > 0.1f && CurrentActiveWeapon.alignRightUpperArmToAim)
            {
                var aimPoint = AimPositionClamped();
                Vector3 v = aimPoint - CurrentActiveWeapon.aimReference.position;
                var orientation = CurrentActiveWeapon.aimReference.forward;

                var upperArm = isUsingLeftHand ? leftUpperArm : rightUpperArm;
                var rot = Quaternion.FromToRotation(upperArm.InverseTransformDirection(orientation), upperArm.InverseTransformDirection(v));
                if ((!float.IsNaN(rot.x) && !float.IsNaN(rot.y) && !float.IsNaN(rot.z)))
                    upperArmRotationAlignment = shooterManager.isShooting ? upperArmRotation : rot;

                var angle = Vector3.Angle(aimPoint - aimAngleReference.transform.position, aimAngleReference.transform.forward);

                if ((!(angle > shooterManager.maxHandAngle || angle < -shooterManager.maxHandAngle)))
                {
                    upperArmRotation = Quaternion.Lerp(upperArmRotation, upperArmRotationAlignment, shooterManager.smoothHandRotation * (Time.deltaTime));
                }
                else
                {
                    upperArmRotation = Quaternion.Euler(0, 0, 0);
                }

                if (!float.IsNaN(upperArmRotation.x) && !float.IsNaN(upperArmRotation.y) && !float.IsNaN(upperArmRotation.z))
                {
                    float _armAlignmentWeight = CurrentActiveWeapon.alignRightHandToAim ? armAlignmentWeight * 0.5f : armAlignmentWeight;
                    upperArm.localRotation *= Quaternion.Euler(upperArmRotation.eulerAngles.NormalizeAngle() * _armAlignmentWeight);
                }
                   
            }
            else
            {
                upperArmRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        protected virtual void RotateAimHand(bool isUsingLeftHand = false)
        {
            if (!shooterManager) return;

            if (CurrentActiveWeapon && armAlignmentWeight > 0.1f &&  CurrentActiveWeapon.alignRightHandToAim)
            {
                var aimPoint = AimPositionClamped();
                Vector3 v = aimPoint - CurrentActiveWeapon.aimReference.position;
                var orientation = CurrentActiveWeapon.aimReference.forward;
                var hand = isUsingLeftHand ? leftHand : rightHand;
                var rot = Quaternion.FromToRotation(hand.InverseTransformDirection(orientation), hand.InverseTransformDirection(v));
                if ((!float.IsNaN(rot.x) && !float.IsNaN(rot.y) && !float.IsNaN(rot.z)))
                    handRotationAlignment = shooterManager.isShooting ? handRotation : rot;

                var angle = Vector3.Angle(aimPoint - aimAngleReference.transform.position, aimAngleReference.transform.forward);
                if ((!(angle > shooterManager.maxHandAngle || angle < -shooterManager.maxHandAngle)))
                    handRotation = Quaternion.Lerp(handRotation, handRotationAlignment, shooterManager.smoothHandRotation * (Time.deltaTime));
                else handRotation = Quaternion.Euler(0, 0, 0);

                if (!float.IsNaN(handRotation.x) && !float.IsNaN(handRotation.y) && !float.IsNaN(handRotation.z))
                    hand.localRotation *= Quaternion.Euler(handRotation.eulerAngles.NormalizeAngle() * armAlignmentWeight);

                CurrentActiveWeapon.SetScopeLookTarget(aimPoint);
            }
            else handRotation = Quaternion.Euler(0, 0, 0);
        }
        protected virtual void CheckCanAiming()
        {
            if (ragdolled || (!isStrafing && !lockAimDebug) || customAction || IsReloading)
            {
                _canAiming = false;
                return;
            }

            var p1 = aimTarget;
            p1.y = transform.position.y;
            var angle = Vector3.Angle(transform.forward, p1 - transform.position);

           var outAngle = (angle > aimTurnAngle);
            //var aimLocalPoint = transform.InverseTransformPoint(aimTarget);
            //var can = aimLocalPoint.z > _capsuleCollider.radius && Mathf.Abs(aimLocalPoint.x) > _capsuleCollider.radius;
            _canAiming = !outAngle;
            if (outAngle && isAiming) RotateTo(aimTarget - transform.position);
        }

        protected virtual void Shot()
        {
            if (isDead || !shooterManager || !shooterManager.CurrentWeapon || customAction) return;

            if ((_canShot || forceCanShot) && !IsReloading && !_waitingReload && _canAiming && IsInShotAngle && isAiming && !inTurn)
            {
                forceCanShot = false;
                if (shooterManager.weaponHasAmmo)
                {
                    if (shots > 0)
                    {
                        shooterManager.Shoot(CurrentActiveWeapon.muzzle.position + CurrentActiveWeapon.muzzle.forward * 100);
                        shots--;
                    }                   
                }

                else if (!IsReloading && !_waitingReload)
                    StartCoroutine(Reload());
            }

            if (!_canShot && !IsReloading && !_waitingReload && doReloadWhileWaiting && shooterManager.CurrentWeapon.ammoCount < shooterManager.CurrentWeapon.clipSize)
            {
                shooterManager.ReloadWeapon();
            }
        }

        protected virtual IEnumerator Reload()
        {
            _waitingReload = true;
            yield return new WaitForSeconds(.5f);
            shooterManager.ReloadWeapon();
            float minTimeToStartReload = 2f;
            ///Wait enter in Reload State
            while (!IsReloading)
            {
                minTimeToStartReload -= Time.deltaTime;
                if (minTimeToStartReload <= 0) break;
                yield return null;
            }
            ///Wait exit Reload State
            while (IsReloading)
            {

                yield return null;
            }
            yield return new WaitForSeconds(.5f);
            _waitingReload = false;
        }

        protected override void TryBlockAttack(vDamage damage)
        {
            if (shooterManager.CurrentWeapon != null) { isBlocking = false; }
            else base.TryBlockAttack(damage);
        }

        public override void Blocking()
        {
            if (shooterManager.CurrentWeapon != null) { isBlocking = false; return; }
            base.Blocking();
        }

        public override void AimTo(Vector3 point, float timeToCancelAim = 1f, object sender = null)
        {
            aimTime = timeToCancelAim;
            isAiming = true;
            aimTarget = point;
        }

        public override void AimToTarget(float stayLookTime = 1, object sender = null)
        {
            aimTime = stayLookTime;
            isAiming = true;
            if (currentTarget.transform && currentTarget.collider)
                aimTarget = _lastTargetPosition + Vector3.up * ((currentTarget.collider.bounds.size.y * 0.5f) + aimTargetHeight);
            else
                aimTarget = _lastTargetPosition + Vector3.up * aimTargetHeight;
            if (!isStrafing && input.magnitude > 0.1f) isStrafing = true;
        }

        public override void StrafeMoveTo(Vector3 newDestination, Vector3 targetDirection, vAIMovementSpeed speed = vAIMovementSpeed.Walking)
        {
            if (isAiming)
            {
                if (useNavMeshAgent && navMeshAgent && navMeshAgent.isOnNavMesh && navMeshAgent.isStopped) navMeshAgent.isStopped = false;
                SetStrafeLocomotion();
                SetSpeed(speed);
                destination = newDestination;
                if (input.magnitude > 0.1f)
                {
                    temporaryDirection = targetDirection;
                    temporaryDirectionTime = 1f;
                }
            }
            else
                base.StrafeMoveTo(newDestination, targetDirection, speed);
        }
    }
}