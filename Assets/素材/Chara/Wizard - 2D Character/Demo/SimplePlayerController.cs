using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimplePlayerController : MonoBehaviour
{
    public float movePower = 10f;
    public float jumpPower = 15f;
    public float GroundCheckLen = 0.1f;
    public float jumpSpeedMul = 0.5f;

    private Rigidbody2D rb;
    private Animator anim;
    private int direction = 1;
    private bool isJumping = false;
    private bool isOnGround = false;
    private bool alive = true;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }


    private void Update()
    {
        
        Restart();
        if (alive)
        {
            Hurt();
            Die();
            Attack();
            Jump();
            Run();

        }
    }
    void FixedUpdate()
    {
        // 1：尝试制作能 只往上台阶，不能下来。可行，但会有瞬间卡顿，找别的方法
        Vector2 pointA = transform.position;
        Vector2 pointB = transform.position - new Vector3(0, GroundCheckLen, 0);
        Debug.DrawLine(transform.position, transform.position - new Vector3(0, GroundCheckLen, 0), Color.white,  10.0f, true);
        bool banded = Physics2D.Linecast(pointA, pointB, 1 << LayerMask.NameToLayer("Ground"));
        bool ignore = !banded;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Ground"), ignore);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            if (anim.GetBool("isJump") && rb.velocity.SqrMagnitude() < 0.1f)
                anim.SetBool("isJump", false);
        }

    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            if (anim.GetBool("isJump") && rb.velocity.SqrMagnitude() < 0.1f)
                anim.SetBool("isJump", false);
        }

    }


    void Run()
    {
        Vector3 moveVelocity = Vector3.zero;
        anim.SetBool("isRun", false);


        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            direction = -1;
            moveVelocity = Vector3.left;

            transform.localScale = new Vector3(direction, 1, 1);
            if (!anim.GetBool("isJump"))
                anim.SetBool("isRun", true);

        }
        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            direction = 1;
            moveVelocity = Vector3.right;

            transform.localScale = new Vector3(direction, 1, 1);
            if (!anim.GetBool("isJump"))
                anim.SetBool("isRun", true);

        }
        if (isJumping) moveVelocity *= jumpSpeedMul;

        rb.AddForce(moveVelocity, ForceMode2D.Impulse);
        //transform.position += moveVelocity * movePower * Time.deltaTime;
    }
    void Jump()
    {
        if ((Input.GetButtonDown("Jump") || Input.GetAxisRaw("Vertical") > 0)
        && !anim.GetBool("isJump"))
        {
            isJumping = true;
            anim.SetBool("isJump", true);
        }
        if (!isJumping)
        {
            return;
        }

        rb.velocity = Vector2.zero;

        Vector2 jumpVelocity = new Vector2(0, jumpPower);
        rb.AddForce(jumpVelocity, ForceMode2D.Impulse);

      isJumping = false;
    }
    void Attack()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            anim.SetTrigger("attack");
        }
    }
    void Hurt()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            anim.SetTrigger("hurt");
            if (direction == 1)
                rb.AddForce(new Vector2(-5f, 1f), ForceMode2D.Impulse);
            else
                rb.AddForce(new Vector2(5f, 1f), ForceMode2D.Impulse);
        }
    }
    void Die()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            anim.SetTrigger("die");
            alive = false;
        }
    }
    void Restart()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            anim.SetTrigger("idle");
            alive = true;
        }
    }
}
