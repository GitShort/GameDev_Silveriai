using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
{

    [SerializeField] AudioClip pickupSound;
    [SerializeField] float rotationSpeed;
    [SerializeField] float floatAmplitude;
    [SerializeField] float floatFrequency;

    [SerializeField] GameObject destroyParticles;
    [SerializeField] GameObject model;

    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        model.transform.Rotate(0f, rotationSpeed, 0f, Space.Self);
        
        tempPos = posOffset;
        tempPos.y = Mathf.Sin(Time.fixedTime * Mathf.PI * floatFrequency) * floatAmplitude;
        model.transform.localPosition = tempPos;
    }

    public void PickupCoin()
    {
        if (destroyParticles != null)
        {
            var particles = Instantiate(destroyParticles, model.transform.position, Quaternion.identity, null);
            particles.GetComponent<ParticleSystem>().Play();
            Destroy(particles, 3f);
        }
        AudioSource.PlayClipAtPoint(pickupSound, model.transform.position);
        GameManager.Instance.CollectectCoins++;
        GameManager.Instance.UpdateUI();
    }

    private void OnDestroy()
    {

    }
}
