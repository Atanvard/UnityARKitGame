using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnedObject : MonoBehaviour {

	// Use this for initialization
	void Start () {
        gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * 2, ForceMode.Force);
        StartCoroutine(Initialize());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    IEnumerator Initialize() {
        yield return new WaitForSeconds(2);
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }
}
