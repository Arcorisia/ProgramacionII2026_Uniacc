using UnityEngine;

public class MovementCharacter : MonoBehaviour
{
    public Rigidbody2D rb;
    public float speed = 5f;    
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

    // Update is called once per frame
    void Update()
    {
        float xInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(xInput * speed, rb.linearVelocity.y);

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

        // Prebuffer: registra intención de salto
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
            coyoteTimeCounter = coyoteTime; // ✅ Inicia contador de coyote time
        }
        if(coyoteTimeCounter > 0)
        {
            coyoteTimeCounter -= Time.deltaTime; // ✅ Decrementa coyote time
        }
       

        // Salto: prebuffer + coyote time combinados
        if (prebuffTimeCounter > 0 && !jumping && IsGrounded() || coyoteTimeCounter > 0 
        && !jumping && Input.GetButtonDown("Jump"))
        {
           Jump();
        }
    }

    public void Jump()
    {
        jumping = true;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // ✅ Resetea velocidad vertical
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        coyoteTimeCounter = 0;  // ✅ Evita doble salto
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
