using UnityEngine;
public class movement: MonoBehaviour
{
    public float speed=2.5f;
    public float jump=5f;
    private float playerinput;
    private Rigidbody2D rb;
    public Transform groundcheck;
    public LayerMask groundLayer;
    public float rangecheck=0.2f;
    private bool isgrounded;

    void Start()
    {
        rb=GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        playerinput=Input.GetAxisRaw("Horizontal");
        if(Input.GetKeyDown(KeyCode.Space) && isgrounded)
        {
            rb.linearVelocity=new Vector2(rb.linearVelocity.x , jump);
        }
        if (playerinput > 0)
        {
            transform.localScale=new Vector3(1,1,1);
        }
        else if (playerinput < 0)
        {
            transform.localScale=new Vector3(-1,1,1);
        }
    }
    void FixedUpdate()
    {
        isgrounded=Physics2D.OverlapCircle(groundcheck.position , rangecheck , groundLayer);
        rb.linearVelocity=new Vector2(playerinput*speed , rb.linearVelocity.y);
    }
}