using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Animator animator;
    private Rigidbody2D player;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform ceilingCheck;


    public GameObject grabber;
    public Transform wallCheck;
    public Transform aboveWallCheck;
    private bool isGrabbingLedge = false;


    float horizontalInput;
    float verticalInput;

    private Vector2 movement;
    public float baseMoveSpeed = 5f;
    private float moveSpeed = 0f;
    public float jumpForce = 5f;

    public float coyoteTime = 5f;
    private float coyoteTimer = 0f;
    public float jumpBufferTime = 1f;
    private float jumpBufferTimer = 0f;

    public bool isFlying = false;
    

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Rigidbody2D>();
        grabber.SetActive(false);
        moveSpeed = baseMoveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //jumping
        if (!IsGrounded() && Input.GetKeyDown("space")){
            jumpBufferTimer = jumpBufferTime;
        }else if(jumpBufferTimer >= 0){
            jumpBufferTimer -= Time.deltaTime;
        }

        if ((IsGrounded() || isGrabbingLedge) && !Input.GetKey("space")){
            coyoteTimer = coyoteTime;
        }else if(coyoteTimer >= 0){
            coyoteTimer -= Time.deltaTime;
        }
        if (IsGrounded() || isGrabbingLedge || coyoteTimer >= 0){
            if(Input.GetKeyDown("space") || jumpBufferTimer >= 0){
                coyoteTimer = 0;
                jumpBufferTimer = 0;
                // player.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                player.velocity = new Vector2(movement.x, jumpForce);
            }
        }
        // if (Input.GetKeyDown("space") && IsGrounded() || Input.GetKeyDown("space") && isGrabbingLedge){
        //     // player.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        //     player.velocity = new Vector2(movement.x, jumpForce);
        // }
        if (Input.GetKeyUp("space") && player.velocity.y > 0f){
            player.velocity = new Vector2(movement.x, player.velocity.y * 0.5f);
        }


        //slow down walking speed
        if (Input.GetKey("left shift")){
            moveSpeed = baseMoveSpeed/2;
            animator.speed = 0.5f;
        }else{
            moveSpeed = baseMoveSpeed;
            animator.speed = 1;
        }

        //crawling
        if (verticalInput < 0  && IsGrounded() || CeilingCheck() && IsGrounded()){
            moveSpeed = baseMoveSpeed/2;
            animator.speed = 0;
            animator.SetBool("IsCrawling", true);
            if(horizontalInput != 0){
                animator.speed = 0.5f;
            }
        }else{
            animator.SetBool("IsCrawling", false);
            // animator.speed = 1;
        }

        //ledge grabbing
        if(WallCheck() && !AboveWallCheck() && verticalInput >= 0){
            grabber.SetActive(true);
            animator.SetBool("IsGrappingLedge", true);
            isGrabbingLedge = true;
        }else{
            grabber.SetActive(false);
            isGrabbingLedge = false;
            animator.SetBool("IsGrappingLedge", false);
        }
        

    
        movement.x = horizontalInput * moveSpeed;

        //flip player
        if (horizontalInput > 0){
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (horizontalInput < 0){
            transform.localScale = new Vector3(1, 1, 1);
        }

        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetFloat("FallingSpeed", player.velocity.y);
        // animator.SetBool("IsAttacking", Input.GetKey("e"));

        
    }

    void FixedUpdate()
    {
        // player.position += movement * Time.fixedDeltaTime;
        player.velocity = new Vector2(movement.x, player.velocity.y);
    }

    private bool IsGrounded()
    {
        if(isFlying){
            return true;
        }
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private bool CeilingCheck()
    {
        return Physics2D.OverlapCircle(ceilingCheck.position, 0.2f, groundLayer);
    }

    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheck.position, new Vector2(0.15f, 0.2f), 0, groundLayer);
    }
    private bool AboveWallCheck()
    {
        return Physics2D.OverlapBox(aboveWallCheck.position, new Vector2(0.15f, 0.2f), 0, groundLayer);
    }
    // void OnTriggerEnter2D(Collider2D collision)
    // {
    //     // if (collision.gameObject.CompareTag("WallCheck"))
    //     // {
    //         Debug.Log("WallCheck");
    //     // }
    // }
}
