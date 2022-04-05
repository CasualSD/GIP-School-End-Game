using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Transform orientation;

    [Header("References")]
    [SerializeField] WallRun wallRun;

    [Header("Player Movement")]
    public float movementSpeed = 6f;
    public float movementSpeedMultiplier = 10f;
    public float jumpForce = 12.5f;
    [SerializeField] float airMovementMultiplier = 0.1f;

    [Header("Player Sprinting")]
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float sprintSpeed = 6f;
    [SerializeField] float acceleration = 10f;

    [Header("Player Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Drag")]
    public float groundDrag = 6f;
    public float airDrag = 0.15f;

    [Header("Ground Detection")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundDistance = 0.1f;
    [SerializeField] Transform groundCheck;
    bool isGrounded;
    private bool canDoubleJump;
    

    float playerHeight = 2f;

    float verticalMovement;
    float horizontalMovement;

    float mouseX;
    float mouseY;

    Rigidbody rb;

    Vector3 movementDirection;
    Vector3 slopeMovementDirection;
    Vector3 mouseDirection;

    private bool wallLeft = false;
    private bool wallRight = false;
    [SerializeField] private float minimumJumpHeight = 1.5f;

    RaycastHit leftWallHit;
    RaycastHit rightWallHit;

    [SerializeField] private float wallDistance = .5f;

    bool CanWallRun()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight);
    }

    void CheckWall()
    {
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallDistance);
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallDistance);
    }


    RaycastHit slopeHit;
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded)
        {
            canDoubleJump = true;
        }

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();

        }
        else
        {
            if (Input.GetKeyDown(jumpKey) && canDoubleJump)
            {
                Jump();
                canDoubleJump = false;
            }
        }
        
        PlayerInput();
        ControlDrag();
        ControlSpeed();

        slopeMovementDirection = Vector3.ProjectOnPlane(movementDirection, slopeHit.normal);
    }

    void PlayerInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        mouseX = Input.GetAxis("Horizontal");
        mouseY = Input.GetAxis("Vertical");



        if (isGrounded)
        {
            movementDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
        }
        else if (!isGrounded)
        {
            movementDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
        }
    }

    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        if (wallLeft == false || wallRight == false)
        {
            StartCoroutine(ObserveJump());
        }
    }

    IEnumerator ObserveJump()
    {
            yield return new WaitForSeconds(0.5f);
            rb.AddForce(Vector3.down * 100, ForceMode.Acceleration);
            Debug.Log("backdown");
    }

    void ControlSpeed()
    {
        if(Input.GetKey(sprintKey) && isGrounded && rb.velocity.magnitude > 2)
        {
            movementSpeed = Mathf.Lerp(movementSpeed, sprintSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            movementSpeed = Mathf.Lerp(movementSpeed, walkSpeed, acceleration * Time.deltaTime);
        }
        if (wallRun.isWalled)
        {
            movementSpeed = Mathf.Lerp(movementSpeed, sprintSpeed * 2, acceleration * Time.deltaTime);
        }
    }

    void ControlDrag()
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else if (!isGrounded)
        {
            rb.drag = airDrag;
        }
    }

    private void FixedUpdate()
    {
        PlayerMove();
    }

    void PlayerMove()
    {
        if (isGrounded && !OnSlope())
        {
            rb.AddForce(movementDirection.normalized * movementSpeed * movementSpeedMultiplier, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeMovementDirection.normalized * movementSpeed * movementSpeedMultiplier, ForceMode.Acceleration);
        }
        else if (!isGrounded)
        {
            rb.AddForce(movementDirection.normalized * movementSpeed * 5 * airMovementMultiplier, ForceMode.Force);
            if(rb.velocity.magnitude > 25)
            {
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, 25);
            }
        }
        CheckWall();

        if (CanWallRun())
        {
            if (wallLeft)
            {
                rb.AddForce(movementDirection.normalized * movementSpeed * 17, ForceMode.Acceleration);
                Debug.Log("left");
            }
            else if (wallRight)
            {
                rb.AddForce(movementDirection.normalized * movementSpeed * 17, ForceMode.Acceleration);
                Debug.Log("right");
            }
        }
        //else if (wallRun.isWalled = true && !isGrounded)
        //{
        //    rb.AddForce(movementDirection.normalized * movementSpeed * movementSpeedMultiplier, ForceMode.Acceleration);
        //    Debug.Log("walled");
        //}

    }
}
