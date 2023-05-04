using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Igloo
{
    public class NetworkMessage {
        public string address;
        public ArrayList argments = new ArrayList();

        public void AddIntArgument      (int value)     {argments.Add(value);}
        public void AddFloatArgument    (float value)   {argments.Add(value);}
        public void AddStringArgument   (string value)  {argments.Add(value);}
        public void AddBoolArgument     (bool value)    {argments.Add(value);}
    }

    public class NetworkManager : MonoBehaviour
    {
        [HideInInspector]
        public int     inPort = 9007;
        public int      InPort { get { return inPort; } }
        [HideInInspector]
        public int     outPort= 9001;
        public int      OutPort { get { return outPort; } }
        [HideInInspector]
        public string   outIp = "127.0.0.1";
        public string   OutIp { get { return outIp; } }


        private void Awake() {
            if (Application.isPlaying && transform.parent == null) DontDestroyOnLoad(this.gameObject);
        }

        public virtual void Setup() {
            Debug.Log("Igloo - Setting up NetworkManager");
            // Setup senders/receivers and bind events
        }

        public virtual void SetSettings(NetworkSettings ns) {
            if (ns == null) return;
            if (ns.inPort != 0) inPort = ns.inPort;
            if (ns.outPort != 0) outPort = ns.outPort;
            if (!string.IsNullOrEmpty(ns.outIP)) inPort = ns.inPort;
        }

        public virtual NetworkSettings GetSettings() {
            NetworkSettings ns = new NetworkSettings();
            ns.inPort   = inPort;
            ns.outPort  = outPort;
            ns.outIP    = outIp;
            return ns;
        }

        /// <summary>
        /// Send network message 
        /// e.g
        ///     Igloo.NetworkMessage msg = new NetworkMessage();
        ///     msg.address = "/test";
        ///     msg.AddIntArgument(1);
        ///     msg.AddFloatArgument(4.0f);
        ///     msg.AddStringArgument("Test!");
        ///     msg.AddBoolArgument(true);
        ///     networkManager.SendMessage(msg);
        /// </summary>
        /// <param name="msg"></param>
        public virtual void SendMessage (Igloo.NetworkMessage msg) {
            // See NetworkManagerOSC.cs for example override method
        }

        #region Player events
        public WalkSpeed OnPlayerWalkSpeed;
        public delegate void WalkSpeed(string name, float speed);


        public RunSpeed OnPlayerRunSpeed;
        public delegate void RunSpeed(string name, float speed);


        public PlayerRotation OnPlayerRotation;
        public delegate void PlayerRotation(string name, Vector3 rotation);


        public PlayerRotationWarper OnPlayerRotationWarper;
        public delegate void PlayerRotationWarper(string name, Vector3 rotation);


        public PlayerPosition OnPlayerPosition;
        public delegate void PlayerPosition(string name, Vector3 position);


        public PlayerRotationInput OnPlayerRotationInput;
        public delegate void PlayerRotationInput(string name, int value);


        public PlayerRotationMode OnPlayerRotationMode;
        public delegate void PlayerRotationMode(string name, int value);


        public PlayerMovementInput OnPlayerMovementInput;
        public delegate void PlayerMovementInput(string name, int value);


        public PlayerMovmentMode OnPlayerMovementMode;
        public delegate void PlayerMovmentMode(string name, int value);


        public PlayerSmoothTime OnPlayerSmoothTime;
        public delegate void PlayerSmoothTime(string name, float value);
        #endregion

        #region GyrOSC events
        public ButtonGYROSC OnButtonGYROSC;
        public delegate void ButtonGYROSC(string name, Vector3 movement);

        public RotationGYROSC OnRotationGYROSC;
        public delegate void RotationGYROSC(string name, Vector3 rotation);
        #endregion

        #region Head Tracking events
        public HeadPosition OnHeadPosition;
        public delegate void HeadPosition(Vector3 postion);

        public HeadRotation OnHeadRotation;
        public delegate void HeadRotation(Vector3 rotation);
        #endregion

        #region OpenVR events
        public VrButtonEvent OnVrButtonEvent;
        public delegate void VrButtonEvent(int deviceID, int buttonID, bool state);

        public VrPadPositionEvent OnVrPadPositionEvent;
        public delegate void VrPadPositionEvent(int deviceID, Vector2 value);

        public VrTriggerEvent OnVrTriggerEvent;
        public delegate void VrTriggerEvent(int deviceID, float value);

        public VrControllerGyroEvent OnVrControllerGyroEvent;
        public delegate void VrControllerGyroEvent(int deviceID, Vector3 rotationEuler);

        public VrControllerPositionEvent OnVrControllerPositionEvent;
        public delegate void VrControllerPositionEvent(int deviceID, Vector3 position);

        public VrTrackerGyroEvent OnVrTrackerGyroEvent;
        public delegate void VrTrackerGyroEvent(int deviceID, Vector3 rotationEuler);

        public VrTrackerPositionEvent OnVrTrackerPositionEvent;
        public delegate void VrTrackerPositionEvent(int deviceID, Vector3 position);

        #endregion

        #region Manager events
        protected void SetHorizontalFOV(float val) {
            if (IglooManager.Instance.igloo){
                IglooManager.Instance.igloo.GetComponent<DisplayManager>().HorizontalFOV = val;
            }
        }
        protected void SetVerticalFOV(float val) {
            if (IglooManager.Instance.igloo){
                IglooManager.Instance.igloo.GetComponent<DisplayManager>().VerticalFOV = val;
            }
        }

        protected void SetDisplaysEnabled(bool val) {
            if (IglooManager.Instance.igloo){
                IglooManager.Instance.igloo.GetComponent<DisplayManager>().SetDisplaysEnabled(val);
            }
        }

        protected void SetNearClip(float val) {
            if (IglooManager.Instance.igloo){
                IglooManager.Instance.igloo.GetComponent<DisplayManager>().SetNearClip(val);
            }
        }

        protected void SetFarClip(float val) {
            if (IglooManager.Instance.igloo){
                IglooManager.Instance.igloo.GetComponent<DisplayManager>().SetFarClip(val);
            }
        }

        protected void SetEyeSeparation(float val) {
            if (IglooManager.Instance.igloo) {
                IglooManager.Instance.igloo.GetComponent<DisplayManager>().SetEyeSeparation(val);
            }
        }

        public SetUIVisible OnSetUIVisible;
        public delegate void SetUIVisible(bool state);

        protected void Quit() {
            Application.Quit();
        }
             
        protected virtual void GetInfo() {
            // Should send info message back to sender with plugin version
            // Igloo.Utils.GetVersion()
        }
        #endregion



        public virtual void OnDestroy() {
            // unbind events
        }
       
    }

}
