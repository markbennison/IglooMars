using System;
using UnityEngine;

namespace Igloo {
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerManager : MonoBehaviour
    {
        public string playerName = "player";
        public Camera m_Camera;
        public bool UsePlayer {
            get { return enabled; }
            set {
                m_Camera.gameObject.SetActive(value);
                GetComponent<CharacterController>().enabled = value;
                GetComponent<Rigidbody>().detectCollisions = !value;
                enabled = value;
            }
        }

        public HeadManager      headManager;
        public Crosshair        crosshair;
        public PlayerPointer    pointer;
        public VRController     vrController;

        public enum ROTATION_INPUT {STANDARD, WARPER, GYROSC, VRCONTROLLER};
        [Header("Igloo Settings")]
        public ROTATION_INPUT rotationInput = ROTATION_INPUT.STANDARD;

        public enum ROTATION_MODE {IGLOO_360, IGLOO_NON_360, GAME}
        public ROTATION_MODE rotationMode = ROTATION_MODE.IGLOO_360;

        public enum MOVEMENT_INPUT { STANDARD, GYROSC, VRCONTROLLER};
        public MOVEMENT_INPUT movementInput = MOVEMENT_INPUT.STANDARD;

        public enum MOVEMENT_MODE { WALKING, FLYING, FLYING_GHOST };
        public MOVEMENT_MODE movementMode = MOVEMENT_MODE.WALKING;

        // Cached Values to handle changing  modes
        private MOVEMENT_MODE cachedMovmentMode = MOVEMENT_MODE.WALKING;
        private ROTATION_MODE cachedRotationMode = ROTATION_MODE.IGLOO_360;


        private float cached_m_GravityMultiplier = 0.0f;
        private float cached_m_StickToGroundForce = 0.0f;

        [Header("Character Controller Settings")]
        [SerializeField] private bool m_IsWalking = false;
        [SerializeField] private float m_WalkSpeed = 5f; // maximum speed of walking
        [SerializeField] private float m_RunSpeed = 10f;
        [SerializeField] private float m_Climbspeed = 0.0f;
        [SerializeField] private float m_JumpSpeed = 10f;
        [SerializeField] private float m_StickToGroundForce = 2f;
        [SerializeField] private float m_GravityMultiplier = 1f;
        [SerializeField] private LookWarper m_LookWarper =null;
        [SerializeField] private MouseLook m_MouseLook =null;
        [SerializeField] private GyrOSCLook m_gyrOSCLook =null;
        [SerializeField] private VRControllerLook m_vrControllerLook =null;

        public enum CROUCH_MODE { ONE, TWO, THREE };
        public CROUCH_MODE crouchMode = CROUCH_MODE.ONE;
        [SerializeField] private float m_crouchMin = -0.4f;
        [SerializeField] private float m_crouchMax = 0.2f;


        private Vector3 _prevAxis; // used for tilting system
        private float _prevAngle; // used for tilting system 


        private bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private bool m_Jumping;
        private float lift = 0.0f;
        private float m_currRotButtonTime = 0.0f;
        private float m_VrRotActivateTime = 0.0f;
        private bool m_VrRotButtonHeld = false;
        public bool m_invertPitch = false;
        private bool m_headtracking = true;

        private float m_smoothTime = 10f;
        public float SmoothTime {
            get { return m_smoothTime; }
            set
            {
                m_LookWarper.smoothTime = value;
                m_MouseLook.smoothTime  = value;
                m_gyrOSCLook.smoothTime = value;
                m_vrControllerLook.smoothTime = value;
                m_smoothTime = value;
            }
        }
        private Vector3 movementAxisGyrOSC;
        
        private Vector2 vrControllerPadPosition;
        private bool vrControllerPadButton = false;
        private bool vrControllerGripButton = false;
        public void SetSettings(PlayerSettings ps) {
            if (ps == null) return;
            playerName      = ps.Name;
            movementMode    = (MOVEMENT_MODE)ps.movementMode;
            movementInput   = (MOVEMENT_INPUT)ps.movementInput;
            rotationInput   = (ROTATION_INPUT)ps.rotationInput;
            rotationMode    = (ROTATION_MODE)ps.rotationMode;
            UsePlayer       = ps.usePlayer;
            SmoothTime    = ps.smoothTime;
            if (ps.runSpeed!=0) m_RunSpeed  = ps.runSpeed;
            if(ps.walkSpeed!=0) m_WalkSpeed = ps.walkSpeed;
            SetCrosshairMode((Crosshair.CROSSHAIR_MODE)ps.crosshairHideMode);
            pointer.SetDraw3D(ps.isCrosshair3D);
        }


        public PlayerSettings GetSettings() {
            PlayerSettings ps = new PlayerSettings();
            ps.Name             = playerName;
            ps.movementMode     = (int)movementMode;
            ps.rotationInput    = (int)rotationInput;
            ps.rotationMode     = (int)rotationMode;
            ps.movementInput    = (int)movementInput;
            ps.usePlayer        = UsePlayer;
            ps.walkSpeed        = m_WalkSpeed;
            ps.runSpeed         = m_RunSpeed;
            ps.smoothTime       = m_smoothTime;
            ps.crosshairHideMode= (int)GetCrosshairMode();
            ps.isCrosshair3D    = pointer.GetDraw3D();
            return ps;
        }

        private void Start() {
            m_CharacterController = GetComponent<CharacterController>();
            if (!headManager) Debug.LogError("HeadManager has not been assigned");
            if (!m_Camera) Debug.LogError("Camera has not been assigned");
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_MouseLook.Init(transform, m_Camera.transform,playerName, m_smoothTime);
            m_LookWarper.Init(transform, m_Camera.transform,playerName, m_smoothTime);
            m_gyrOSCLook.Init(transform, m_Camera.transform, playerName, m_smoothTime);
            m_vrControllerLook.Init(transform, m_Camera.transform, playerName, m_smoothTime);
            m_vrControllerLook.vrControllerGO = vrController.gameObject;
            m_vrControllerLook.UpdateLastFrameY();
            if (IglooManager.Instance == null) {
                Debug.LogError("Igloo Manager Missing");
                return;
            }
            // Register to events
            IglooManager.Instance.GetNetworkManager().OnPlayerWalkSpeed += SetWalkSpeed;
            IglooManager.Instance.GetNetworkManager().OnPlayerRunSpeed += SetRunSpeed;
            IglooManager.Instance.GetNetworkManager().OnPlayerRotationMode += SetRotationMode;
            IglooManager.Instance.GetNetworkManager().OnPlayerRotationInput += SetRotationInput;
            IglooManager.Instance.GetNetworkManager().OnPlayerMovementInput += SetMovementInput;
            IglooManager.Instance.GetNetworkManager().OnPlayerMovementMode += SetMovementMode;
            IglooManager.Instance.GetNetworkManager().OnPlayerSmoothTime += SetSmoothTime;
            IglooManager.Instance.GetNetworkManager().OnButtonGYROSC += SetGYROSCButton;
            IglooManager.Instance.GetNetworkManager().OnVrPadPositionEvent += SetVRControllerPadPosition;
            IglooManager.Instance.GetNetworkManager().OnVrButtonEvent += SetVRControllerButtons;

            if (rotationInput == ROTATION_INPUT.VRCONTROLLER) {
                pointer.enabled = false;
                crosshair.gameObject.SetActive(false);
                vrController.gameObject.SetActive(true);
            } 
        }

        private void SetWalkSpeed(string name, float value) {
            if (name == playerName) m_WalkSpeed = value;
        }
        private void SetRunSpeed(string name, float value)
        {
            if (name == playerName) m_RunSpeed = value;
        }
        private void SetRotationInput(string name, int value)
        {
            if (name == playerName) rotationInput = (ROTATION_INPUT) value;
        }
        private void SetRotationMode(string name, int value)
        {
            if (name == playerName) rotationMode = (ROTATION_MODE) value;
        }
        private void SetMovementInput(string name, int value)
        {
            if (name == playerName) movementInput = (MOVEMENT_INPUT)value;
        }
        private void SetMovementMode(string name, int value)
        {
            if (name == playerName) movementMode = (MOVEMENT_MODE)value;
        }
        private void SetSmoothTime(string name, float value)
        {
            if (name == playerName) SmoothTime = value;
        }
        private void SetGYROSCButton(string name, Vector3 movement)
        {
            if (name == playerName) movementAxisGyrOSC = movement;
        }
        private void SetVRControllerPadPosition(int deviceID, Vector2 movement) {
            if (deviceID == 1) vrControllerPadPosition = movement;
        }
        private void SetVRControllerButtons(int deviceID, int buttonID, bool state) {
            if (deviceID == 1) {
                if (buttonID == 2) vrControllerPadButton = state;
                else if (buttonID == 3) vrControllerGripButton = state;
            } 
        }

        #region Ladder System, Collider interaction.

        private bool _isClimbing = false;
        private Transform _ladderTrigger;

        private void OnTriggerEnter(Collider col) {
            if (col.tag == "ladder bottom" || col.tag == "ladder top") _ladderTrigger = col.transform;

            if (col.tag == "ladder bottom") {
                if (!_isClimbing) {
                    // player is not climbing the ladder, and wants to.
                    // Check for player interaction
                    // Prompt for Keypress
                    // Check for Keypress (controller A)
                    _climbingTransition = TransitionState.ToLadder1;
                    ToggleClimbing();
                }
                else {
                    // player has reached the bottom of the ladder, whilst climbing.
                    // Player detatch
                    ToggleClimbing();
                    _climbingTransition = TransitionState.None;
                }
            }


            if (col.tag == "ladder top") {
                // if player hit the top of the ladder, whilst climbing it.
                if (_isClimbing)
                {
                    // move to the upper point and exit the ladder.
                    _climbingTransition = TransitionState.ToLadder3;
                }
                else
                {
                    // player has come from above, and wants to go down the ladder
                    // Check for player interaction
                    // Make ladder selectable and glow. 
                    // Prompt for Keypress
                    // Check for Keypress (controller A)
                    _climbingTransition = TransitionState.ToLadder2;
                }

                ToggleClimbing();

            }

            if (col.tag == "Respawn") {
                throw new NotImplementedException("Death System");
            }
        }

        private void ToggleClimbing() {
            _isClimbing = !_isClimbing;
            //ladderIconToggle = !ladderIconToggle;
            //_ladderIconInUse.SetActive(ladderIconToggle);
        }

        private enum TransitionState {
            None = 0,
            ToLadder1 = 1,
            ToLadder2 = 2,
            ToLadder3 = 3
        }

        private TransitionState _climbingTransition = TransitionState.None;
        // private bool ladderIconToggle = false;
        private GameObject _ladderIconInUse;

#endregion


        // Update is called once per frame
        private void Update() {
            Vector3 pos = this.transform.localPosition;

            if (movementMode != cachedMovmentMode) UpdateMovementMode();
            if (rotationMode != cachedRotationMode) UpdateMovementMode();

            RotateView();
            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)  {
                m_Jump = Input.GetButtonDown("Jump");
            }
            try {
                if (Input.GetButtonDown("XboxB"))  {
                    _isClimbing = false;
                }
            }
            catch{}

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded) {
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded) {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;

            if (movementMode != cachedMovmentMode) UpdateMovementMode();
            if (rotationMode != cachedRotationMode) UpdateRotationMode();

        }

        private void UpdateMovementMode() {
            switch (movementMode) {
                case MOVEMENT_MODE.FLYING:
                    GetComponent<Rigidbody>().useGravity = false;
                    if (cachedMovmentMode == MOVEMENT_MODE.WALKING) {
                        cached_m_GravityMultiplier = m_GravityMultiplier;
                        cached_m_StickToGroundForce = m_StickToGroundForce;
                    }
                    m_StickToGroundForce = 0;
                    m_GravityMultiplier = 0;
                    break;

                case MOVEMENT_MODE.FLYING_GHOST:
                    GetComponent<Rigidbody>().useGravity = false;
                    if (cachedMovmentMode == MOVEMENT_MODE.WALKING) {
                        cached_m_GravityMultiplier = m_GravityMultiplier;
                        cached_m_StickToGroundForce = m_StickToGroundForce;
                    }
                    m_StickToGroundForce = 0;
                    m_GravityMultiplier = 0;
                    break;

                case MOVEMENT_MODE.WALKING:
                    GetComponent<Rigidbody>().useGravity = true;
                    m_GravityMultiplier = cached_m_GravityMultiplier;
                    m_StickToGroundForce = cached_m_StickToGroundForce;
                    break;
            }
            cachedMovmentMode = movementMode;
        }

        private void UpdateRotationMode() {
            this.transform.localEulerAngles = Vector3.zero;
            m_Camera.transform.parent.transform.localEulerAngles = Vector3.zero;
            m_Camera.transform.localEulerAngles = Vector3.zero;
            headManager.transform.localEulerAngles = Vector3.zero;

            cachedRotationMode = rotationMode;
        }

        private void FixedUpdate() {
            if (movementMode != cachedMovmentMode) UpdateMovementMode();

            float speed = 0f;
            GetInput(out float curspeed);

            if (curspeed > speed) {
                speed = Mathf.Lerp(speed, curspeed, m_WalkSpeed * Time.smoothDeltaTime);
            }
            else if (speed > curspeed) {
                speed = Mathf.Lerp(curspeed, speed, m_WalkSpeed * Time.smoothDeltaTime);
            }
            // always move along the camera forward as it is the direction that it being aimed at
            // unless using a VR controller in which case use it's forward vector
            Vector3 desiredMove = Vector3.zero;
            if (rotationInput == ROTATION_INPUT.VRCONTROLLER) {
                desiredMove = vrController.transform.forward * m_Input.y + vrController.transform.right * m_Input.x;
            } else {
                desiredMove = m_Camera.transform.forward * m_Input.y + m_Camera.transform.right * m_Input.x;
            }
            

            // get a normal for the surface that is being touched to move along it
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out RaycastHit hitInfo,
                                m_CharacterController.height / 2f);

            m_MoveDir.x = desiredMove.x * speed;
            if (movementMode == MOVEMENT_MODE.FLYING || movementMode == MOVEMENT_MODE.FLYING_GHOST) {
                if (lift == 0) m_MoveDir.y = desiredMove.y * speed;
                else m_MoveDir.y = lift * speed;
            }
            m_MoveDir.z = desiredMove.z * speed;


#region Ladder Climbing Logic

            Vector3 ladderDesiredMove = Vector3.zero;

            if (_isClimbing)
            {

                Transform trLadder = _ladderTrigger.parent;

                if (_climbingTransition != TransitionState.None)
                {
                    print("First IF - Climbing Transition" + _climbingTransition.ToString());

                    // Move the player to the next point along their current path
                    transform.position = trLadder.Find(_climbingTransition.ToString()).position;

                    _climbingTransition = TransitionState.None;
                }
                else
                {
                    print("First Else - Climbing Transition " + " Rotation system.");
                    // Attach player to the ladder, with the rotation angle of the ladder's transform
                    ladderDesiredMove = trLadder.rotation * Vector3.forward * m_Input.y;

                    m_MoveDir.y = ladderDesiredMove.y * m_Climbspeed;
                    m_MoveDir.x = ladderDesiredMove.x * m_Climbspeed;
                    m_MoveDir.z = ladderDesiredMove.z * m_Climbspeed;

                    m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
                }
            }
            else
            {
                //print("Second Else - Movement system");

                ladderDesiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

                // find the normal for the surface that is being touched to move along it
                Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo, m_CharacterController.height / 2f);
                ladderDesiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

                m_MoveDir.x = ladderDesiredMove.x * speed;
                m_MoveDir.z = ladderDesiredMove.z * speed;

                if (m_CharacterController.isGrounded)
                {
                    m_MoveDir.y = -m_StickToGroundForce;
                }
            }



#endregion

            if (movementMode != MOVEMENT_MODE.FLYING_GHOST) {
                if (m_CharacterController.isGrounded) {
                    m_MoveDir.y = -m_StickToGroundForce;

                    if (m_Jump) {
                        m_MoveDir.y = m_JumpSpeed;
                        m_Jump = false;
                        m_Jumping = true;
                    }
                }
                else {
                    m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.smoothDeltaTime;
                }
            }

            if (!PlayerFrozen) {
                if (movementMode != MOVEMENT_MODE.FLYING_GHOST)
                    m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

                else if (movementMode == MOVEMENT_MODE.FLYING_GHOST) {
                    transform.Translate(m_MoveDir * Time.fixedDeltaTime, Space.World);
                }
            }
            UpdateCrouch();
        }

        void Teleport(Transform floorNode) {
            gameObject.transform.position = floorNode.position;
        }

        private void UpdateCrouch() {
            if (movementMode == MOVEMENT_MODE.WALKING) {
                float min;
                float max;
                Vector3 newCameraPosition = m_Camera.transform.localPosition;

                switch (crouchMode)  {
                    case CROUCH_MODE.ONE:
                        newCameraPosition.y = Mathf.Lerp(newCameraPosition.y, Mathf.Clamp(newCameraPosition.y + lift, m_crouchMin, m_crouchMax), Time.deltaTime * 3);
                        m_Camera.transform.localPosition = newCameraPosition;
                        break;
                    case CROUCH_MODE.TWO:
                        min = m_OriginalCameraPosition.y + m_crouchMin;
                        max = m_OriginalCameraPosition.y + m_crouchMax;
                        if (lift == 0) {
                            newCameraPosition.y = Mathf.Lerp(newCameraPosition.y, m_OriginalCameraPosition.y, Time.deltaTime * 3);
                            m_Camera.transform.localPosition = newCameraPosition;
                        }
                        else if (lift < 0) {
                            newCameraPosition.y = Mathf.Lerp(newCameraPosition.y, min, Time.deltaTime * -lift * 3);
                            m_Camera.transform.localPosition = newCameraPosition;
                        }
                        else if (lift > 0) {
                            newCameraPosition.y = Mathf.Lerp(newCameraPosition.y, max, Time.deltaTime * lift * 3);
                            m_Camera.transform.localPosition = newCameraPosition;
                        }
                        break;
                    case CROUCH_MODE.THREE:
                        min = m_OriginalCameraPosition.y - (m_crouchMin * lift);
                        max = m_OriginalCameraPosition.y + (m_crouchMax * lift);
                        if (lift == 0) {
                            newCameraPosition.y = Mathf.Lerp(newCameraPosition.y, m_OriginalCameraPosition.y, Time.deltaTime * 3);
                            m_Camera.transform.localPosition = newCameraPosition;
                        }
                        else if (lift < 0) {
                            newCameraPosition.y = Mathf.Lerp(newCameraPosition.y, min, Time.deltaTime * 3);
                            m_Camera.transform.localPosition = newCameraPosition;
                        }
                        else if (lift > 0) {
                            newCameraPosition.y = Mathf.Lerp(newCameraPosition.y, max, Time.deltaTime * 3);
                            m_Camera.transform.localPosition = newCameraPosition;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void GetInput(out float speed) {
            float horizontal    = 0.0f;
            float vertical      = 0.0f;

            switch (movementInput) {
                case MOVEMENT_INPUT.STANDARD:
                    horizontal  = Input.GetAxis("Horizontal");
                    vertical    = Input.GetAxis("Vertical");
                    //lift = Input.GetAxis("Igloo_Lift");
                    break;
                case MOVEMENT_INPUT.GYROSC:
                    horizontal  = movementAxisGyrOSC.z;
                    vertical    = movementAxisGyrOSC.x;
                    lift        = movementAxisGyrOSC.y;
                    break;
                case MOVEMENT_INPUT.VRCONTROLLER:
                    if (vrControllerPadButton) {
                        horizontal = vrControllerPadPosition.x;
                        vertical = vrControllerPadPosition.y;
                        
                    }

                    if (vrControllerGripButton) {
                        if (!m_VrRotButtonHeld) {
                            m_currRotButtonTime += Time.deltaTime;

                            if (m_currRotButtonTime >= m_VrRotActivateTime) {
                               // Debug.Log("Grip Held!");
                                m_VrRotButtonHeld = true;
                                //Reset m_lastFrameY 
                                m_vrControllerLook.UpdateLastFrameY();
                            }
                        }
                    }
                    else {
                        m_VrRotButtonHeld = false;
                        m_currRotButtonTime = 0.0f;
                    }
                    break;
                default:
                    break;
            }
            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            //m_IsWalking = !Input.GetKey(KeyCode.LeftShift); // Igloo - removed
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.JoystickButton4)) {
                m_IsWalking = false;
            }
            else
                m_IsWalking = true;
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1) {
                m_Input.Normalize();
            }
        }


        private void RotateView() {
            if (!PlayerFrozen) {
                if (false) {

                }
                else {
                    Transform yRotationTransform = m_Camera.transform.parent;
                    Transform xRotationTransform = m_Camera.transform;
                        
                    switch (rotationMode) {
                        case ROTATION_MODE.IGLOO_360:
                            xRotationTransform = m_Camera.transform;
                            yRotationTransform = m_Camera.transform.parent;
                            break;
                        case ROTATION_MODE.IGLOO_NON_360:
                            if (m_headtracking) {
                                xRotationTransform = this.transform;
                                yRotationTransform = this.transform;

                            } else {
                                xRotationTransform = m_Camera.transform;
                                yRotationTransform = headManager.transform;
                            }
                            break;
                        case ROTATION_MODE.GAME:
                            xRotationTransform = headManager.transform;
                            yRotationTransform = this.transform;
                            break;
                        default:
                            break;
                    }

                    switch (rotationInput) {
                        case ROTATION_INPUT.STANDARD:
                            m_MouseLook.LookRotation(yRotationTransform, xRotationTransform, m_invertPitch, rotationMode);
                            break;
                        case ROTATION_INPUT.WARPER:
                            m_LookWarper.LookRotation(yRotationTransform, xRotationTransform, m_invertPitch, rotationMode);
                            break;
                        case ROTATION_INPUT.GYROSC:
                            m_gyrOSCLook.LookRotation(yRotationTransform, xRotationTransform, m_invertPitch, rotationMode);
                            break;
                        case ROTATION_INPUT.VRCONTROLLER:
                            if(m_VrRotButtonHeld)
                                m_vrControllerLook.LookRotation(yRotationTransform, xRotationTransform, m_invertPitch, rotationMode);
                            break;
                    }
                }
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit) {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below) {
                return;
            }

            if (body == null || body.isKinematic) {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }

        public bool PlayerFrozen { get; set; } = false;

        public void SetCrosshairMode(Crosshair.CROSSHAIR_MODE mode) {
            if (crosshair) {
                crosshair.SetMode(mode);
            }
        }

        private Crosshair.CROSSHAIR_MODE GetCrosshairMode() {
            Crosshair.CROSSHAIR_MODE mode = Crosshair.CROSSHAIR_MODE.HIDE;
            if (crosshair) mode = crosshair.crosshairMode;
            return mode;
        }

    }

    [Serializable]
    public class Look {
        public bool clampVerticalRotation = false;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth = true;
        public float smoothTime = 10f;
        public string m_playerName;

        public float yRot;
        public float xRot;

        protected Quaternion m_CharacterTargetRot;
        protected Quaternion m_CameraTargetRot;


        public virtual void Init(Transform character, Transform camera, string playerName, float smoothTime) {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
            m_playerName = playerName;
        }

        public virtual void LookRotation(Transform character, Transform camera, bool invertPitch, PlayerManager.ROTATION_MODE rotationMode) {

        }

    }

    [Serializable]
    public class MouseLook : Look {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;

        public override void LookRotation(Transform character, Transform camera, bool invertPitch, PlayerManager.ROTATION_MODE rotationMode) {
            float joystickX = 0f;
            float joystickY = 0f;

            try {
                joystickX = Input.GetAxis("Right Stick X Axis");
                joystickY = Input.GetAxis("Right Stick Y Axis");
            }
            catch {
                Debug.LogWarning("Joystick Axis not mapped for Igloo PlayerManager ");
            }
            
            yRot = Input.GetAxis("Mouse X") + joystickX * XSensitivity;
            xRot = Input.GetAxis("Mouse Y") + joystickY * YSensitivity;

            if (invertPitch) xRot = -xRot;

            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation) m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            if (smooth) {
                character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot, smoothTime * Time.deltaTime);
            }
            else {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }
        }


        Quaternion ClampRotationAroundXAxis(Quaternion q) {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }

    [Serializable]
    public class LookWarper : Look {

        public override void Init(Transform character, Transform camera, string playerName, float _smoothTime) {
            base.Init(character, camera, playerName, _smoothTime);
            if (IglooManager.Instance!= null) IglooManager.Instance.GetNetworkManager().OnPlayerRotationWarper += SetRotation;
            }

        public override void LookRotation(Transform character, Transform camera, bool invertPitch, PlayerManager.ROTATION_MODE rotationMode) {            
            if (invertPitch) xRot = -xRot;
            
            m_CharacterTargetRot = Quaternion.Euler(0, yRot, 0f);
            m_CameraTargetRot = Quaternion.Euler(xRot, 0f, 0f);

            if (smooth) {
                character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot, smoothTime * Time.deltaTime);
            }
            else {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }
        }

        void SetRotation(string name, Vector3 rot) {
            if (name == m_playerName) {
                xRot = rot.x;
                yRot = rot.y;
            }
        }

    }

    [Serializable]
    public class VRControllerLook : Look {
        public float XSensitivity = 2f;
        public float YSensitivity = 1.5f;
        public float rightBoundDegrees = 180.0f;
        public float leftBoundDegrees = -180.0f;
        public GameObject vrControllerGO;

        public float m_lastFrameY = 0.0f;
        public void UpdateLastFrameY() {
           m_lastFrameY = vrControllerGO.transform.localEulerAngles.y;
        }
        public override void Init(Transform character, Transform camera, string playerName, float _smoothTime) {
            base.Init(character, camera, playerName, _smoothTime);
            if (IglooManager.Instance != null) {
                rightBoundDegrees = IglooManager.Instance.GetDisplayManager().GetSettings().horizontalFOV /2;
                leftBoundDegrees = -rightBoundDegrees;

               // m_lastFrameY = vrControllerGO.transform.localEulerAngles.y;
            } 
        }

        public override void LookRotation(Transform character, Transform camera, bool invertPitch, PlayerManager.ROTATION_MODE rotationMode) {
            yRot = 0.0f;

            Vector3 rot = vrControllerGO.transform.localEulerAngles;
            yRot = Mathf.DeltaAngle(m_lastFrameY, rot.y);
            if (Mathf.Abs(yRot) < 0.8f)
                return;
           
            m_lastFrameY = rot.y;


            m_CharacterTargetRot *= Quaternion.Euler(0f, -yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(0f, 0f, 0f);


            if (smooth) {
                character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, smoothTime * Time.deltaTime);
            } else {
                character.localRotation = m_CharacterTargetRot;
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q) {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }

    [Serializable]
    public class GyrOSCLook : Look {
        public override void Init(Transform character, Transform camera, string playerName, float _smoothTime) {
            base.Init(character, camera, playerName, _smoothTime);
            if (IglooManager.Instance != null) IglooManager.Instance.GetNetworkManager().OnRotationGYROSC += SetRotation;
        }

        public override void LookRotation(Transform character, Transform camera, bool invertPitch, PlayerManager.ROTATION_MODE rotationMode) {
            if (invertPitch) xRot = -xRot;
            m_CharacterTargetRot = Quaternion.Euler(0, yRot, 0f);
            m_CameraTargetRot = Quaternion.Euler(xRot, 0f, 0f);

            if (smooth) {
                character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot, smoothTime * Time.deltaTime);
            }
            else {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }
        }

        void SetRotation(string name, Vector3 rot) {
            if (name == m_playerName) {
                xRot = rot.x;
                yRot = rot.y;
            }
        }
    }

}

