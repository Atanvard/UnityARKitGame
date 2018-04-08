using System.Collections;
using System.Collections.Generic;
using SKStudios.Common.Utils;
using UnityEngine;

namespace SKStudios.Portals.Demos
{
    //[ExecuteInEditMode]
    public class Spinning : MonoBehaviour
    {
        private float destSpeed;
        public float speed = 4f;
        public float spinupTime;
        public bool spinning = false;
        public bool triggerRequired = false;
        public Vector3 axis = Vector3.up;

        void Start()
        {
            if (triggerRequired)
            {
                destSpeed = speed;
                speed = 0;
            }
        }
        // Update is called once per frame

        void Update()
        {
            if (spinning)
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles +
                                                      Quaternion.AngleAxis(speed * Time.deltaTime, axis).eulerAngles);
        }

        IEnumerator smoothToSpeed(float spinupTime, float speed)
        {
            while (this.speed <= destSpeed)
            {
                this.speed += Mathfx.Sinerp(0, destSpeed, Time.deltaTime / spinupTime);
                yield return new WaitForFixedUpdate();
            }
            yield return null;
            this.speed = destSpeed;
        }
    }
}
