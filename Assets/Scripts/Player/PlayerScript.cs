using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{

    [Header("Basic Movement")]
    private PlayerInput playerInput;
    [SerializeField] private float moveSpeed;
    private float moveInput;
    [SerializeField] private Rigidbody2D rb;
    private bool isFacingRight = true;

    [Header("Jump")]
    [SerializeField] private Transform groundPos;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheck;
    [SerializeField] private float jumpingPower, doubleJumpingPower;
    private bool isGrounded;
    [SerializeField] private float normalGravity, modifiedGravity;
    private bool doubleJump;
    [SerializeField] private bool doubleJumpEnabled;


    [Header("Dash")]
    [SerializeField] private float dashingPower;
    [SerializeField] private float dashingTime;
    [SerializeField] private float dashingCooldownTime;
    private bool isDashing;
    private bool canDash;
    [SerializeField] private TrailRenderer ren;


    [Header("Death")]
    private bool isDead;

    [Header("Camera Shake")]
    [SerializeField] private float shakeIntensity = 3f;
    [SerializeField] private float shakeTime = 0.23f;
    private GameObject cinemachineCamera;
    private CinemachineVirtualCamera vCam;
    CinemachineBasicMultiChannelPerlin cbmcp;

    [Header("Misc")]
    [SerializeField] private bool gizmoDraw;


    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        isDead = false;
        rb.gravityScale = normalGravity;
        canDash = true;





        #region CameraShake
        cinemachineCamera = GameObject.FindGameObjectWithTag("Cinemachine");
        vCam = cinemachineCamera.GetComponent<CinemachineVirtualCamera>();
        cbmcp = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cbmcp.m_AmplitudeGain = 0;
        #endregion
    }

    private void Update()
    {
        #region Checks
        if (isDashing)
        {
            return;
        }


        if (Physics2D.OverlapBox(groundPos.position, groundCheck, 0, groundLayer))
        {
            isGrounded = true;
        }
        else isGrounded = false;

        if (isGrounded && !playerInput.actions["Jump"].inProgress) { doubleJump = false; }

        #endregion

        #region Run

        moveInput = playerInput.actions["Movement"].ReadValue<float>();
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        #endregion

        #region Jump

        if (playerInput.actions["Jump"].triggered && !isDead)
        {
            if (doubleJumpEnabled)
            {
                if (isGrounded || doubleJump)
                {
                    rb.velocity = new Vector2(rb.velocity.x, doubleJump ? doubleJumpingPower : jumpingPower);
                    doubleJump = !doubleJump;
                }
            }
            else if (!doubleJumpEnabled)
            {
                if (isGrounded)
                {
                    rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
                    doubleJump = !doubleJump;
                }
            }
        }

        #endregion

        #region Dash
        if (playerInput.actions["Dash"].triggered && canDash && !isDead)
        {
            StartCoroutine(Dash());
            StartCoroutine(CameraShake());
        }
        #endregion

    }

    private void FixedUpdate()
    {
        if (isFacingRight == false && moveInput > 0)
        {
            Flip();
        }
        else if (isFacingRight == true && moveInput < 0)
        {
            Flip();
        }

        if (!isDashing)
        {
            if (rb.velocity.y >= 0f)
            {
                rb.gravityScale = normalGravity; //ResetingVelocity
            }
            else if (rb.velocity.y < 0f)
            {
                rb.gravityScale = modifiedGravity; //FasterFalling

                if (rb.velocity.y < -20f) //TerminalVelocity
                {
                    rb.velocity = new Vector2(rb.velocity.x, -25f);
                }
            }
        }

    }

    private void Flip()
    {
        if ((isFacingRight && moveInput < 0f) || (!isFacingRight && moveInput > 0f))
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = normalGravity;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        ren.emitting = true;
        doubleJump = true;
        yield return new WaitForSeconds(dashingTime);

        ren.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldownTime);

        canDash = true;
    }

    #region CamShake
    IEnumerator CameraShake()
    {
        cbmcp.m_AmplitudeGain = shakeIntensity;
        yield return new WaitForSeconds(shakeTime);

        cbmcp.m_AmplitudeGain = 0;
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (gizmoDraw)
        {
            Gizmos.DrawWireCube(groundPos.position, groundCheck);
        }
    }
}
