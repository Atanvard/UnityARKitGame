using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SKStudios.Portals.Demos
{
    public class ObjectSpawner : MonoBehaviour
    {
        public GameObject spawnedObject;
        private GameObject cube;

        private Rigidbody body;

        // Use this for initialization
        void Start()
        {
            cube = Instantiate(spawnedObject);
            body = cube.GetComponent<Rigidbody>();
            StartCoroutine(DropCube());
        }

        IEnumerator DropCube()
        {
            while (true)
            {
                cube.transform.position = transform.position;
                cube.transform.rotation = Quaternion.identity;
                body.velocity = Vector3.zero;
                yield return new WaitForSeconds(2);
            }
        }
    }
}
