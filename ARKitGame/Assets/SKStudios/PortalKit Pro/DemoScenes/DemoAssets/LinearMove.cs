using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SKStudios.Portals.Demos
{
    public class LinearMove : MonoBehaviour
    {
        public float speed = 10;
        public float height = 1;
        private Vector3 initialPos;

        private bool dir = false;

        // Use this for initialization
        void Start()
        {
            initialPos = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (!dir)
                transform.position = transform.position + new Vector3(0, speed * Time.deltaTime, 0);
            else
                transform.position = transform.position - new Vector3(0, speed * Time.deltaTime, 0);

            if (transform.position.y < initialPos.y - height && dir)
                dir = false;
            if (transform.position.y > initialPos.y && !dir)
                dir = true;

        }
    }
}
