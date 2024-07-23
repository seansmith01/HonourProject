using FishNet.Component.Transforming;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;

public struct MoveData : IReplicateData
{
    public bool Jump;
    public bool GotToZero;
    public bool Teleport;
    public float Horizontal;
    public float Vertical;
    public Vector2 LookInput;

    /* Everything below this is required for
    * the interface. You do not need to implement
    * Dispose, it is there if you want to clean up anything
    * that may allocate when this structure is discarded. */
    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}


public struct ReconcileData : IReconcileData
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Quaternion CameraHolderRotation; 
    public float VerticalVelocity;

    /* Everything below this is required for
    * the interface. You do not need to implement
    * Dispose, it is there if you want to clean up anything
    * that may allocate when this structure is discarded. */
    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}


public class CSPMotor : NetworkBehaviour
{
    /// <summary>
    /// Audio to play when jumping.
    /// </summary>
    [SerializeField]
    private AudioSource _jumpAudio;
    /// <summary>
    /// How fast to move.
    /// </summary>
    /// [SerializeField]
    [Header("Vertical Movement")]
    public float MaxFallSpeed;
    [SerializeField]
    private float jumpVelocity = 15f;
    [SerializeField]
    private float gravity = -30f;
    [Header("Horizontal Movement")]
    public float MoveSpeed = 8;


    [SerializeField] Transform cameraHolder;
    [SerializeField] Camera playerCamera;
    //public Camera weaponCamera;
    //[SerializeField] float sensX = 1f;
    //[SerializeField] float sensY = 1f;
    [SerializeField] float sensitivity = 10f;
    private float sensitivityStep = 1f;
    private float minSensitivity = 5f;
    private float maxSensitivity = 15f;


    public Vector2 currentLook;

    PlayerInput playerInput;
    float rotX, rotY;
    Vector2 lookInput;

    /// <summary>
    /// CharacterController on the object.
    /// </summary>
    private CharacterController _characterController;

    public bool ResetQueued;
    /// <summary>
    /// True if a jump was queued on client-side.
    /// </summary>
    private bool jumpQueued;
    public bool GoToZeroo;

    private bool teleportQueued;
    /// <summary>
    /// Velocity of the character, synchronized.
    /// </summary>
    private float verticalVelocity;
    Camera _camera;
    float repeatSpacingX, repeatSpacingY, repeatSpacingZ;

    LevelRepeater levelRepeater;
    ServerManager serverManager;



    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        levelRepeater = FindFirstObjectByType<LevelRepeater>();
        repeatSpacingX = levelRepeater.RepeatSpacing.x;
        repeatSpacingY = levelRepeater.RepeatSpacing.y;
        repeatSpacingZ = levelRepeater.RepeatSpacing.z;
    }
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        
        _camera = GetComponentInChildren<Camera>();

        if (!base.Owner.IsLocalClient)
        {
            GetComponentInChildren<AudioListener>().enabled = false;
            _camera.enabled = false;
        }
        else
        {
            //_characterController.enabled = true; // think this needs to be active for everyone for client collision checks
        }
        serverManager = FindFirstObjectByType<ServerManager>();
        var lvlRepeater = FindFirstObjectByType<LevelRepeater>();
        _camera.backgroundColor = lvlRepeater.FogColours[lvlRepeater.ColourIndex];
        _characterController.enabled = true;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        base.TimeManager.OnTick += TimeManager_OnTick;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        if (base.TimeManager != null)
            base.TimeManager.OnTick -= TimeManager_OnTick;
    }

    private void Update()
    {
        if (!base.IsOwner)
        {
            return;
        }

        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    Cursor.lockState = CursorLockMode.Locked;
        //    Cursor.visible = false;
        //}
        //Check if the owner intends to jump.

        if (Input.GetKeyDown(KeyCode.L))
            GoToZeroo = true;

        if (Input.GetKeyDown(KeyCode.Space))
            jumpQueued = true;

        //_teleportQueued = ShouldWorldWrap();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            transform.position += transform.forward * 20f;
        }
        //!PredictionManager.IsReplaying()

        lookInput = playerInput.actions["Look"].ReadValue<Vector2>();

    }

    private bool ShouldWorldWrap()
    {
        return Mathf.Abs(transform.position.x) > repeatSpacingX / 2f ||
               Mathf.Abs(transform.position.y) > repeatSpacingY / 2f ||
               Mathf.Abs(transform.position.z) > repeatSpacingZ / 2f;
    }

    /// <summary>
    /// Called every time the TimeManager ticks.
    /// This will occur at your TickDelta, generated from the configured TickRate.
    /// </summary>
    /// 
    // On the server, for rigidbodies you will want to run your Replicate
    // in OnTick and send the Reconcile in OnPostTick. This is because
    // the simulation runs after OnTick but before OnPostTick.
    // By sending Reconcile within OnPostTick you are sending the latest values.
    // Check out the TransformPrediction example for more notes.
    // taken from https://fish-networking.gitbook.io/docs/manual/guides/prediction/version-1/using-client-side-prediction
    private void TimeManager_OnTick()
    {
        if(!enabled) return;

        if (base.IsServer)
        {
            Move(default, true);
            ReconcileData rd = new ReconcileData()
            {
                Position = transform.position,
                Rotation = transform.rotation,
                CameraHolderRotation = cameraHolder.rotation,
                VerticalVelocity = verticalVelocity
            };
            Reconcile(rd, true); // if as server is true, the passed data is relayed to the client
        }

        if (base.IsOwner)
        {
            Reconcile(default, false); // "Default is used as the first argument because the values are received from the server and already set using behind the scenes magic"
            BuildActions(out MoveData md);
            Move(md, false);
        }
    }
    /// <summary>
    /// Build MoveData that both the client and server will use in Replicate.
    /// </summary>
    /// <param name="moveData"></param>
    private void BuildActions(out MoveData moveData)
    {
        moveData = default;
        moveData.Jump = jumpQueued;
        moveData.GotToZero = GoToZeroo;
        moveData.Teleport = teleportQueued;
        moveData.Horizontal = Input.GetAxisRaw("Horizontal");
        moveData.Vertical = Input.GetAxisRaw("Vertical");
        moveData.LookInput = lookInput;

        //Unset queued values.
        jumpQueued = false;
        teleportQueued = false;
        //GoToZeroo = false;
    }
    


    /// <summary>
    /// Runs MoveData on the client and server.
    /// </summary>
    /// <param name="asServer">True if the method is running on the server side. False if on the client side.</param>
    /// <param name="replaying">True if logic is being replayed from cached inputs. This only executes as true on the client.</param>
    [Replicate]
    private void Move(MoveData moveData, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
    {
        float delta = (float)base.TimeManager.TickDelta;


        // Adjust sensitivity using mouse scroll wheel
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta != 0f)
        {
            sensitivity = Mathf.Clamp(sensitivity + scrollDelta * sensitivityStep, minSensitivity, maxSensitivity);
            Debug.Log("Mouse sensitivity adjusted to: " + sensitivity);
        }

        // Reset sensitivity if mouse wheel is clicked
        if (Input.GetMouseButtonDown(2)) // 2 represents the middle mouse button (mouse wheel click)
        {
            sensitivity = 10.0f; // Reset to default sensitivity
            Debug.Log("Mouse sensitivity reset to default.");
        }

        //look
        Vector2 _lookInput = moveData.LookInput;

        float x = _lookInput.x * sensitivity * delta;
        transform.Rotate(new Vector3(0, x, 0), Space.Self);


        float y = _lookInput.y * sensitivity * delta;
        cameraHolder.transform.Rotate(new Vector3(-y, 0, 0), Space.Self);


        //move
        Vector3 movement;

        movement = (transform.forward * moveData.Vertical + transform.right * moveData.Horizontal).normalized;

        movement *= MoveSpeed;
        //}

        //print("transform right = " + transform.right);
        //Add moveSpeed onto movement.
        //If jumping move the character up one unit.
        if (moveData.Jump && _characterController.isGrounded)
        {
            //7f is our jump velocity.
            verticalVelocity = jumpVelocity;
            //if (!asServer && !replaying)
            // _jumpAudio.Play();
        }
        //if(_characterController.velocity.y < 0)

        //Subtract gravity from the vertical velocity.
        verticalVelocity += (gravity * delta);
        //Perhaps prevent the value from getting too low.
        verticalVelocity = Mathf.Max(MaxFallSpeed, verticalVelocity);
        //Add vertical velocity to the movement after movement is normalized.
        //You don't want to normalize the vertical velocity.
        movement += new Vector3(0f, verticalVelocity, 0f);


        _characterController.Move(movement * delta);


        if (!replaying && (asServer || base.IsHost)) 
        {
            WorldWrapCheck();
        }
        
        if(ResetQueued) 
        {
            //transform.position = transform.position + new Vector3(0, 10, 0);
            var spawnPointHolder = GameObject.Find("SpawnPointHolder").transform;
            transform.position = spawnPointHolder.GetChild(Random.Range(0, spawnPointHolder.childCount)).position;
            ResetQueued = false;
        }
    }

    /// <summary>
    /// Resets the client to ReconcileData.
    /// </summary>
    [Reconcile]
    private void Reconcile(ReconcileData recData, bool asServer, Channel channel = Channel.Unreliable)
    {
        /* Reset the client to the received position. It's okay to do this
         * even if there is no de-synchronization. */
        transform.position = recData.Position;
        transform.rotation = recData.Rotation;
        cameraHolder.rotation = recData.CameraHolderRotation;
        verticalVelocity = recData.VerticalVelocity;
    }
    private void WorldWrapCheck()
    {
        float halfRepeatSpacingX = repeatSpacingX / 2;
        float halfRepeatSpacingY = repeatSpacingY / 2;
        float halfRepeatSpacingZ = repeatSpacingZ / 2;

        if (transform.position.x > halfRepeatSpacingX)
        {
            transform.position -= new Vector3(repeatSpacingX, 0f, 0f);
            UpdateTimesWorldWrapped();
        }
        else if (transform.position.x < -halfRepeatSpacingX)
        {
            transform.position += new Vector3(repeatSpacingX, 0f, 0f);
            UpdateTimesWorldWrapped();
        }

        if (transform.position.y > halfRepeatSpacingY)
        {
            transform.position -= new Vector3(0f, repeatSpacingY, 0f);
            UpdateTimesWorldWrapped();
        }
        else if (transform.position.y < -halfRepeatSpacingY)
        {
            transform.position += new Vector3(0f, repeatSpacingY, 0f);
            UpdateTimesWorldWrapped();
        }

        if (transform.position.z > halfRepeatSpacingZ)
        {
            transform.position -= new Vector3(0f, 0f, repeatSpacingZ);
            UpdateTimesWorldWrapped();
        }
        else if (transform.position.z < -halfRepeatSpacingZ)
        {
            transform.position += new Vector3(0f, 0f, repeatSpacingZ);
            UpdateTimesWorldWrapped();
        }
    }

    private void UpdateTimesWorldWrapped()
    {
        if(!base.IsServer)
            return;

        serverManager.UpdateTimesWorldWrapped(OwnerId);
    }
}

