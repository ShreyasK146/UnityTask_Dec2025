using System.Collections;
using TMPro;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private bool isGrounded = false;
    private float runSpeed = 5;
    private float jumpHeight = 5;
    public float mouseSensitivity = 50f;
    private float pitch = 0f;    
    private float yaw = 0f;
    [SerializeField] private TextMeshProUGUI gameEndText;
    [SerializeField] private Transform cam;
    [SerializeField] private Transform hologram;
    [SerializeField] private Transform headPoint;
    [SerializeField] private TextMeshProUGUI cubeCollectedCount;
    private float yRotation;
    Animator animator;
    private Vector3 gravityDir = Vector3.down;
    Vector3 nextGravityDir = Vector3.zero;
    private float timeToDeath = 5f;
    private Rigidbody rb;
    private float horizontalInput, verticalInput;
    private bool jumpPressed = false;
    public float previewSlerpSpeed = 12f;
    public float snapOffset = 0.5f;
    public int totalCollectableCount = 0;
    public int collectedCount = 0;
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
        HandleInput();
        if (collectedCount == totalCollectableCount)
        {
            gameEndText.text = "YOU WIN";
            gameEndText.gameObject.SetActive(true);
            Time.timeScale = 0f;
        }


    }

    private void FixedUpdate()
    {
        Debug.Log("gravityDir = " + gravityDir);
        transform.rotation = gravityAlign * Quaternion.Euler(0f, yRotation, 0f);
       

        HandleCharacterMovement();
        HandleMouseLook();
        HanldeGravity();
        HandleJump();
        UpdateAnimator();
        CheckFreeFall();
        

    }

    #region Input related code
    private void HandleJump()
    {
        if (isGrounded && jumpPressed)
        {
            Debug.Log("trying to jump");
            rb.AddForce(-gravityDir * jumpHeight, ForceMode.Impulse);
            isGrounded = false; jumpPressed = false;
        }
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
            jumpPressed = true;

        ChangeGravityDirection();

    }
    #endregion

    #region Movement related code. Was unable to setup left and right movement in time so i had to just stick with front and back movement for now
    private void HandleCharacterMovement()
    {
        Vector3 gravityForward = transform.forward;
        Vector3 gravityRight = transform.right;

        Vector3 moveDirection = gravityForward * verticalInput + gravityRight * horizontalInput;
        moveDirection.Normalize();
        rb.MovePosition(rb.position + moveDirection * runSpeed * Time.deltaTime);

    }
    #endregion

    #region Camera Handle (Tried Cinemachine to handle camera couldn't make it work when gravity was changing) :
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80f, 80f);
        cam.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
    #endregion

    #region Gravity Handling
    private void HanldeGravity()
    {

        Vector3 gravityDown = gravityDir.normalized;
        Vector3 rayStart = transform.position - gravityDown * 0.2f;
        isGrounded = Physics.Raycast(rayStart, gravityDown, out RaycastHit hit, 0.4f);

    }

    private void ApplyGravityChange()
    {


        transform.position = hologram.position;
        rb.position = hologram.position;//might be extra

        transform.rotation = hologram.rotation;
        rb.rotation = hologram.rotation;//might be extra

        rb.velocity = Vector3.zero;//might be extra

        yRotation = transform.eulerAngles.y;
        Physics.gravity = nextGravityDir.normalized * Physics.gravity.magnitude;//ex : 9.81 * (0,0,1)
        gravityAlign = Quaternion.FromToRotation(Vector3.up, -nextGravityDir);//change character standing direction to opposite of its gravity
        gravityDir = nextGravityDir.normalized;
    }

    private void ChangeGravityDirection()
    {
        if (isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                nextGravityDir = -transform.right;
                ShowHologramPreview(nextGravityDir);
            }


            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                nextGravityDir = transform.right;
                ShowHologramPreview(nextGravityDir);
            }


            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                nextGravityDir = transform.forward;
                ShowHologramPreview(nextGravityDir);
            }


            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                nextGravityDir = -transform.forward;
                ShowHologramPreview(nextGravityDir);
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                ApplyGravityChange();
                hologram.gameObject.SetActive(false);
            }
        }


    }
    //chatgpt starts 
    private float hologramHeadOffset = 0.0f; 

    
    private void ShowHologramPreview(Vector3 gravityDir)
    {
        hologram.gameObject.SetActive(true);

       
        hologram.position = headPoint.position;

        Vector3 desiredUp = -gravityDir.normalized; // opposite to gravity

        
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, desiredUp);

  
        if (forward.sqrMagnitude < 1e-6f) // if gravity is acting in same direction as transform.forward the projection may fail so if its close to 0 we go from transform.forward to transform.up
            forward = Vector3.ProjectOnPlane(transform.up, desiredUp);

        forward = forward.normalized;

        hologram.rotation = Quaternion.LookRotation(forward, desiredUp);

     
        if (Mathf.Abs(hologramHeadOffset) > 1e-6f) // hologram offset 
            hologram.position += forward * hologramHeadOffset;

        StartCoroutine(WAIT());
    }
    //chatgpt ends

    private IEnumerator WAIT()
    {
        yield return new WaitForSeconds(1);
        hologram.gameObject.SetActive(false);
    }
    #endregion

    #region Animation
    private void UpdateAnimator()
    {
        if (isGrounded && (horizontalInput != 0 || verticalInput != 0))
        {
            animator.SetBool("isFalling", false);
            animator.SetBool("isRunning", true);
        }
        else if(isGrounded)
        {
            animator.SetBool("isFalling", false);
            animator.SetBool("isRunning", false);
        }
        else if(!isGrounded)
        {
            animator.SetBool("isFalling", true);
        }
    }
    #endregion

    #region Free fall check
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
            timeToDeath = 5;
        }
    }
    #endregion

    #region Collectible handler
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "collectable")
        {
            Destroy(other.gameObject);
            cubeCollectedCount.text = ++collectedCount + "";
        }
    }
    #endregion

}