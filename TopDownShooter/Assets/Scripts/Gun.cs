﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Gun : MonoBehaviour
{

    public enum GunType { Semi, Burst, Auto } //виды стрельбы
    public GunType gunType;
    public float rpm; // частота выстрелов в секунду

    //Prefabs
    public Projectile projectile;

    //Components
    public Transform spawn; //впавн выстрела
    public Transform shellSpawn;
    public Rigidbody shell;
    public AudioClip clip; //звук стрельбы
    private LineRenderer tracer; //отрисовка выстрела
    MuzzleFlash muzzleFlash;

    //System
    private float secondsBetweenShots; //сколько выстрелов в секунду
    private float nextPossibleShootTime; //для определения возможности произведения выстрела

    private void Start()
    {
        muzzleFlash = GetComponent<MuzzleFlash>();
        secondsBetweenShots = 60 / rpm; //определние частоты выстрелов, конечное
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

            nextPossibleShootTime = Time.time + secondsBetweenShots;

            GetComponent<AudioSource>().PlayOneShot(clip);
            //if (tracer) StartCoroutine("RendreTracer",ray.direction * distanceShot);

            Projectile newProjectile = Instantiate(projectile,spawn.transform.position,spawn.rotation);
            newProjectile.SetSpeed(10f);

            Rigidbody newShell = Instantiate(shell,shellSpawn.position,Quaternion.identity) as Rigidbody;
            newShell.AddForce(shellSpawn.forward * Random.Range(150f,200f) + spawn.forward * Random.Range(-10f,10f));

            muzzleFlash.Activate();
        }
    }

    IEnumerator RendreTracer(Vector3 hitPoint) //отрисовка линии
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
