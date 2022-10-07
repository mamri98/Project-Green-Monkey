using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Collision coll;
    [HideInInspector]
    public Rigidbody2D rb;
    private Animator anim;

    [Space]
    [Header("Stats")]
    public float speed = 10;
    public float jumpForce = 50;
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;

    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool wallJumped;
    public bool wallSlide;
    public int side = 1;
    private bool facingRight;


    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        facingRight = true;
    }

    // Update is called once per frame
    void Update()
    {

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);

        Move(dir);

        anim.SetFloat("YVelocity", rb.velocity.y);
        anim.SetBool("Grounded", coll.onGround);
        anim.SetBool("WallSlide", wallSlide);

        if (coll.onWall && canMove)
        {
            wallSlide = false;
        }

        if (!coll.onWall || !canMove)
        {
            wallSlide = false;
        }

        if (coll.onGround)
        {
            wallJumped = false;
            GetComponent<BetterJumping>().enabled = true;
            
        }

        if (coll.onWall && !coll.onGround)
        {
            if (x != 0)
            {
                wallSlide = true;
                WallSlide();
            }
        }

        if (!coll.onWall || coll.onGround)
            wallSlide = false;

        if (Input.GetButtonDown("Jump"))
        {
            if (coll.onGround)
                Jump(Vector2.up, false);
            if (coll.onWall && !coll.onGround)
                WallJump();
        }

        if (wallSlide || !canMove)
            return;


        if ((x < 0 && facingRight) || (x > 0 && !facingRight))
        {
            facingRight = !facingRight;
            side *= -1;
            transform.Rotate(0f, 180f, 0f);
        }

    }

    private void WallJump()
    {
        if ((side == 1 && coll.onRightWall) || side == -1 && !coll.onRightWall)
        {
            side *= -1;
            facingRight = !facingRight;
            transform.Rotate(0f, 180f, 0f);

        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;

        Jump((Vector2.up / 1.5f + wallDir / 1.5f), true);

        wallJumped = true;
    }

    private void WallSlide()
    {
        if (!canMove)
            return;

        bool pushingWall = false;
        if ((rb.velocity.x > 0 && coll.onRightWall) || (rb.velocity.x < 0 && coll.onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : rb.velocity.x;

        rb.velocity = new Vector2(push, -slideSpeed);
    }

    private void Move(Vector2 dir)
    {
        if (!canMove)
            return;

        if (!wallJumped)
        {
            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
            anim.SetBool("Running", dir.x != 0);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }

    private void Jump(Vector2 dir, bool wall)
    {
        rb.velocity = Vector2.zero;
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += dir * jumpForce;
        anim.SetTrigger("Jump");
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }
}
