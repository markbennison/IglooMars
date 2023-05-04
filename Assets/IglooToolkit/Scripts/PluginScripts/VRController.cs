using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Igloo {
    public class VRController : MonoBehaviour {

        public GameObject crosshair;
        Renderer crosshairRenderer;

        LineRenderer lineRenderer; 
        bool drawLine = false;
        float lineWidth = 0.01f;
        float maxLineLength = 20.0f;


        bool hasHit = false;
        public float crosshairSize = 0.015f;


        private void Start() {
            lineRenderer = GetComponent<LineRenderer>();
            Vector3[] initLinePositions = new Vector3[2] { Vector3.zero, Vector3.zero };
            lineRenderer.SetPositions(initLinePositions);
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.enabled = true;


            if (IglooManager.Instance != null) {
                IglooManager.Instance.GetNetworkManager().OnVrControllerGyroEvent += SetRotation;
                IglooManager.Instance.GetNetworkManager().OnVrControllerPositionEvent += SetPosition;
            }
            crosshairRenderer = crosshair.GetComponent<Renderer>();
        }

        void SetPosition(int deviceID, Vector3 position) {
            if (deviceID == 1) {
                transform.localPosition = position;
            } 
        }

        void SetRotation(int deviceID, Vector3 rot) {
            if (deviceID == 1) {
                transform.localEulerAngles = rot;
            }
        }

        private void Update() {
            Vector3 endPos = Vector3.zero;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity)) {
                crosshair.transform.position = hit.point;
                endPos = hit.point;

                // Crosshair positioning system
                crosshair.transform.rotation = Quaternion.FromToRotation(crosshair.transform.up, hit.normal) * crosshair.transform.rotation;
                if (!hasHit) {
                    crosshairRenderer.enabled = true;
                    crosshair.transform.localScale = new Vector3(crosshairSize, crosshairSize, crosshairSize);
                    hasHit = true;
                }
            }             
            else {               
                hasHit = false;
                crosshairRenderer.enabled = false;
                endPos = transform.position + (maxLineLength * transform.TransformDirection(Vector3.forward));
                crosshair.transform.rotation = Quaternion.FromToRotation(crosshair.transform.up, this.transform.forward) * crosshair.transform.rotation;

            }
            if (drawLine) {
                lineRenderer.SetPosition(0, this.transform.position);
                lineRenderer.SetPosition(1, endPos);
            } 
        }
    }


} 

