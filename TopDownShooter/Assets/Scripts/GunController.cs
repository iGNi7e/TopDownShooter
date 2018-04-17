using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour {

    public Transform hands;
    public Gun startingGun;

    Gun equippedGun;

    private void Start()
    {
        if(startingGun != null)
        {
            EquipGun(startingGun);
        }
    }

    public void EquipGun(Gun gunToEquip)
    {
        if(equippedGun != null)
        {
            Destroy(equippedGun.gameObject);
        }

        equippedGun = Instantiate(gunToEquip,hands.transform.position, hands.rotation) as Gun;
        equippedGun.transform.parent = hands;

    }

}
