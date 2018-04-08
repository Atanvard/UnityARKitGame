using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfRightingPlayer : MonoBehaviour {
    public float moveSpeed = 2f;
    private Rigidbody _body;

	// Use this for initialization
	void Start () {
        _body = gameObject.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        //Get the forward direction of the capsule solely on the XZ plane for rotational calculation
        Vector3 forward = transform.forward;
        forward = new Vector3(forward.x, 0, forward.z).normalized;

        //rotate the rotation closer to true up based on MoveSpeed
        Quaternion newRot = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(
                forward,
                Vector3.up), Time.deltaTime * moveSpeed);

        //Apply rotation to player
        if (!_body) {
            transform.rotation = newRot;
        } else {
            //Prevent this corrective rotation from applying angular velocity to the player if the player has a rigidbody attached
            Vector3 velocity = _body.velocity;
            _body.isKinematic = true;
            transform.rotation = newRot;
            _body.isKinematic = false;
            _body.velocity = velocity;
        }
	}
}
