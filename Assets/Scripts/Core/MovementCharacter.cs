using UnityEngine;

public class MovementCharacter : MonoBehaviour
{
    public Rigidbody2D rb;
    public float speed = 5f;    
    public float runSpeed = 8f; // 🔹 Nueva velocidad al correr
    public float jumpForce = 5f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public float lowMultiplier = 0.5f;
    public float fallMultiplier = 2.5f;
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    public float prebuffTime = 0.2f;
    private float prebuffTimeCounter;
    public bool jumping;
    private float originalGravityScale;

    void Start()
    {
        originalGravityScale = rb.gravityScale;
    }

    void Update()
    {
        float xInput = Input.GetAxis("Horizontal");

        // 🔹 Detectar si está corriendo (Shift + Grounded)
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && IsGrounded();

        // 🔹 Elegir velocidad
        float currentSpeed = isRunning ? runSpeed : speed;

        rb.linearVelocity = new Vector2(xInput * currentSpeed, rb.linearVelocity.y);

        if(rb.linearVelocityY < 0)
        {
            rb.gravityScale = originalGravityScale * fallMultiplier;
        }
        else if(rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.gravityScale = originalGravityScale * lowMultiplier;
        }
        else
        {
            rb.gravityScale = originalGravityScale;
        }        

        if(prebuffTimeCounter > 0)
        {
            prebuffTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            prebuffTimeCounter = prebuffTime;
        }               
      
        if (IsGrounded() && jumping && rb.linearVelocityY < 0)        
        {
            coyoteTimeCounter = 0; 
            jumping = false;            
        }
        if(!IsGrounded() && !jumping && coyoteTimeCounter <= 0)
        {
            coyoteTimeCounter = coyoteTime;
        }
        if(coyoteTimeCounter > 0)
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (prebuffTimeCounter > 0 && !jumping && IsGrounded() || 
            coyoteTimeCounter > 0 && !jumping && Input.GetButtonDown("Jump"))
        {
           Jump();
        }
    }

    public void Jump()
    {
        jumping = true;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        coyoteTimeCounter = 0;
        prebuffTimeCounter = 0;
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 
        groundCheckRadius, LayerMask.GetMask("Ground"));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}