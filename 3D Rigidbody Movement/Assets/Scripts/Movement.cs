using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    #region Config
    [Header("---- Input Settings ----")]
    [SerializeField] private KeyCode key_Jump = KeyCode.Space;
    [SerializeField] private KeyCode key_Run= KeyCode.LeftShift;

    [SerializeField] private string axis_Zoom = "Mouse ScrollWheel";
    [SerializeField] private string axis_Look_X = "Mouse X";
    [SerializeField] private string axis_Look_Y = "Mouse Y";
    [SerializeField] private string axis_Horizontal = "Horizontal";
    [SerializeField] private string axis_Vertical = "Vertical";

    [Header("---- Camera Perspective Settings ----")]

    [Header("Rotation Settings")]
    [SerializeField] private bool rotation_Stun = false;
    [Min(1f)] [SerializeField] private float rotation_Sensitivity_X = 350f;
    [Min(1f)] [SerializeField] private float rotation_Sensitivity_Y = 350f;

    [Range(-180f, 0f)] [SerializeField] private float rotation_MinEuler_X = -90f;
    [Range(0f, 180f)] [SerializeField] private float rotation_MaxEuler_Y = 90f;

    [Header("Camera Settings")]
    [SerializeField] private float camera_Distance = 0f; //0 = First Person =>  0 < Third Person 
    [SerializeField] private float camera_Hight = 0.5f;

    [Header("Zoom Settings")]
    [SerializeField] private bool zoom_Able = true;
    [SerializeField] private float zoom_Min = 0f; //Less or equal distance
    [SerializeField] private float zoom_Max = 15f; //More or equal distance
    [Min(1f)] [SerializeField] private float zoom_Sensitivity = 10f;

    [Header("Culling Settings")]
    [SerializeField] private bool culling_Able = true;
    [Range(0f,1f)] [SerializeField] private float culling_Multiplikator = 0.8f;
    [Min(1f)] [SerializeField] private float culling_Sensitivity = 50f;

    //End Camera Perspective Settings

    [Header("---- Movement Settings ----")]

    [SerializeField] private bool move_Stun = false;
    [Min(0.1f)] [SerializeField] private float move_Speed = 5f;

    [Header("Drag Settings")]
    [Min(0.01f)] [SerializeField] private float drag = 5f;
    [SerializeField] private float drag_Player_Hight = 1f; //Player hight to get ground = half player colider 
    [Min(0.01f)] [SerializeField] private float drag_Air_Multiplikator = 0.4f; //Miltiplikation of drag in air

    [Header("Run Settings")]
    [SerializeField] private bool run_Able = true; 
    [SerializeField] private float run_Speed = 8f; //Need to be more then speed

    [Header("Jump Settings")]
    [SerializeField] private bool jump_Able = true;
    [Min(1f)] [SerializeField] private float jump_Force = 7f;
    [Min(0.1f)] [SerializeField] private float jump_CoolDown = 0.25f;
    #endregion

    #region Fix
    private Transform transform_Camera_Parent;
    private Transform transform_Camera;
    private Rigidbody rb;
    private bool _Jump_Ready = true;
    private bool _Grounded;
    private float _Rotation_X;
    private float _Rotation_Y;
    private float _Input_Horizontal;
    private float _Input_Vertical;
    private float _Speed;
    #endregion

    #region Start
    void Start()
    {
        Get_Set_NeededObjects();
    }
    void Get_Set_NeededObjects()
    {
        transform_Camera_Parent = new GameObject().transform;
        transform_Camera_Parent.name = "Camera Holder";
        transform_Camera = Camera.main.transform;
        transform_Camera.parent = transform_Camera_Parent;

        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.freezeRotation = true;

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.transform.parent = this.transform;
        player.transform.localPosition = Vector3.zero;
    }
    #endregion

    #region Update
    void Update()
    {
        Check_Ground();
        if (culling_Able) Prevent_Culling();
        Update_InputMode();
        Update_SpeedLimit();
    }

    void Check_Ground()
    {
        _Grounded = Physics.Raycast(transform.position, Vector3.down, drag_Player_Hight + 0.1f);

        if (_Grounded) rb.drag = drag;
        else rb.drag = 0f;
    }

    void Prevent_Culling()
    {
        RaycastHit hit;
        float distance = camera_Distance;
        if (Physics.Linecast(transform.position, transform_Camera.TransformPoint(transform_Camera.localPosition * zoom_Max), out hit))
        {
            distance = hit.distance * culling_Multiplikator;
        }

        SetCameraPosition(distance, culling_Sensitivity);
    }

    #region Update Input
    void Update_InputMode()
    {
        if (!rotation_Stun)
        {
            Rotate_CameraWithPlayer(Input.GetAxisRaw(axis_Look_X), Input.GetAxisRaw(axis_Look_Y));
            if (zoom_Able & Input.GetAxis(axis_Zoom) != 0f) Zoom_Perspective();
        }

        if (!move_Stun)
        {
            Set_TransformInput(Input.GetAxisRaw(axis_Horizontal), Input.GetAxisRaw(axis_Vertical));
            if (_Grounded) Change_MaxSpeed(Input.GetKey(key_Run));
            if (jump_Able) Jump(Input.GetKey(key_Jump));
        }
    }

    void Rotate_CameraWithPlayer(float xImp, float yImp)
    {
        transform_Camera_Parent.position = new Vector3(transform.position.x, transform.position.y + camera_Hight, transform.position.z);

        _Rotation_Y += xImp * Time.deltaTime * rotation_Sensitivity_X;
        _Rotation_X = Mathf.Clamp((_Rotation_X - yImp * Time.deltaTime * rotation_Sensitivity_Y), rotation_MinEuler_X, rotation_MaxEuler_Y);

        transform_Camera_Parent.rotation = Quaternion.Euler(_Rotation_X, _Rotation_Y, 0f);
        transform.rotation = Quaternion.Euler(0, _Rotation_Y, 0);
    }

    void Zoom_Perspective()
    {
        float i = -transform_Camera.localPosition.z + (Input.GetAxis(axis_Zoom) * zoom_Sensitivity);
        camera_Distance = Mathf.Clamp(i, zoom_Min, zoom_Max);
        if (!culling_Able) SetCameraPosition(camera_Distance, zoom_Sensitivity);
    }

    void Set_TransformInput(float horizontalImp, float verticalImp)
    {
        _Input_Horizontal = horizontalImp;
        _Input_Vertical = verticalImp;
    }

    void Change_MaxSpeed(bool run)
    {
        if (run & run_Able) _Speed = run_Speed;
        else _Speed = move_Speed;
    }

    void Jump(bool jump)
    {
        if (jump & _Jump_Ready & _Grounded)
        {
            _Jump_Ready = false;
            rb.velocity = Vector3.zero;
            rb.AddForce(transform.up * jump_Force, ForceMode.Impulse);
            Invoke(nameof(ResetJump), jump_CoolDown);
        }
    }

    public void ResetJump()
    {
        _Jump_Ready = true;
    }
    #endregion

    void Update_SpeedLimit()
    {
        Vector3 velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (velocity.magnitude > _Speed)
        {
            Vector3 limit = velocity.normalized * _Speed;
            rb.velocity = new Vector3(limit.x, rb.velocity.y, limit.z);
            Debug.Log("Speed: " + limit.magnitude);
        }
    }

    void SetCameraPosition(float distance, float sensitivity)
    {
        distance = Mathf.Clamp(-distance, -camera_Distance, -zoom_Min);
        transform_Camera.localPosition = Vector3.Lerp(transform_Camera.localPosition, new Vector3(0, 0, distance), Time.deltaTime * sensitivity);
    }
    #endregion

    #region FixedUpdate 
    private void FixedUpdate()
    {
        if (!move_Stun) Transform_Player();
    }

    void Transform_Player()
    {
        Vector3 move_Direction = transform.forward * _Input_Vertical + transform.right * _Input_Horizontal;
        if (_Grounded) move_Direction = move_Direction.normalized * _Speed *10;
        else move_Direction = move_Direction.normalized * _Speed *10 * drag_Air_Multiplikator;

        rb.AddForce(move_Direction, ForceMode.Force);
    }
    #endregion
}