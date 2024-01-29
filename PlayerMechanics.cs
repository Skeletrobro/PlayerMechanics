using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMechanics : MonoBehaviour
{
    // Movement variables
    private Rigidbody2D rb;
    private BoxCollider2D bc;
    [SerializeField] private float jumpStrength = 16f;
    [SerializeField] private float speed = 6.5f;
    [SerializeField] private float normalSpeed = 6.5f;
    private float move;
    private bool grounded = true;
    private bool jumped;
    private bool facingRight = true;
    private SpriteRenderer sr;

    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject shootPosition;
    [SerializeField] private GameObject DeathScreen;

    public float delay = 0.75f;
    public float time = 0f;

    private float tempTime = 0f;
    [SerializeField] private Image pHealthBar;
    [SerializeField] private float pHealth = 100;

    [SerializeField] private float immunity = 2;
    private bool immune = false;

    private bool hasDash = true;
    [SerializeField] private float dashingSpeed = 70f;
    [SerializeField] private float dashingTime = 0.15f;
    [SerializeField] private float dashingCooldown = .6f;
    [SerializeField] private TrailRenderer tr;



    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        tr = GetComponent<TrailRenderer>();
        sr = GetComponent<SpriteRenderer>();
        pHealthBar.fillAmount = pHealth / 100f;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        Fire();

        if (immune)
        {
            tempTime += Time.deltaTime;
            if (tempTime >= immunity)
            {
                tempTime = 0;
                immune = false;
            }
        }
        // if (pHealth <= 0 || Tracker.gameOver)
        if (pHealth <= 0)
        {
            TakeDamage(pHealth);
            gameObject.SetActive(false);
            DeathScreen.SetActive(true);
        }

        Jump();
        IsMoving();

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.C)) && hasDash)
        {
            StartCoroutine(Dash());
        }

        Flip();
    }
    // Detect if it is on the ground
    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.gameObject.tag == "Ground")
        {
            grounded = true;
            hasDash = true;
            animator.SetBool("jumping", false);
            animator.SetBool("walking", true);
            // rb.gravityScale = 50;
        }
    }

    void OnCollisionExit2D(Collision2D c)
    {
        if (c.gameObject.tag == "Ground")
        {
            grounded = false;
            animator.SetBool("walking", false);
            animator.SetBool("jumping", true);
        }
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        if (c.gameObject.tag == "BAttack" && !immune)
        {
            TakeDamage(10);
            // Tracker.damageTaken += 10;
            immune = true;
        }
        if (c.gameObject.CompareTag("Enemy") && !immune)
        {
            TakeDamage(10);
            // Tracker.damageTaken += 10;
            immune = true;
        }

    }

    void Jump()
    {
        if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space)) && grounded)
        {
            rb.gravityScale = 4;
            rb.velocity = new Vector2(rb.velocity.x, jumpStrength);
            jumped = true;
        }
        else if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space)) && jumped)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpStrength);
            jumped = false;
        }
    }


    void IsMoving()
    {
        move = Input.GetAxis("Horizontal");
        if (move != 0)
        {
            animator.SetBool("walking", true);
            gameObject.transform.SetParent(null);
            rb.velocity = new Vector2(move * speed, rb.velocity.y);
        }
        else
        {
            animator.SetBool("walking", false);
        }
    }

    // Dashes too much if player isn't moving
    private IEnumerator Dash()
    {
        animator.SetBool("dashing", true);
        animator.SetBool("dashing", false);
        hasDash = false;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        speed = dashingSpeed;
        if (facingRight)
        {
            rb.velocity = new Vector2(dashingSpeed, 0f);
        }
        else
        {
            rb.velocity = new Vector2(-1 * dashingSpeed, 0f);
        }
        // speed = dashingSpeed;
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        speed = normalSpeed;
        rb.velocity = new Vector2(0f, 0f);
        rb.gravityScale = originalGravity;
        tr.emitting = false;
        yield return new WaitForSeconds(dashingCooldown);
        hasDash = true;
    }

    void Flip()
    {
        if (facingRight && move < 0 || !facingRight && move > 0)
        {
            facingRight = !facingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    void Fire()
    {
        if (Input.GetKeyDown(KeyCode.X) && time > delay)
        {
            GameObject circle = Instantiate(bullet, shootPosition.transform.position, Quaternion.identity);
            Rigidbody2D circleRb = circle.GetComponent<Rigidbody2D>();

            if (facingRight)
            {
                circleRb.velocity = new Vector2(30, 0);
            }
            else
            {
                circleRb.velocity = new Vector2(-30, 0);
            }
        }
    }

    void TakeDamage(float damage)
    {
        pHealth -= damage;
        pHealthBar.fillAmount = pHealth / 100f;
    }
}
