using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPlayerControllerRigidBody : CComponent
{
    #region Serielizefield
    [SerializeField] private GameObject playerCameraObj;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float gravity;
    #endregion

    #region public
    [Header("KeyBinding")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;
    #endregion

    Transform playerTransform;
    Rigidbody chellController;
    Vector3 playerDirection;
    float moveSpeed;
    Transform chellModel;
    Animator chellAnimator;
    Transform cameraTransform;

    Vector3 move;


    PlayerState playerState;
    enum PlayerState
    {
        walk,
        crouch,
        air
    }

    public override void Awake()
    {
        base.Awake();

        playerTransform = transform;
        chellModel = transform.GetChild(0);
        chellAnimator = chellModel.GetComponent<Animator>();
        chellController = GetComponent<Rigidbody>();
        cameraTransform = playerCameraObj.transform.GetChild(0);
    }

    public override void Start()
    {
        base.Start();

        moveSpeed = walkSpeed;
    }

    public override void Update()
    {
        base.Update();

        
        if (Physics.Raycast(playerTransform.position, Vector3.down, 3.1f))
        {
            GroundCheck();

            PlayerMove();
        }
        else
        {
            move.y -= gravity * Time.deltaTime;
            PlayerMove();
        }

        if (Physics.Raycast(playerTransform.position, Vector3.down, 3.1f) && Input.GetKeyDown(jumpKey))
        {
            chellController.velocity = new Vector3(chellController.velocity.x, 0f, chellController.velocity.z);
            chellController.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            chellAnimator.SetTrigger("aJump");
        }

        chellController.velocity = move;
    }

    public override void LateUpdate()
    {
        base.LateUpdate();

        playerCameraObj.transform.position = playerTransform.position;
    }

    private void PlayerMove()
    {
        float tempMoveY = move.y;

        move.y = 0f;

        Vector3 InputMoveXZ = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        chellAnimator.SetFloat("DirectionN", InputMoveXZ.z, 0.5f, Time.deltaTime);
        chellAnimator.SetFloat("DirectionE", InputMoveXZ.x, 0.5f, Time.deltaTime);

        float InputMoveXZMagnitude = InputMoveXZ.sqrMagnitude;
        InputMoveXZ = playerTransform.TransformDirection(InputMoveXZ);

        if (InputMoveXZMagnitude <= 1)
        {
            InputMoveXZ *= moveSpeed;
        }
        else
        {
            InputMoveXZ = InputMoveXZ.normalized * moveSpeed;
        }

        Quaternion cameraRot = cameraTransform.rotation;

        cameraRot.x = cameraRot.z = 0f;
        playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, cameraRot, 10.0f * Time.deltaTime);

        move = Vector3.MoveTowards(move, InputMoveXZ, moveSpeed);

        float speed = move.sqrMagnitude;
        chellAnimator.SetFloat("aSpeed", speed);

        move.y = tempMoveY;
    }

    private void GroundCheck()
    {
        if (Physics.Raycast(playerTransform.position, Vector3.down, 3.1f))
        {
            move.y = -5f;
        }
        else
        {
            move.y = -1f;
        }
    }
}