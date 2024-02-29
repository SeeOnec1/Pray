using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [Header("Basic Movement")]
    private PlayerControls playerControls;
    [SerializeField] private float moveSpeed;
    private float moveInput;
    [SerializeField] private Rigidbody2D rb;
    private bool isFacingRight = true;
    [SerializeField] private Transform groundPos;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private float jumpingPower;
    private bool isGrounded;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Start()
    {

    }

    private void Update()
    {

        #region Checks

        if (Physics2D.OverlapCircle(groundPos.position, groundCheckRadius, groundLayer))
        {
            isGrounded = true;
        }
        else isGrounded = false;

        #endregion

        #region Run

        moveInput = playerControls.Player.Movement.ReadValue<float>();
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        #endregion

    }
}
