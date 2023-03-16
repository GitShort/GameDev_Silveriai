using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] GameObject destroyParticles;

    private void OnTriggerEnter(Collider other)
    {
        GameObject particles = Instantiate(destroyParticles, transform.position, Quaternion.identity);
        particles.GetComponent<ParticleSystem>().Play();

        Destroy(particles, 3f);
        Destroy(this.gameObject);
    }
}
