


using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

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
    private float timeToDeath = 100000f;
    private Rigidbody rb;
    private float horizontalInput, verticalInput;
    private float turnSmoothVelocity = 0f;
    private float turnSmoothTime = 0.2f;
    private bool jumpPressed = false;
    public float previewSlerpSpeed = 12f;
    public float snapOffset = 0.5f; // how far player sits behind hologram when snapping
    //private bool test = true;
    public int totalCollectableCount = 0;
    //private bool freezeframe = false;
    //bool lasttest = false;
    private Quaternion gravityAlign;
    private void Start()
    {
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("collectable"))
        {
            totalCollectableCount++;
        }
        Debug.Log(totalCollectableCount);
    }

    private void Update()
    {
        // Show hologram always
        //Debug.Log("LIVE hologram pos: " + hologram.position);

        //if (freezeframe)
        //{
        //    //Debug.Log("During freezeframe — hologram pos: " + hologram.position);
        //    //Debug.Log("During freezeframe — rb pos: " + rb.position);
        //    return;   // <-- IMPORTANT
        //}

        // Only run these logs when NOT frozen
        //Debug.Log("UPDATE writing movement? pos before code: " + rb.position);

        //if (lasttest == true && test == false)
        //{
        //    //Debug.Log("TEST EXIT FRAME — rb pos: " + rb.position);
        //    //Debug.Log("TEST EXIT FRAME — transform pos: " + transform.position);
        //    //Debug.Log("TEST EXIT FRAME — hologram pos: " + hologram.position);
        //}

        //lasttest = test;

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
            //if (freezeframe)
            //{
            //    // NO LOGS HERE
            //    freezeframe = false;
            //    return;
            //}

        // Normal frame (not frozen)
        //Debug.Log("AFTER freezeframe — rb pos: " + rb.position);
        //Debug.Log("AFTER freezeframe — hologram pos: " + hologram.position);
        //Debug.Log("FIXEDUPDATE movement start pos: " + rb.position);

        HandleCharacterMovement();
        //HandleMouseMovement();
        HanldeGravity();
        HandleJump();
        UpdateAnimator();
        CheckFreeFall();
    }



    private void HandleCharacterMovement()
    {
        //if (freezeframe) return;
        //if (!test)
        //{
        //    Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
        //    moveDirection.y = 0f;
        //    moveDirection.Normalize();

        //    // Debug statement for moveDirection
        //    Debug.Log($"Move Direction: {moveDirection}");

        //    rb.MovePosition(rb.position + moveDirection * runSpeed * Time.deltaTime);

        //    float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;

        //    // Debug statement for targetAngle
        //    Debug.Log($"Target Angle: {targetAngle}");

        //    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

        //    // Debug statement for angle
        //    Debug.Log($"Smooth Angle: {angle}");

        //    transform.rotation = Quaternion.Euler(0f, angle, 0f);
        //    Vector2 movingDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

        //    // Debugging the final rotation
        //    Debug.Log($"Final Rotation: {transform.rotation}");
        //}
     
            Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
            moveDirection.y = 0f;
            moveDirection.Normalize();
            rb.MovePosition(rb.position + moveDirection * runSpeed * Time.deltaTime);
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = gravityAlign * Quaternion.Euler(0f, angle, 0f);


        Vector2 movingDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            
        
    }

    private void HandleMouseMovement()
    {
        //if (freezeframe) return;
        //if (!test)
        //{
        //    float mouseX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
        //    yRotation += mouseX;

        //    // Debug statement for yRotation before applying
        //    Debug.Log($"Mouse X: {mouseX}, Y Rotation Before: {yRotation}");

        //    transform.rotation = Quaternion.Euler(0, yRotation, 0);

        //    // Debugging the final rotation after mouse movement
        //    Debug.Log($"Final Rotation After Mouse Movement: {transform.rotation}");
        //}
        
            float mouseX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
            yRotation += mouseX;
        transform.rotation = gravityAlign * Quaternion.Euler(0, yRotation, 0);




    }

    private void HanldeGravity()
    {

        RaycastHit hit;
        if (Physics.Raycast(transform.position + new Vector3(0, 1f, 0), gravityDir, out hit, 1f))
        {
            //Debug.Log(hit.collider.name);
            isGrounded = true;
        }
        else
        {
            //Debug.Log("did not hit anything");
            isGrounded = false;
        }
        velocity += gravityDir * gravity * Time.deltaTime;
        //Debug.DrawRay(transform.position, gravityDir.normalized*1f, Color.red);


    }

    private void HandleJump()
    {
        if (isGrounded && jumpPressed)
        {
            Debug.Log("trying to jump");
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
            isGrounded = false; jumpPressed = false;
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
        // Left
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            nextGravityDir = -transform.right;
            ShowHologramPreview(nextGravityDir);
        }

        // Right
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            nextGravityDir = transform.right;
            ShowHologramPreview(nextGravityDir);
        }

        // Forward
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            nextGravityDir = transform.forward;
            ShowHologramPreview(nextGravityDir);
        }

        // Backwards
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            nextGravityDir = -transform.forward;
            ShowHologramPreview(nextGravityDir);
        }

        if (Input.GetKeyDown(KeyCode.Return))
            ApplyGravityChange();
    


}

private void ApplyGravityChange()
    {

        //Debug.Log("transform and hologram position before apply change :" + transform.position + "," + hologram.position);
        //Debug.Log("rb pos before apply change :"+ rb.position);
        transform.position = hologram.position; 
        rb.position = hologram.position;
        //Debug.Log("transform and hologram position after apply change :" + transform.position + "," + hologram.position);
        //Debug.Log("rb pos after apply change :" + rb.position);
        //Debug.Log("transform and hologram rotation before apply change :" + transform.rotation + "," + hologram.rotation);
        //Debug.Log("rb rot before apply change :" + rb.rotation);
        transform.rotation = hologram.rotation; 
        rb.rotation = hologram.rotation;
        //Debug.Log("transform and hologram rotation after apply change :" + transform.rotation + "," + hologram.rotation);
        //Debug.Log("rb rot after apply change :" + rb.rotation);
        rb.velocity = Vector3.zero;
        turnSmoothVelocity = 0f;
        yRotation = transform.eulerAngles.y;    
        Physics.gravity = nextGravityDir.normalized * Physics.gravity.magnitude;
        gravityAlign = Quaternion.FromToRotation(Vector3.up, -nextGravityDir);
        //test = false;
        //freezeframe = true; 
    }
    private void LateUpdate()
    {
        //if (freezeframe) freezeframe = false;
    }
    private void CheckFreeFall()
    {

        if (!isGrounded)
        {
            //Debug.Log(timeToDeath);
            timeToDeath -= 1f * Time.deltaTime;
            if (timeToDeath < 0)
            {
                gameEndText.gameObject.SetActive(true);
                Time.timeScale = 0f;
            }

        }
        else
        {
            timeToDeath = 100000;
        }
    }

    private float hologramHeadOffset = 0.0f; // tweak if head isn't aligned (units in meters)

    private void ShowHologramPreview(Vector3 gravityDir)
    {
        hologram.gameObject.SetActive(true);

        // Anchor hologram head to headPoint (you may tweak offset below)
        hologram.position = headPoint.position;

        Vector3 desiredUp = -gravityDir.normalized;

        // Primary forward: local forward projected onto plane of desiredUp
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, desiredUp);

        // If degenerate (forward parallel to desiredUp), use local up (head->feet) instead
        if (forward.sqrMagnitude < 1e-6f)
            forward = Vector3.ProjectOnPlane(transform.up, desiredUp);

        forward = forward.normalized;

        hologram.rotation = Quaternion.LookRotation(forward, desiredUp);

        // Optional: nudge hologram so its head sits at headPoint.
        // Adjust 'hologramHeadOffset' (positive moves hologram forward along 'forward').
        if (Mathf.Abs(hologramHeadOffset) > 1e-6f)
            hologram.position += forward * hologramHeadOffset;

        StartCoroutine(WAIT());
    }


    private IEnumerator WAIT()
    {
        yield return new WaitForSeconds(2);
        hologram.gameObject.SetActive(false);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "collectable")
        {
            Destroy(other.gameObject);
            totalCollectableCount--;
        }
    }
    
}