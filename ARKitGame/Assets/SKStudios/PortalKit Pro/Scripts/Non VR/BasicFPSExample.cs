
using SKStudios.Common.Utils;
using UnityEngine;
namespace SKStudios.Portals{
    public class BasicFPSExample : MonoBehaviour{

        public GameObject eyeParent;
        public GameObject headset;
        public Camera headCam;
        public Collider playerPhysBounds;
        public float moveDistance = 0.2f;
        public float moveTime = 0.2f;
        private float totalTime;

        private bool moving;
        private float distanceMoved;
        private Vector3 moveStartPosition;
        private Quaternion lastBodyRot;

        public Vector2 clampInDegrees = new Vector2(360, 180);
        public bool lockCursor;
        public Vector2 sensitivity = new Vector2(2, 2);
        public Vector2 smoothing = new Vector2(3, 3);
        public Vector2 targetDirection;
        public Vector2 targetCharacterDirection;
        private bool paused = false;
        private Rigidbody _rigidbody;
        Vector2 _mouseAbsolute;
        Vector2 _smoothMouse;
        // Assign this if there's a parent object controlling motion, such as a Character Controller.
        // Yaw rotation will affect this object instead of the camera if set.
        public GameObject characterBody;

        void Start() {
            // Set target direction to the camera's initial orientation.
            targetDirection = transform.localRotation.eulerAngles;
            // Set target direction for the character body to its inital state.
            if (characterBody) targetCharacterDirection = characterBody.transform.localRotation.eulerAngles;
            _rigidbody = GetComponent<Rigidbody>();
        }
        void OnEnable() {
            //playerPhysBounds.transform.position = headCam.transform.position;
            PositionalUpdate();
            playerPhysBounds.enabled = true;
            playerPhysBounds.transform.localPosition += new Vector3(0, playerPhysBounds.GetComponent<Collider>().bounds.extents.y, 0);
        }



        public void Update()
        {

            if (Input.GetKeyDown(KeyCode.Escape))
                paused = !paused;

            Cursor.lockState = !paused ? CursorLockMode.Locked : CursorLockMode.None;
            if (paused) return;
            // Allow the script to clamp based on a desired target value.
            var targetOrientation = Quaternion.Euler(targetDirection);
            var targetCharacterOrientation = Quaternion.Euler(targetCharacterDirection);

            // Get raw mouse input for a cleaner reading on more sensitive mice.
            var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            // Scale input against the sensitivity setting and multiply that against the smoothing value.
            mouseDelta = Vector2.Scale(mouseDelta,
                new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

            // Interpolate mouse movement over time to apply smoothing delta.
            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

            // Find the absolute mouse movement value from point zero.
            _mouseAbsolute += _smoothMouse;

            // Clamp and apply the local x value first, so as not to be affected by world transforms.
            if (clampInDegrees.x < 360)
                _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

            var VRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right);
            headset.transform.localRotation = VRotation;

            // Then clamp and apply the global y value.
            if (clampInDegrees.y < 360)
                _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

            headset.transform.localRotation *= targetOrientation;

            // If there's a character body that acts as a parent to the camera
            if (characterBody) {
                var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, characterBody.transform.up);
                characterBody.transform.localRotation = yRotation;
                characterBody.transform.localRotation *= targetCharacterOrientation;
            } else {
                var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x,
                    headset.transform.InverseTransformDirection(Vector3.up));
                headset.transform.localRotation *= yRotation;
            }

            Vector3 dirVector = headCam.transform.forward;
            dirVector.y = 0;
            dirVector = dirVector.normalized * moveDistance;
            //Keyboard input
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += dirVector * transform.lossyScale.magnitude * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position += (Quaternion.AngleAxis(-90, Vector3.up) * dirVector) * transform.lossyScale.magnitude * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position += (Quaternion.AngleAxis(180, Vector3.up) * dirVector) * transform.lossyScale.magnitude * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += (Quaternion.AngleAxis(90, Vector3.up) * dirVector) * transform.lossyScale.magnitude * Time.deltaTime;
            }
        }

        private void PositionalUpdate() {
            /*
            Vector3 diffPos = headCam.transform.localPosition - lastHeadPos;
            playerPhysBounds.transform.localPosition += new Vector3(diffPos.x, 0, diffPos.z);
          
            //Vertical movement
            eyeParent.transform.localPosition = new Vector3(eyeParent.transform.localPosition.x, diffPos.y, eyeParent.transform.localPosition.z);

            //Camera movement /w physical control override
            Vector3 oldEuler = playerPhysBounds.transform.localRotation.eulerAngles;
            Transform oldParent = eyeParent.transform.parent;
            eyeParent.transform.SetParent(playerPhysBounds.transform);
            eyeParent.transform.localPosition = (-headCam.transform.localPosition);

            eyeParent.transform.localPosition = new Vector3(eyeParent.transform.localPosition.x,
                playerPhysBounds.bounds.extents.y - (playerPhysBounds.bounds.extents.y * 2),
                eyeParent.transform.localPosition.z);

            //Cache new position
            lastHeadPos = headCam.transform.localPosition;
            lastBodyRot = playerPhysBounds.transform.localRotation;*/
        }

        //void FixedUpdate() {
            //playerPhysBounds.transform.rotation = Quaternion.Slerp(playerPhysBounds.transform.rotation, targetRotation, Time.deltaTime * 3f);

            //Vector3 oldPos = playerPhysBounds.transform.localPosition;
            //transform.position = new Vector3(transform.position.x, playerPhysBounds.transform.position.y, transform.position.z);
            //playerPhysBounds.transform.localPosition = oldPos;

            //eyeParent.transform.position = playerPhysBounds.transform.position;
            //eyeParent.transform.position = playerPhysBounds.transform.TransformPoint(-headCam.transform.localPosition);
            //eyeParent.transform.rotation = playerPhysBounds.transform.rotation;


        //}

        void moveByDelta(Vector3 start, Vector3 delta, float moveTime) {
            float distDelta = Mathfx.Hermite(0, 1, totalTime / moveTime) - distanceMoved;
            Vector3 deltaPos = Quaternion.AngleAxis(headset.gameObject.transform.localEulerAngles.y, Vector3.up) * (delta * distDelta);
            transform.position += transform.rotation * deltaPos;
            totalTime += Time.deltaTime;
            distanceMoved += distDelta;
        }
    }
}
