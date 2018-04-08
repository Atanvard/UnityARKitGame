using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SKStudios.Portals.Demos
{
    public class ResetPosition : MonoBehaviour {
        private Vector3 initialPosition;
        private Rigidbody rigid;
        private IEnumerator ResetPos;
        public float resetTime;
        // Use this for initialization
        void Start() {
            rigid = gameObject.GetComponent<Rigidbody>();
            initialPosition = transform.position;
            ResetPos = ResetPositionandVelocity(resetTime);
            StartCoroutine(ResetPos);
        }

        public IEnumerator ResetPositionandVelocity(float time)
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(time);
                rigid.isKinematic = true;
                transform.position = initialPosition;
                rigid.isKinematic = false;
                rigid.velocity = Vector3.zero;
            }     
        }
    }

}

