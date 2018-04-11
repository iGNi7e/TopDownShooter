using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Gun : MonoBehaviour
{

    public enum GunType { Semi, Burst, Auto }
    public GunType gunType;
    public float rpm;

    //Components
    public Transform spawn;
    public AudioClip clip;
    private LineRenderer tracer;

    //System
    private float secondsBetweenShots;
    private float nextPossibleShootTime;

    private void Start()
    {
        secondsBetweenShots = 60 / rpm;
        if (GetComponent<LineRenderer>())
        {
            tracer = GetComponent<LineRenderer>();
        }
    }

    public void Shoot()
    {

        if (CanShoot())
        {
            Ray ray = new Ray(spawn.position,spawn.forward);
            RaycastHit hit;

            float distanceShot = 20f;

            if (Physics.Raycast(ray,out hit,distanceShot))
            {
                distanceShot = hit.distance;
            }
            //Debug.DrawLine(ray.origin,ray.direction * distanceShot,Color.red,1);
            nextPossibleShootTime = Time.time * secondsBetweenShots;

            GetComponent<AudioSource>().PlayOneShot(clip);
            if (tracer) StartCoroutine("RendreTracer",ray.direction * distanceShot);
        }
    }

    IEnumerator RendreTracer(Vector3 hitPoint)
    {
        tracer.enabled = true;
        tracer.SetPosition(0,spawn.position);
        tracer.SetPosition(1,spawn.position + hitPoint);
        yield return null;
        tracer.enabled = false;
    }

    public void ShootContinue()
    {
        if (gunType == GunType.Auto)
        {
            Shoot();
        }
    }

    private bool CanShoot()
    {
        bool canShoot = true;

        if (Time.time < nextPossibleShootTime)
        {
            canShoot = false;
        }

        return canShoot;
    }

}
