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
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform gun;

    private Vector2 moveInput;
    private bool jumpInput;

    private Rigidbody2D myRigidbody;
    private Animator myAnimator;
    private CapsuleCollider2D myBodyCollider;
    private BoxCollider2D myFeetCollider;

    private bool wasGrounded;
    private bool jumpAllowed;

    private bool isAlive = true;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myRigidbody.simulated = true;
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeetCollider = GetComponent<BoxCollider2D>();
    }

    void ResetAnimator()
    {
        myAnimator.SetBool("IsRunning", false);
        myAnimator.SetBool("IsClimbing", false);
    }

    void Update()
    {
        if (!isAlive)
        {
            return;
        }

        ResetAnimator();
        Run();
        FlipSprite();
        ClimbLadder();
        CheckGrounded();
        Jump();
        Die();
    }

    private void CheckGrounded()
    {
        wasGrounded |= Physics2D.Raycast(transform.position, Vector2.down, .03f, groundLayer);
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
        if (myFeetCollider.IsTouchingLayers(jumpLayer) && wasGrounded)
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
        if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ladder")) ||
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

    void Die()
    {
        if (isAlive && myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemies", "Hazards")))
        {
            isAlive = false;
            myAnimator.SetTrigger("Dying");
            FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }
    }


    #region Input

    void OnFire(InputValue value)
    {
        if (!isAlive)
        {
            return;
        }

        Instantiate(bullet, gun.position, transform.rotation);
    }

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