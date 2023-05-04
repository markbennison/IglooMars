using UnityEngine;
using System.Collections;

namespace Igloo {
    public class FollowObjectTransform : MonoBehaviour
    {
        public enum Follow {OBJECT, MAIN_CAMERA}
        public Follow followType = Follow.OBJECT;
        public GameObject followObject;

        private Transform followTransform;

        public bool followPosition;
        public bool followRotation;
        public bool followScale;

        /// <summary>
        /// The position vector - Choose which axis of the position should be followed
        /// </summary>
        public enum PositionVector { XYZ, X, Y, Z, XY, XZ, YZ };
        public PositionVector positionVector = PositionVector.XYZ;

        /// <summary>
        /// The rotation vector. - Choose which axis of the rotaion should be followed
        /// </summary>
        public enum RotationVector { XYZ, X, Y, Z, XY, XZ, YZ };
        public RotationVector rotationVector = RotationVector.XYZ;

        /// <summary>
        /// The scale vector. - Choose which axis of scale should be followed
        /// </summary>
        public enum ScaleVector { XYZ, X, Y, Z, XY, XZ, YZ };
        public ScaleVector scaleVector = ScaleVector.XYZ;

        public Vector3 positionOffset;

        void LateUpdate() {
            switch (followType) {
                case Follow.MAIN_CAMERA:
                    followTransform = Camera.main.transform;
                    break;
                case Follow.OBJECT:
                    followTransform = followObject.transform;
                    break;
                default:
                    break;
            }
            if (!followTransform)   followTransform = Camera.main.transform;
            if (followPosition)     SetPositionTransform();
            if (followRotation)     SetRotationTransform();
            if (followScale)        SetScaleTrasform();
        }

        void SetPositionTransform() {
            // Position 
            switch (positionVector) {
                case PositionVector.XYZ:
                    transform.position = new Vector3(followTransform.position.x, followTransform.position.y, followTransform.position.z) + positionOffset;
                    break;
                case PositionVector.X:
                    transform.position = new Vector3(followTransform.position.x, transform.position.y, transform.position.z) + positionOffset;
                    break;
                case PositionVector.Y:
                    transform.position = new Vector3(transform.position.x, followTransform.position.y, transform.position.z) + positionOffset;
                    break;
                case PositionVector.Z:
                    transform.position = new Vector3(transform.position.x, transform.position.y, followTransform.position.z) + positionOffset;
                    break;
                case PositionVector.XY:
                    transform.position = new Vector3(followTransform.position.x, followTransform.position.y, transform.position.z) + positionOffset;
                    break;
                case PositionVector.XZ:
                    transform.position = new Vector3(followTransform.position.x, transform.position.y, followTransform.position.z) + positionOffset;
                    break;
                case PositionVector.YZ:
                    transform.position = new Vector3(transform.position.x, followTransform.position.y, followTransform.position.z) + positionOffset;
                    break;
                default:
                    print("Incorrect Position Vector");
                    break;
            }
        }

        void SetRotationTransform() {

            switch (rotationVector) {
                case RotationVector.XYZ:
                    transform.eulerAngles = new Vector3(followTransform.eulerAngles.x, followTransform.eulerAngles.y, followTransform.eulerAngles.z);
                    break;
                case RotationVector.X:
                    transform.eulerAngles = new Vector3(followTransform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
                    break;
                case RotationVector.Y:
                    transform.eulerAngles = new Vector3(transform.eulerAngles.x, followTransform.eulerAngles.y, transform.eulerAngles.z);
                    break;
                case RotationVector.Z:
                    transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, followTransform.eulerAngles.z);
                    break;
                case RotationVector.XY:
                    transform.eulerAngles = new Vector3(followTransform.eulerAngles.x, followTransform.eulerAngles.y, transform.eulerAngles.z);
                    break;
                case RotationVector.XZ:
                    transform.eulerAngles = new Vector3(followTransform.eulerAngles.x, transform.eulerAngles.y, followTransform.eulerAngles.z);
                    break;
                case RotationVector.YZ:
                    transform.eulerAngles = new Vector3(transform.eulerAngles.x, followTransform.eulerAngles.y, followTransform.eulerAngles.z);
                    break;
                default:
                    print("Incorrect Rotation Vector");
                    break;
            }
        }

        void SetScaleTrasform() {
            switch (scaleVector) {
                case ScaleVector.XYZ:
                    transform.localScale = new Vector3(followTransform.localScale.x, followTransform.localScale.y, followTransform.localScale.z);
                    break;
                case ScaleVector.X:
                    transform.localScale = new Vector3(followTransform.localScale.x, transform.localScale.y, transform.localScale.z);
                    break;
                case ScaleVector.Y:
                    transform.localScale = new Vector3(transform.localScale.x, followTransform.localScale.y, transform.localScale.z);
                    break;
                case ScaleVector.Z:
                    transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, followTransform.localScale.z);
                    break;
                case ScaleVector.XY:
                    transform.localScale = new Vector3(followTransform.localScale.x, followTransform.localScale.y, transform.localScale.z);
                    break;
                case ScaleVector.XZ:
                    transform.localScale = new Vector3(followTransform.localScale.x, transform.localScale.y, followTransform.localScale.z);
                    break;
                case ScaleVector.YZ:
                    transform.localScale = new Vector3(transform.localScale.x, followTransform.localScale.y, followTransform.localScale.z);
                    break;
                default:
                    print("Incorrect Scale Vector");
                    break;
            }
        }
    }
}

