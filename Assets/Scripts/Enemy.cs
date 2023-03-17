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

    [Header("Other")]
    [SerializeField] Animator anim;
    [SerializeField] LayerMask playerMask;

    // Start is called before the first frame update
    void Start()
    {
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
            Destroy(this.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
