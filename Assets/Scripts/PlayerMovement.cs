using UnityEngine;
using UnityEditor.UI;
using UnityEngine.UI;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    public float groundDrag;
    [Header("Ground check")]

    public LayerMask groundLayer;
    public bool isGrounded;
    public float playerHeight;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    [Header("Sprint Settings")]
    public float speedWhenSprinting;
    public float speedWhenWalking;

    public float currentSpeedBarValue;

    public float sprintSpeedDrainRate;

    public Slider sprintBar;
    public GameObject fill;

    [Header("Sprint Recharge Settings")]

    bool canSprint;
    bool isSprinting;

    public float normalRechargeRate;
    public float tiredRechargeRate;

    [Header("Jump Settings")]

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    bool quetedToJump;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        //ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);

        PlayerInput();
        SpeedControl();

        // ground drag application
        if (isGrounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();

        if(quetedToJump)
        {
            //quetedToJump = false;
            //Jump();
            //Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    public void PlayerInput()
    {
        //movement input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //jump
        if (Input.GetKeyDown(KeyCode.Space)  && isGrounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }





        //sprint input
        if (Input.GetKey(KeyCode.LeftShift) && currentSpeedBarValue > 0 && canSprint)
        {
            moveSpeed = speedWhenSprinting;

            currentSpeedBarValue -= sprintSpeedDrainRate * Time.deltaTime;
            sprintBar.value = currentSpeedBarValue;
            isSprinting = true;

            if (currentSpeedBarValue < 0)
            {
                StartCoroutine(FillRecharge(false));
                fill.SetActive(false);
                currentSpeedBarValue = 0;
            }
            else
            {
                fill.SetActive(true);
            }
        }
        else
        {
            isSprinting = false;
            moveSpeed = speedWhenWalking;
            StartCoroutine(FillRecharge(true));
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (isGrounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    IEnumerator FillRecharge(bool shouldSprint)
    {

        yield return new WaitForSeconds(1f);
        canSprint = shouldSprint;
        if (canSprint)
        {
            while (currentSpeedBarValue < 100)
            {
                if (isSprinting)
                {
                    yield break;
                }
                Debug.Log("Normal Recharge");
                fill.SetActive(true);
                currentSpeedBarValue += normalRechargeRate * Time.deltaTime;
                sprintBar.value = currentSpeedBarValue;
                yield return null;
            }
        }
        else
        {
            while (currentSpeedBarValue < 100)
            {
                Debug.Log("Tired Recharge");
                fill.SetActive(true);
                currentSpeedBarValue += tiredRechargeRate * Time.deltaTime;
                sprintBar.value = currentSpeedBarValue;
                yield return null;

            }
            canSprint = true;
        }

    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        
        rb.linearVelocity += Vector3.up * jumpForce;
        //rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        Debug.Log(rb.linearVelocity);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

}
