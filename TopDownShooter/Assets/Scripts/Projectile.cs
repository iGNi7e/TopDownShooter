using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    float speed = 10f;
    float damage = 13f;

    public void SetSpeed(float _speed)
    {
        speed = _speed;
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy")
        {
            IDamageable damageableObject = other.GetComponent<IDamageable>();
            if(damageableObject != null)
            {
                damageableObject.TakeHit(damage);
            }
            //Destroy(other.gameObject);
        }
    }
}
