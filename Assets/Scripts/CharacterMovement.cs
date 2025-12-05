
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    
    private bool isGrounded = false;
    private float runSpeed = 5;
    private float jumpHeight = 5;
    private float mouseSens = 250f;
    [SerializeField] private TextMeshProUGUI gameEndText;
    [SerializeField] private Transform cam;
    [SerializeField] private Transform hologram;
    [SerializeField] private Transform headPoint;
    private float yRotation;
    Animator animator;
    private Vector3 velocity;
    private Vector3 gravityDir = Vector3.down;
    private float gravity = 9.81f;
    Vector3 nextGravityDir = Vector3.zero;
    private float timeToDeath = 10f;
    private Rigidbody rb;
    private float horizontalInput,verticalInput;
    private float turnSmoothVelocity = 0f;  
    private float turnSmoothTime = 0.2f;
    private bool jumpPressed = false;
    public float previewSlerpSpeed = 12f;
    public float snapOffset = 0.5f; // how far player sits behind hologram when snapping
    private bool isPreviewing = false; // true while player is cycling previews


    private void Start()
    {
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
    }   

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
            jumpPressed = true;

        ChangeGravityDirection();

    }

    private void FixedUpdate()
    {
        HandleCharacterMovement();
        HandleMouseMovement();
        HanldeGravity();
        HandleJump();
        UpdateAnimator();
        CheckFreeFall();
    }



    private void HandleCharacterMovement()
    {

        Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
        moveDirection.y = 0f;
        moveDirection.Normalize();


        rb.MovePosition(rb.position + moveDirection * runSpeed * Time.deltaTime);

        float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;

        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,turnSmoothTime);

        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        Vector2 movingDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

    }

    private void HandleMouseMovement()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;

        yRotation += mouseX;

        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void HanldeGravity()
    {

        RaycastHit hit;
        if (Physics.Raycast(transform.position+new Vector3(0,1f,0), gravityDir, out hit, 1f))
        {
            Debug.Log(hit.collider.name);
            isGrounded = true;
        }
        else
        {
            Debug.Log("did not hit anything");
            isGrounded = false;
        }
        velocity += gravityDir * gravity * Time.deltaTime;
        Debug.DrawRay(transform.position, gravityDir.normalized*1f, Color.red);
    }

    private void HandleJump()
    {
        if (isGrounded && jumpPressed)
        {
            Debug.Log("trying to jump");
            rb.AddForce(Vector3.up * jumpHeight,ForceMode.Impulse);
            isGrounded = false;jumpPressed = false;
        }
    }

    private void UpdateAnimator()
    {
        if (isGrounded && (horizontalInput != 0 || verticalInput != 0))
        {
            animator.SetBool("isRunning", true);
        }
        else 
        {
            animator.SetBool("isRunning", false);
        }
    }

    private void ChangeGravityDirection()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            nextGravityDir = -transform.right;      // player-left
            if (nextGravityDir != Vector3.zero)
                ShowHologramPreview(nextGravityDir);
        }


        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            nextGravityDir = transform.right;       // player-right
            if (nextGravityDir != Vector3.zero)
                ShowHologramPreview(nextGravityDir);
        }


        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            nextGravityDir = transform.up;          // player-up
            if (nextGravityDir != Vector3.zero)
                ShowHologramPreview(nextGravityDir);
        }


        //if (Input.GetKeyDown(KeyCode.DownArrow))
        //    nextGravityDir = -transform.up;         // player-down



        if (Input.GetKeyDown(KeyCode.Return))
            ApplyGravityChange();

    }

    private void ApplyGravityChange()
    {
        gravityDir = nextGravityDir.normalized;
        velocity = Vector3.zero;
        rb.velocity = Vector3.zero;

        // 1. Rotate so that player's UP = opposite of gravity
        Quaternion targetRot = Quaternion.FromToRotation(transform.up, -gravityDir) * transform.rotation;
        transform.rotation = targetRot;

        // 2. Move the player to hologram position
        transform.position = hologram.position;

        // 3. Optional: force small downward stick to ground
        rb.MovePosition(transform.position + gravityDir * 0.05f);

        
    }

    private void CheckFreeFall()
    {
        
        if (!isGrounded)
        {
            Debug.Log(timeToDeath);
            timeToDeath -= 1f * Time.deltaTime;
            if(timeToDeath < 0)
            {
                gameEndText.gameObject.SetActive(true);
                Time.timeScale = 0f;
            }
                
        }
        else
        {
            timeToDeath = 10;
        }
    }

    private void ShowHologramPreview(Vector3 gravityDir)
    {
        hologram.gameObject.SetActive(true);

        // 1. Position hologram at head
        hologram.position = headPoint.position;

        // 2. Hologram Up = opposite of new gravity direction
        Vector3 desiredUp = -gravityDir.normalized;

        // 3. Hologram Forward = player's forward direction projected on plane
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, desiredUp).normalized;

        // 4. Set rotation exactly (no smoothing)
        hologram.rotation = Quaternion.LookRotation(forward, desiredUp);

        StartCoroutine(WAIT());
    }


    private IEnumerator WAIT()
    {
        yield return new WaitForSeconds(2);
        hologram.gameObject.SetActive(false);
    }
}
