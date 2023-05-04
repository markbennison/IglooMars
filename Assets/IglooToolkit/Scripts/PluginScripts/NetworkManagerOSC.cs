using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_WEBGL
using extOSC;
#endif

namespace Igloo
{
    public class NetworkManagerOSC : NetworkManager
    {
#if !UNITY_WEBGL
        [HideInInspector]
        public OSCReceiver     oscIn;
        [HideInInspector]
        public OSCTransmitter  oscOut;

        private void Awake() {
            if (Application.isPlaying && transform.parent == null && IglooManager.Instance.dontDestroyOnLoad) DontDestroyOnLoad(this.gameObject);
        }

        public override void Setup() {
            base.Setup();

            if (!oscIn) oscIn = gameObject.AddComponent<OSCReceiver>();
            oscIn.LocalPort = inPort;
            oscIn.Connect();

            if (!oscOut) oscOut = gameObject.AddComponent<OSCTransmitter>();
            oscOut.LocalHost = OutIp;
            oscOut.RemoteHost = outIp;
            oscOut.RemotePort = outPort;

            // Igloo Player messages
            oscIn.Bind("/gameEngine/*/localPosition",       PlayerPositionMessage);
            oscIn.Bind("/gameEngine/*/localEulerRotation",  PlayerRotationMessage);
            oscIn.Bind("/gameEngine/*/walkSpeed",           PlayerWalkSpeedMessage);
            oscIn.Bind("/gameEngine/*/runSpeed",            PlayerRunSpeedMessage);
            oscIn.Bind("/gameEngine/*/rotationInput",       PlayerRotationInputMessage);
            oscIn.Bind("/gameEngine/*/rotationMode",        PlayerRotationModeMessage);
            oscIn.Bind("/gameEngine/*/movementInput",       PlayerMovementInputMessage);
            oscIn.Bind("/gameEngine/*/movementMode",        PlayerMovementModeMessage);
            oscIn.Bind("/gameEngine/*/smoothTime",          PlayerSmoothTimeMessage);

            // Legacy messages for rotation data
            oscIn.Bind("/ximu/euler", WarperRotationMessage);

            // GyrOSC app messages 
            oscIn.Bind("/gyrosc/player/gyro",   GYROSCRotationMessage);
            oscIn.Bind("/gyrosc/player/button", GYROSCButtonMessage);

            // Igloo Manager messages
            oscIn.Bind("/gameEngine/camera/verticalFOV",        VerticalFOVMessage);
            oscIn.Bind("/gameEngine/camera/horizontalFOV",      HorizontalFOVMessage);
            oscIn.Bind("/gameEngine/camera/setDisplaysEnabled", SetDisplaysEnabledMessage);
            oscIn.Bind("/gameEngine/camera/farClipDistance",    FarClipMessage);
            oscIn.Bind("/gameEngine/camera/nearClipDistance",   NearClipMessage);
            oscIn.Bind("/gameEngine/camera/eyeSeparation",      EyeSeparationMessage);

            // Tracker Messages
            oscIn.Bind("/gameEngine/*/headPosition", HeadPositionMessage);
            oscIn.Bind("/gameEngine/*/headRotation", HeadRotationMessage);

            // OpenVR Messages
            
            // Controller messages
            oscIn.Bind("/openVRController/*/button/*",      OpenVrButtonMessage);
            oscIn.Bind("/openVRController/*/pad/*",         OpenVrPadPositionMessage);
            oscIn.Bind("/openVRController/*/trigger/*",     OpenVrTriggerMessage);
           
            oscIn.Bind("/openVRController/*/gyroEuler/*",   OpenVrControllerGyroMessage);
            oscIn.Bind("/openVRController/*/position/*",   OpenVrControllerPositionMessage);

            oscIn.Bind("/openVRTracker/*/gyroEuler/*", OpenVrTrackerGyroMessage);
            oscIn.Bind("/openVRTracker/*/postition/*", OpenVrTrackerPositionMessage);


            // System
            oscIn.Bind("/gameEngine/quit",      QuitMessage);
            oscIn.Bind("/gameEngine/getInfo",   GetInfoMessage);
        }

        public override void SetSettings(NetworkSettings ns) {
            base.SetSettings(ns);
        }

        public override NetworkSettings GetSettings() {
            return base.GetSettings();
        }

        public override void SendMessage (Igloo.NetworkMessage msg) {
            var msgOSC = new OSCMessage(msg.address);
            foreach (object obj in msg.argments) {
                if (obj is int){
                    msgOSC.AddValue(OSCValue.Int((int)obj));
                }
                else if (obj is float){
                    msgOSC.AddValue(OSCValue.Float((float)obj));
                }
                else if (obj is bool){
                    msgOSC.AddValue(OSCValue.Bool((bool)obj));
                }
                else if (obj is string){
                    msgOSC.AddValue(OSCValue.String((string)obj));
                }
                else Debug.Log("Igloo - NetworkManager - Unknown argument type");
            }
            oscOut.Send(msgOSC);
        }

        #region Player events
        private void PlayerWalkSpeedMessage(OSCMessage msg) {
            string playerName = Utils.StringSplitter(msg.Address, new char[] { '/' }, 2);
            if (OnPlayerWalkSpeed != null) OnPlayerWalkSpeed(playerName, msg.Values[0].FloatValue);
        }

        private void PlayerRunSpeedMessage(OSCMessage msg) {
            string playerName = Utils.StringSplitter(msg.Address, new char[] { '/' }, 2);
            if (OnPlayerRunSpeed!=null) OnPlayerRunSpeed(playerName, msg.Values[0].FloatValue);
        }

        private void PlayerRotationMessage(OSCMessage msg) {
            string playerName = Utils.StringSplitter(msg.Address, new char[] { '/' }, 2);
            if (OnPlayerRotation != null) OnPlayerRotation(playerName, new Vector3(msg.Values[0].FloatValue, msg.Values[1].FloatValue, msg.Values[2].FloatValue));
        }

        private void WarperRotationMessage(OSCMessage msg) {
            if (OnPlayerRotationWarper != null) OnPlayerRotationWarper("player", new Vector3(msg.Values[0].FloatValue, -msg.Values[2].FloatValue, msg.Values[1].FloatValue));
        }

        private void PlayerPositionMessage(OSCMessage msg) {
            string playerName = Utils.StringSplitter(msg.Address, new char[] { '/' }, 2);
            if (OnPlayerPosition != null) OnPlayerPosition(playerName, new Vector3(msg.Values[0].FloatValue, msg.Values[1].FloatValue, msg.Values[2].FloatValue));
        }

        private void PlayerRotationInputMessage(OSCMessage msg) {
            string playerName = Utils.StringSplitter(msg.Address, new char[] { '/' }, 2);
            if (OnPlayerRotationInput != null) OnPlayerRotationInput(playerName, msg.Values[0].IntValue);
        }

        private void PlayerRotationModeMessage(OSCMessage msg) {
            string playerName = Utils.StringSplitter(msg.Address, new char[] { '/' }, 2);
            if (OnPlayerRotationMode != null) OnPlayerRotationMode(playerName, msg.Values[0].IntValue);
        }

        private void PlayerMovementInputMessage(OSCMessage msg) {
            string playerName = Utils.StringSplitter(msg.Address, new char[] { '/' }, 2);
            if (OnPlayerMovementInput != null) OnPlayerMovementInput(playerName, msg.Values[0].IntValue);
        }

        private void PlayerMovementModeMessage(OSCMessage msg) {
            string playerName = Utils.StringSplitter(msg.Address, new char[] { '/' }, 2);
            if (OnPlayerMovementMode != null) OnPlayerMovementMode(playerName, msg.Values[0].IntValue);
        }

        private void PlayerSmoothTimeMessage(OSCMessage msg) {
            string playerName = Utils.StringSplitter(msg.Address, new char[] { '/' }, 2);
            if (OnPlayerSmoothTime != null) OnPlayerSmoothTime(playerName, msg.Values[0].FloatValue);
        }

        #endregion

        #region Manager events
        private void HorizontalFOVMessage(OSCMessage msg) {
            base.SetHorizontalFOV(msg.Values[0].FloatValue);
        }

        private void VerticalFOVMessage(OSCMessage msg) {
            base.SetVerticalFOV(msg.Values[0].FloatValue);
        }

        private void SetDisplaysEnabledMessage(OSCMessage msg) {
            bool state = false;
            if (GetBoolValue(msg.Values[0], out state)) base.SetDisplaysEnabled(state);            
        }

        private void NearClipMessage(OSCMessage msg) {
            base.SetNearClip(msg.Values[0].FloatValue);
        }

        private void FarClipMessage(OSCMessage msg){
            base.SetFarClip(msg.Values[0].FloatValue);
        }

        private void EyeSeparationMessage(OSCMessage msg) {
            base.SetEyeSeparation(msg.Values[0].FloatValue);
        }

        private void QuitMessage(OSCMessage msg) {
            base.Quit();
        }

        private void GetInfoMessage(OSCMessage msg) {
            OSCMessage newMessage = new OSCMessage("/gameEngine/info");
            newMessage.AddValue(OSCValue.String(Igloo.Utils.GetVersion()));
            if (oscOut)oscOut.Send(newMessage);
        }

        private void SetUIVisibleMessage(OSCMessage msg) {
            if (OnSetUIVisible != null) OnSetUIVisible(msg.Values[0].BoolValue);
        }

        #endregion

        #region GyrOSC events
        void GYROSCButtonMessage(OSCMessage message) {
            Vector3 movementAxis = new Vector3();
            if ((int)message.Values[1].IntValue == 1 && (int)message.Values[0].IntValue == 1) movementAxis.x = 1;
            if ((int)message.Values[1].IntValue == 0 && (int)message.Values[0].IntValue == 1) movementAxis.x = 0;
            if ((int)message.Values[1].IntValue == 1 && (int)message.Values[0].IntValue == 2) movementAxis.x = -1;
            if ((int)message.Values[1].IntValue == 0 && (int)message.Values[0].IntValue == 2) movementAxis.x = 0;

            if ((int)message.Values[1].IntValue == 1 && (int)message.Values[0].IntValue == 3) movementAxis.z = -1;
            if ((int)message.Values[1].IntValue == 0 && (int)message.Values[0].IntValue == 3) movementAxis.z = 0;
            if ((int)message.Values[1].IntValue == 1 && (int)message.Values[0].IntValue == 9) movementAxis.z = 1;
            if ((int)message.Values[1].IntValue == 0 && (int)message.Values[0].IntValue == 9) movementAxis.z = 0;

            if ((int)message.Values[1].IntValue == 1 && (int)message.Values[0].IntValue == 8) movementAxis.y = -1;
            if ((int)message.Values[1].IntValue == 0 && (int)message.Values[0].IntValue == 8) movementAxis.y = 0;
            if ((int)message.Values[1].IntValue == 1 && (int)message.Values[0].IntValue == 7) movementAxis.y = 1;
            if ((int)message.Values[1].IntValue == 0 && (int)message.Values[0].IntValue == 7) movementAxis.y = 0;

            if (OnButtonGYROSC != null) OnButtonGYROSC("player", movementAxis);
        }

        void GYROSCRotationMessage(OSCMessage message) {
            Vector3 rotation = new Vector3();
            float x = message.Values[0].FloatValue;
            float z = message.Values[1].FloatValue;
            float y = message.Values[2].FloatValue;

            rotation.x = -((x / (Mathf.PI / 2)) * 65.0f);
            rotation.y = -(y / Mathf.PI) * 180.0f;
            rotation.z = -((z / (Mathf.PI / 2)) * 65.0f);

            if (OnRotationGYROSC != null) OnRotationGYROSC("player", rotation);
        }
        #endregion

        #region Head Tracking events
        private void HeadPositionMessage(OSCMessage msg) {
            if (OnHeadPosition != null) OnHeadPosition(new Vector3(msg.Values[0].FloatValue, msg.Values[1].FloatValue, msg.Values[2].FloatValue));
        }
        private void HeadRotationMessage(OSCMessage msg) {
            if (OnHeadRotation != null) OnHeadRotation(new Vector3(msg.Values[0].FloatValue, msg.Values[1].FloatValue, msg.Values[2].FloatValue));
        }
        #endregion

        #region OpenVR events
        private void OpenVrButtonMessage(OSCMessage msg) {
            int deviceID = -1;
            int buttonID = -1;
            int.TryParse(Utils.StringSplitter(msg.Address, new char[] { '/' }, 2), out deviceID);
            int.TryParse(Utils.StringSplitter(msg.Address, new char[] { '/' }, 4), out buttonID);           
            if (OnVrButtonEvent != null) OnVrButtonEvent(deviceID,buttonID,msg.Values[0].BoolValue);
        }
        private void OpenVrPadPositionMessage(OSCMessage msg) {
            int deviceID = -1;
            int.TryParse(Utils.StringSplitter(msg.Address, new char[] { '/' }, 2), out deviceID);
            if (OnVrPadPositionEvent != null) OnVrPadPositionEvent(deviceID, new Vector2(msg.Values[0].FloatValue, msg.Values[1].FloatValue));
        }
        private void OpenVrTriggerMessage(OSCMessage msg) {
            int deviceID = -1;
            int.TryParse(Utils.StringSplitter(msg.Address, new char[] { '/' }, 2), out deviceID);
            if (OnVrTriggerEvent != null) OnVrTriggerEvent(deviceID, msg.Values[0].FloatValue);
        }
        private void OpenVrControllerGyroMessage(OSCMessage msg) {
            int deviceID = -1;
            int.TryParse(Utils.StringSplitter(msg.Address, new char[] { '/' }, 2), out deviceID);
            if (OnVrControllerGyroEvent != null) OnVrControllerGyroEvent(deviceID, new Vector3(msg.Values[0].FloatValue, msg.Values[1].FloatValue, msg.Values[2].FloatValue));
        }
        private void OpenVrControllerPositionMessage(OSCMessage msg) {
            int deviceID = -1;
            int.TryParse(Utils.StringSplitter(msg.Address, new char[] { '/' }, 2), out deviceID);
            if (OnVrControllerPositionEvent != null) OnVrControllerPositionEvent(deviceID, new Vector3(msg.Values[0].FloatValue, msg.Values[1].FloatValue, msg.Values[2].FloatValue));
        }
        private void OpenVrTrackerPositionMessage(OSCMessage msg) {
            int deviceID = -1;
            int.TryParse(Utils.StringSplitter(msg.Address, new char[] { '/' }, 2), out deviceID);
            if (OnVrTrackerPositionEvent != null) OnVrTrackerPositionEvent(deviceID, new Vector3(msg.Values[0].FloatValue, msg.Values[1].FloatValue, msg.Values[2].FloatValue));
        }
        private void OpenVrTrackerGyroMessage(OSCMessage msg) {
            int deviceID = -1;
            int.TryParse(Utils.StringSplitter(msg.Address, new char[] { '/' }, 2), out deviceID);
            if (OnVrTrackerGyroEvent != null) OnVrTrackerGyroEvent(deviceID, new Vector3(msg.Values[0].FloatValue, msg.Values[1].FloatValue, msg.Values[2].FloatValue));
        }
        #endregion

        #region OSC Utility functions
        private bool GetIntValue(OSCValue value, out int i)
        {
            bool ok = false;
            switch (value.Type)
            {
                case OSCValueType.Int:
                    i = value.IntValue;
                    ok = true;
                    break;
                case OSCValueType.True:
                    i = 1;
                    ok = true;
                    break;
                case OSCValueType.False:
                    i = 0;
                    ok = true;
                    break;
                case OSCValueType.Float:
                    i = (int)value.FloatValue;
                    ok = true;
                    break;
            }
            i = -1;
            Debug.LogWarning("Can't convert OSC parameter, likely incorrect type or value sent");
            return ok;
        }

        private bool GetFloatValue(OSCValue value, out float f)
        {
            bool ok = false; 
            switch (value.Type)
            {
                case OSCValueType.Float:
                    f = value.FloatValue;
                    ok = true;
                    break;
                case OSCValueType.True:
                    f = 1.0f;
                    ok  = true;
                    break;
                case OSCValueType.False:
                    f = 0.0f;
                    ok = true;
                    break;
                case OSCValueType.Int:
                    f = (float)value.IntValue;
                    break;
            }
            f = -1.0f;
            Debug.LogWarning("Can't convert OSC parameter, likely incorrect type or value sent");
            return ok;
        }

        private bool GetBoolValue(OSCValue value, out bool b)
        {
            bool ok = false;
            switch (value.Type)
            {
                case OSCValueType.Int:
                    if (value.IntValue == 0)
                    {
                        b = false;
                        ok = true;
                    }
                    else if (value.IntValue == 1)
                    {
                        b = true;
                        ok = true;
                    }
                    break;
                case OSCValueType.True:
                    b = true;
                    ok = true;
                    break;
                case OSCValueType.False:
                    b = false;
                    ok = true;
                    break;
                case OSCValueType.Float:
                    if (value.FloatValue == 0.0f)
                    {
                        b = false;
                        ok = true;
                    }
                    else if (value.FloatValue == 1.0f)
                    {
                        b = true;
                        ok = true;
                    }
                    break;
            }
            b = false;
            Debug.LogWarning("Can't convert OSC parameter, likely incorrect type or value sent");
            return ok;
        }
        #endregion



        public override void OnDestroy() {
            if (oscIn)oscIn.UnbindAll();
        }
#endif       
    }

}
