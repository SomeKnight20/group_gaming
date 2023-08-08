using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Animator animator;
    private Rigidbody2D player;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    float horizontalInput;
    float verticalInput;

    private Vector2 movement;
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    public bool isFlying = false;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        if (Input.GetKeyDown("space") && IsGrounded()){
            player.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        if (Input.GetKeyUp("space") && player.velocity.y > 0f){
            player.velocity = new Vector2(movement.x, player.velocity.y * 0.5f);
        }
        
        


        // movement.x = horizontalInput;
        movement.x = horizontalInput * moveSpeed;

        if (horizontalInput > 0){
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (horizontalInput < 0){
            transform.localScale = new Vector3(1, 1, 1);
        }

        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetFloat("FallingSpeed", player.velocity.y);
        animator.SetBool("IsAttacking", Input.GetKey("e"));
        animator.SetBool("IsSitting", Input.GetKey("left shift"));
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
}
