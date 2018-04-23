using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable {

    public float startingHealth;
    protected float health;
    protected bool dead;

    public event System.Action OnDeath;

    protected virtual void Start()
    {
        health = startingHealth;
    }

    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        health -= damage;
        if(health <= 0)
        {
            Die();
        }
    }

    [ContextMenu("Self Destruct")]
    protected void Die()
    {
        dead = true;
        if(OnDeath != null)
        {
            OnDeath();
        }

        GameObject.Destroy(gameObject);
    }

}
