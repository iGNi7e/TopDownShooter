using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity {

    public enum State {Idle,Chasing,Attacking};
    State currentState;

    public ParticleSystem deathEffect;

    NavMeshAgent pathfinder;
    Transform target;
    Material skinMaterial;
    LivingEntity targetEntity;

    Color originalColor;

    float attackDistanceThreshhold = 1.5f;
    float timeBetweenAttacks = 1f;
    float damage = 40;

    float nextAttackTime;
    float myCollisionRadius;
    float targetCollisionRadius;

    bool hasTarget;

	// Use this for initialization
	protected override void Start () {
        base.Start();
        pathfinder = GetComponent<NavMeshAgent>();
        skinMaterial = GetComponent<Renderer>().material;
        originalColor = skinMaterial.color;

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            currentState = State.Chasing;
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            targetEntity.OnDeath += OnTargetDeath;

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CharacterController>().radius;

            StartCoroutine(UpdatePath());
        }
	}

    public override void TakeHit(float damage,Vector3 hitPoint,Vector3 hitDirection)
    {
        if(damage >= health)
        {
            Destroy(Instantiate(deathEffect.gameObject,hitPoint,Quaternion.FromToRotation(Vector3.forward,hitDirection)) as GameObject,deathEffect.startLifetime);
        }
        base.TakeHit(damage,hitPoint,hitDirection);
    }

    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }

	// Update is called once per frame
	void Update () {
        if (hasTarget)
        {
            if (Time.time > nextAttackTime)
            {
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;

                if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshhold + myCollisionRadius + targetCollisionRadius,2))
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    StartCoroutine(Attack());
                }
            }
        }
	}

    IEnumerator Attack()
    {
        currentState = State.Attacking;

        pathfinder.enabled = false;

        Vector3 originalPosition = transform.position;
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - directionToTarget * (myCollisionRadius + targetCollisionRadius);

        float attackSpeed = 3f;
        float percent = 0f;

        skinMaterial.color = Color.cyan;

        bool hasAppliedDamage = false;

        while (percent <= 1)
        {
            if(percent >= .5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeHit(damage,Vector3.zero,Vector3.zero);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent,2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPosition,attackPosition,interpolation);

            yield return null;
        }

        skinMaterial.color = originalColor;
        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = .25f;
        while(hasTarget)
        {
            if (currentState == State.Chasing)
            {
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - directionToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshhold/2);
                if (!dead)
                    pathfinder.SetDestination(targetPosition);
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
