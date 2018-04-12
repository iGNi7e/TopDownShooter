using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour {

    private float lifeTime = 2f;
    private float deathTime;
    private Material mat;
    private Color originalCol;
    private float fadePercent;
    private bool fading;

	void Start () {
        mat = GetComponent<Renderer>().material;
        originalCol = mat.color;
        deathTime = Time.time + lifeTime;
        StartCoroutine("Fade");
	}
	
    IEnumerator Fade()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);

            if (fading)
            {
                fadePercent += Time.deltaTime * 2;
                mat.color = Color.Lerp(originalCol,Color.clear,fadePercent);

                if (fadePercent >= 1)
                {
                    Destroy(gameObject);
                }
            }
            else if (Time.time > deathTime)
                {
                    fading = true;
                }
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ground")
        {
            GetComponent<Rigidbody>().Sleep();
        }
    }
}
