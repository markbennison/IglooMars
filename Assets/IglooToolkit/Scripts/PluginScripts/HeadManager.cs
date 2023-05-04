using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Igloo {
    public class HeadManager : MonoBehaviour
    {
        private float nearClipPlane = 0.3f;
        private float farClipPlane = 1000f;

        private bool isHeadTracking = false;

        private Vector3 leftEyeOffset = Vector3.zero;
        private Vector3 centerEyeOffset { get { return (leftEyeOffset + rightEyeOffset) * 0.5f; } }
        private Vector3 rightEyeOffset = Vector3.zero;

        private Vector3 positionCached = Vector3.zero;
        private Vector3 rotationCached = Vector3.zero;

        public HeadSettingsChange OnHeadSettingsChange;
        public delegate void HeadSettingsChange();

        public float FarClipPlane { 
            get { return farClipPlane; }
            set {
                farClipPlane = value;
                OnHeadSettingsChange();
            }
        }
        public float NearClipPlane {
            get { return nearClipPlane; }
            set {
                nearClipPlane = value;
                OnHeadSettingsChange();
            }
        }
        public Vector3 LeftEyeOffset {
            get { return leftEyeOffset; }
            set {
                leftEyeOffset = value;
                OnHeadSettingsChange();
            }
        }
        public Vector3 RightEyeOffset {
            get { return rightEyeOffset; }
            set{
                rightEyeOffset = value;
                OnHeadSettingsChange();
            }
        }
        public bool HeadTracking {
            get { return isHeadTracking; }
            set {
                isHeadTracking = value;
                if (!isHeadTracking) {
                    transform.localPosition     = positionCached;
                    transform.localEulerAngles  = rotationCached;
                } 
            }
        }

        public void SetSettings(HeadSettings hs) {
            transform.localPosition     = hs.headPositionOffset.Vector3;
            positionCached              = hs.headPositionOffset.Vector3;
            transform.localEulerAngles  = hs.headRotationOffset.Vector3;
            rotationCached              = hs.headRotationOffset.Vector3;
            isHeadTracking              = hs.headtracking;
            if (hs.leftEyeOffset != null) leftEyeOffset     = hs.leftEyeOffset.Vector3;
            if (hs.rightEyeOffset != null) rightEyeOffset   = hs.rightEyeOffset.Vector3;

            IglooManager.Instance.GetNetworkManager().OnHeadPosition += HandlePositonMessage;
            IglooManager.Instance.GetNetworkManager().OnHeadRotation += HandleRotationMessage;
        }

        public virtual void HandlePositonMessage(Vector3 pos) {
                if (isHeadTracking) transform.localPosition = pos;           
        }

        public virtual void HandleRotationMessage(Vector3 rotation) {
                if(isHeadTracking) transform.localEulerAngles = rotation ;           
        }

        public HeadSettings GetSettings() {
            HeadSettings hs = new HeadSettings();
            // Use starting head position as head tracking is not friendly here
            hs.headPositionOffset   = new Vector3Item(positionCached);
            hs.headRotationOffset   = new Vector3Item(rotationCached);
            hs.leftEyeOffset        = new Vector3Item(leftEyeOffset);
            hs.rightEyeOffset       = new Vector3Item(rightEyeOffset);
            hs.headtracking         = isHeadTracking;
            return hs;
        }

        public Camera CreateEye(EYE eye ,string _name, Vector3 rotation, GameObject prefab = null, Transform _parentTransform = null) {
            GameObject cameraGO;
            if (prefab != null) cameraGO = Instantiate(prefab) as GameObject;
            else cameraGO = new GameObject();
            cameraGO.name = _name + "_" + eye.ToString();
            if (cameraGO.GetComponent<AudioListener>()) Destroy(cameraGO.GetComponent<AudioListener>());
            if (!cameraGO.GetComponent<Camera>()) cameraGO.AddComponent<Camera>();
            Camera res = cameraGO.GetComponent<Camera>();

            switch (eye) {
                case EYE.LEFT:
                    GameObject camParent = new GameObject("Stereo Camera Pair for - " + _name);
                    camParent.transform.parent = transform;
                    camParent.transform.localPosition = Vector3.zero;
                    camParent.transform.localEulerAngles = rotation;

                    res.transform.parent = camParent.transform;
                    res.transform.localEulerAngles = Vector3.zero;
                    break;
                case EYE.CENTER:
                    res.stereoTargetEye = StereoTargetEyeMask.None;
                    res.transform.parent = transform;
                    res.transform.localEulerAngles = rotation;
                   break;
                case EYE.RIGHT:
                    if (_parentTransform) {
                        res.transform.parent = _parentTransform.transform;
                        res.transform.localEulerAngles = Vector3.zero;
                    }
                    else Debug.LogError("Parent object for Right eye does not exist");
                    break;
                default:
                    break;
            }

            ApplyCameraSettings(res, eye);
            return res;
        }

        public void SetEyeSeparation(float value) {
            leftEyeOffset = new Vector3(-value/2, 0, 0);
            rightEyeOffset = new Vector3(value/2, 0, 0);
            OnHeadSettingsChange();
        }

        public void ApplyCameraSettings(Camera cam , EYE eye) {
            cam.nearClipPlane   = nearClipPlane;
            cam.farClipPlane    = farClipPlane;
            switch (eye) {
                case EYE.LEFT:
                    cam.transform.localPosition = leftEyeOffset;                   
                    break;
                case EYE.CENTER:
                    cam.transform.localPosition = centerEyeOffset;
                    break;
                case EYE.RIGHT:
                    cam.transform.localPosition = rightEyeOffset;
                    break;
            }
        }
    }
}

