using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovements : MonoBehaviour
{
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float jumpSpeed = 10f;
    [SerializeField] private float climbSpeed = 5f;
    [SerializeField] private LayerMask jumpLayer;
    [SerializeField] private LayerMask groundLayer;


    private Vector2 moveInput;
    private bool jumpInput;

    private Rigidbody2D myRigidbody;
    private Animator myAnimator;
    private CapsuleCollider2D myCapsuleCollider2D;

    private bool wasGrounded;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myCapsuleCollider2D = GetComponent<CapsuleCollider2D>();
    }

    void ResetAnimator()
    {
        myAnimator.SetBool("IsRunning", false);
        myAnimator.SetBool("IsClimbing", false);
    }
 
    void FixedUpdate()
    {
        wasGrounded |= Physics2D.Raycast(transform.position, Vector2.down, .05f, groundLayer);
    }

    void Update()
    {
        ResetAnimator();
        Run();
        FlipSprite();
        ClimbLadder();
        Jump();
    }


    void Run()
    {
        Vector2 playerVelocity = new Vector2(moveInput.x * runSpeed, myRigidbody.velocity.y);
        myRigidbody.velocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("IsRunning", playerHasHorizontalSpeed);
    }

    void Jump()
    {
        if (myCapsuleCollider2D.IsTouchingLayers(jumpLayer) && wasGrounded)
        {
            if (jumpInput)
            {
                wasGrounded = false;
                myRigidbody.velocity += new Vector2(0f, jumpSpeed);
            }
        }

        jumpInput = false;
    }

    void ClimbLadder()
    {
        if (!myCapsuleCollider2D.IsTouchingLayers(LayerMask.GetMask("Ladder")) ||
            Mathf.Approximately(moveInput.y, 0))
        {
            return;
        }

        if (myRigidbody.velocity.y > climbSpeed)
        {
            return;
        }

        Vector2 playerVelocity = new Vector2(myRigidbody.velocity.x, moveInput.y * climbSpeed);
        myRigidbody.velocity = playerVelocity;

        bool playerHasVerticalSpeed = Mathf.Abs(myRigidbody.velocity.y) > Mathf.Epsilon;
        myAnimator.SetBool("IsClimbing", playerHasVerticalSpeed);
    }

    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody.velocity.x), 1f);
        }
    }

    #region Input

    void OnJump(InputValue value)
    {
        jumpInput = value.isPressed;
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    #endregion
}