using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SKStudios.Portals.Demos
{
    public class PlayerBody : MonoBehaviour
    {
        public GameObject target;

        private Collider targetCol;

        // Use this for initialization
        void Start()
        {
            StartCoroutine(Setup());
        }

        // Update is called once per frame
        void Update()
        {
            if (targetCol)
                transform.position = targetCol.bounds.center;
        }

        IEnumerator Setup()
        {
            yield return new WaitForSeconds(1);
            targetCol = target.GetComponent<Collider>();
        }
    }
}
