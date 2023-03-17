using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] int damage;
    [SerializeField] GameObject destroyParticles;
    [SerializeField] bool bouncy = false;

    private void Start()
    {
        StartCoroutine(DestroyParticle(5f));
        if (bouncy)
        {
            GetComponent<SphereCollider>().isTrigger = false;
        }
        else
        {
            GetComponent<SphereCollider>().isTrigger = true;
        }    
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Enemy"))
        {
            other.gameObject.GetComponentInParent<Enemy>().LoseHealth(damage);
        }
        StartCoroutine(DestroyParticle(0f));

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Enemy"))
        {
            collision.gameObject.GetComponentInParent<Enemy>().LoseHealth(damage);
            StartCoroutine(DestroyParticle(0f));
        }
    }

    IEnumerator DestroyParticle(float delay)
    {
        yield return new WaitForSeconds(delay);
        var particles = Instantiate(destroyParticles, transform.position, Quaternion.identity);
        particles.GetComponent<ParticleSystem>().Play();

        Destroy(particles, 3f);
        Destroy(this.gameObject);
    }
}
