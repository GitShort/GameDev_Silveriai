using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed;
    [SerializeField] float groundDrag;
    [SerializeField] float friction;

    [SerializeField] bool useSprint = true;
    [SerializeField] float sprintSpeedMultiplier = 1.5f;

    bool isSprinting = false;
    float originalMoveSpeed;
    bool movementEnabled = true;

    [Header("Jumping")]
    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;
    [SerializeField] float airMultiplier;
    bool readyToJump;

    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] bool useLowJump = true;
    [SerializeField] float lowJumpMultiplier = 2f;

    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] KeyCode shootKey = KeyCode.Mouse0;

    [Header("Fighting")]
    [SerializeField] int health = 5;
    int currHealth;
    [SerializeField] int bullets = 10;
    [SerializeField] float shootPower = 20f;
    bool readyToShoot = true;

    [Header("Particles")]
    [SerializeField] GameObject loseParticles;

    [Header("Sounds")]
    [SerializeField] AudioClip shootSound;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip loseSound;
    [SerializeField] AudioClip getHitSound;

    [Header("")]
    [Header("")]
    [Header("----------------------OTHER------------------")]
    [SerializeField] Transform orientation;
    [SerializeField] Animator anim;
    [SerializeField] PhysicMaterial physicMaterial;
    [SerializeField] GameObject projectile;
    [SerializeField] Transform shootingPos;
    [SerializeField] bool sideScroller = false;
    [Header("Ground check")]
    [SerializeField] float playerHeight;
    [SerializeField] LayerMask whatIsGround;
    bool grounded;

    Vector3 moveDirection;
    Rigidbody rb;

    float horizontalInput;
    float verticalInput;

    public int Health { get { return health; } set { health = value; } }
    public int CurrHealth { get { return currHealth; } set { currHealth = value; } }

    public int Bullets { get { return bullets; } set { bullets = value; } }

    public static PlayerMovement Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        currHealth = health;
    }

    private void Start()
    {

        originalMoveSpeed = moveSpeed;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        ResetJump();
    }

    private void Update()
    {
        if (GameManager.Instance.GameStarted)
        {
            grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

            if (movementEnabled)
            {
                MyInput();
            }

            SpeedControl();

            if (grounded)
            {
                physicMaterial.dynamicFriction = friction;
                rb.drag = groundDrag;
            }
            else
            {
                physicMaterial.dynamicFriction = 0.01f;
                rb.drag = 0;
            }

            if (Input.GetKey(sprintKey) && !isSprinting && grounded)
            {
                isSprinting = true;
                moveSpeed *= sprintSpeedMultiplier;
            }
            else if (!Input.GetKey(sprintKey) && isSprinting && grounded)
            {
                isSprinting = false;
                moveSpeed = originalMoveSpeed;
            }

            //Debug.Log(rb.velocity.magnitude);
            BetterJump();

            if (Input.GetKeyDown(shootKey) && readyToShoot && bullets > 0)
            {
                StartCoroutine(ShootProjectile());
            }
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.GameStarted)
            MovePlayer();
    }

    void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (!sideScroller)
            verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            anim.SetTrigger("isJumping");
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        if (moveDirection != Vector3.zero)
            anim.SetBool("isRunning", true);
        else
            anim.SetBool("isRunning", false);

    }

    void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    void Jump()
    {
        AudioSource.PlayClipAtPoint(jumpSound, transform.position);
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void BetterJump()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(jumpKey) && useLowJump)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    void ResetJump()
    {
        readyToJump = true;
    }

    IEnumerator ShootProjectile()
    {
        readyToShoot = false;
        bullets--;
        AudioSource.PlayClipAtPoint(shootSound, transform.position);
        GameObject bullet = Instantiate(projectile, shootingPos.transform.position, shootingPos.transform.rotation);
        bullet.GetComponent<Rigidbody>().AddRelativeForce(Vector3.up * shootPower, ForceMode.Impulse);
        GameManager.Instance.UpdateUI();
        yield return new WaitForSeconds(.75f);
        readyToShoot = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Coin"))
        {
            other.gameObject.GetComponent<CoinManager>().PickupCoin();
            Destroy(other.gameObject, 0.05f);
        }

        if (other.gameObject.tag.Equals("Spike"))
        {
            currHealth = 0;
            PlayerLose();
        }

        if (other.gameObject.tag.Equals("Health"))
        {
            var go = other.gameObject.GetComponent<PickupManager>();

            if (currHealth < health && (currHealth + go.ValueRestored < health))
                currHealth += go.ValueRestored;
            else
                currHealth = health; 
            go.Pickup();
            Destroy(other.gameObject, 0.05f);
        }

        if (other.gameObject.tag.Equals("Bullets"))
        {
            var go = other.gameObject.GetComponent<PickupManager>();
            bullets += go.ValueRestored;
            go.Pickup();
            Destroy(other.gameObject, 0.05f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Enemy") && collision.gameObject.GetComponentInParent<Enemy>().CanAttack)
        {
            currHealth -= collision.gameObject.GetComponentInParent<Enemy>().DamageToPlayer;
            StartCoroutine(collision.gameObject.GetComponentInParent<Enemy>().AttackReset());
            GameManager.Instance.UpdateUI();
            AudioSource.PlayClipAtPoint(getHitSound, transform.position);
            if (currHealth <= 0)
            {
                PlayerLose();
            }
        }

        if (collision.gameObject.tag.Equals("Spike"))
        {
            PlayerLose();
        }
    }

    void PlayerLose()
    {
        currHealth = 0;
        GameManager.Instance.UpdateUI();
        AudioSource.PlayClipAtPoint(loseSound, transform.position);
        GameObject deathParticles = Instantiate(loseParticles, transform.position, Quaternion.identity);
        deathParticles.GetComponent<ParticleSystem>().Play();
        StartCoroutine(DisablePlayerChar());
        rb.isKinematic = true;
        movementEnabled = false;
        StartCoroutine(RestartGame());
    }

    IEnumerator DisablePlayerChar()
    {
        yield return new WaitForSeconds(0.1f);
        GetComponentInChildren<CapsuleCollider>().gameObject.SetActive(false);
    }

    IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
