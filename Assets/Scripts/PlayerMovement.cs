using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed;
    [SerializeField] Transform orientation;
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

    [Header("Ground check")]
    [SerializeField] float playerHeight;
    [SerializeField] LayerMask whatIsGround;
    bool grounded;

    [Header("Fighting")]
    [SerializeField] GameObject projectile;
    [SerializeField] Transform shootingPos;
    [SerializeField] float shootPower = 20f;

    [Header("Particles")]
    [SerializeField] GameObject loseParticles;

    Vector3 moveDirection;
    Rigidbody rb;

    float horizontalInput;
    float verticalInput;
    [SerializeField] PhysicMaterial physicMaterial;

    private void Start()
    {
        originalMoveSpeed = moveSpeed;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        ResetJump();
    }

    private void Update()
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

        if (Input.GetKeyDown(shootKey))
            ShootProjectile();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();

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
        Debug.Log("Reset jump");
        readyToJump = true;
    }

    void ShootProjectile()
    {
        GameObject bullet = Instantiate(projectile, shootingPos.transform.position, shootingPos.transform.rotation);
        bullet.GetComponent<Rigidbody>().AddRelativeForce(Vector3.up * shootPower, ForceMode.Impulse);
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
            GameObject deathParticles = Instantiate(loseParticles, transform.position, Quaternion.identity);
            deathParticles.GetComponent<ParticleSystem>().Play();
            StartCoroutine(DisablePlayerChar());
            rb.isKinematic = true;
            movementEnabled = false;
            StartCoroutine(RestartGame());
        }
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
