using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Enemy : MonoBehaviour
{
    NavMeshAgent agent;
    PlayerMovement player;

    [Header("Enemy Attributes")]
    bool playerInSightRange;
    [SerializeField] int health;
    [SerializeField] float sightRange;
    [SerializeField] int damageToPlayer = 2;
    bool canAttack = true;

    [Header("Other")]
    [SerializeField] Animator anim;
    [SerializeField] LayerMask playerMask;
    [SerializeField] GameObject destroyParticles;

    public int DamageToPlayer { get { return damageToPlayer; } set { damageToPlayer = value; } }
    public bool CanAttack { get { return canAttack; } set { canAttack = value; } }

    // Start is called before the first frame update
    void Start()
    {
        canAttack = true;
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerMask);

        if (playerInSightRange)
            agent.destination = PlayerMovement.Instance.transform.position;
        else
            agent.destination = transform.position;

        if (agent.velocity != Vector3.zero)
            anim.SetBool("isWalking", true);
        else
            anim.SetBool("isWalking", false);
    }

    public void LoseHealth(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            var particles = Instantiate(destroyParticles, transform.position, Quaternion.identity);
            particles.GetComponent<ParticleSystem>().Play();

            Destroy(this.gameObject);
        }
    }

    public IEnumerator AttackReset()
    {
        canAttack = false;
        yield return new WaitForSeconds(.5f);
        canAttack = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
