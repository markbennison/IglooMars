using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Igloo {
    public class VirtualDisplay : Display
    {
        public bool isTextureSharing;
        public TextureShareUtility.TextureShareMode textureShareMode;
        public int targetDisplay = -1;

        public override void SetSettings(DisplayItem settings) {
            base.SetSettings(settings);
            textureShareMode = (TextureShareUtility.TextureShareMode)settings.textureShareMode;
            targetDisplay = settings.targetDisplay;
        }

        public override DisplayItem GetSettings() {
            DisplayItem settings = base.GetSettings();
            settings.textureShareMode = (int)textureShareMode;
            settings.targetDisplay = targetDisplay;
            return settings;
        }

        public override void SetupDisplay() {
            base.SetupDisplay();
            foreach (var cam in activeCameras) {
                if (cam.Key == EYE.LEFT) {
                    //if (cam.Value.targetTexture != null) TextureShareUtility.AddTextureSender
                    //            (textureShareMode, this.gameObject, Name + "_"  + cam.Key, ref leftTexture);

                    // Sequencial naming
                    if (cam.Value.targetTexture != null) TextureShareUtility.AddTextureSender
                            (textureShareMode, this.gameObject, Name, ref leftTexture);
                }
                if (cam.Key == EYE.CENTER) {
                    if (cam.Value.targetTexture != null) TextureShareUtility.AddTextureSender
                            (textureShareMode, this.gameObject, Name, ref centerTexture);
                }
                if (cam.Key == EYE.RIGHT) {
                    //if (cam.Value.targetTexture != null) TextureShareUtility.AddTextureSender
                    //            (textureShareMode, this.gameObject, Name + "_" + cam.Key, ref rightTexture);

                    // Sequencial naming
                    string nameTrimmed = Name.TrimEnd(Name[Name.Length - 1]);
                    char lastChar = Name[Name.Length - 1];
                    int camIndex = (int)char.GetNumericValue(lastChar);

                    camIndex += IglooManager.Instance.settings.DisplaySettings.Displays.Length;

                    string newName = nameTrimmed + camIndex.ToString();
                    if (cam.Value.targetTexture != null) TextureShareUtility.AddTextureSender
                            (textureShareMode, this.gameObject, newName, ref rightTexture);
                }
            }          
        }

        public override void InitialiseCameras() {
            if (is3D) {
                Camera leftCam = headManager.CreateEye(EYE.LEFT, Name , isOffAxis ? this.transform.localEulerAngles : camRotation, cameraPrefab);
                Camera rightCam = headManager.CreateEye(EYE.RIGHT, Name, isOffAxis ? this.transform.localEulerAngles : camRotation,  cameraPrefab , leftCam.transform.parent);
                if (targetDisplay >= 0) {
                    leftCam.targetDisplay = targetDisplay;
                    //leftCam.stereoTargetEye = StereoTargetEyeMask.Left;
                    rightCam.targetDisplay = targetDisplay;
                    //rightCam.stereoTargetEye = StereoTargetEyeMask.Right;
                    if (UnityEngine.Display.displays.Length > targetDisplay) UnityEngine.Display.displays[targetDisplay].Activate();
                }
                activeCameras.Add(EYE.LEFT, leftCam);
                activeCameras.Add(EYE.RIGHT, rightCam);
            }
            else {
                Camera centerCam = headManager.CreateEye(EYE.CENTER, "Camera - " + name, isOffAxis ? Vector3.zero : camRotation,cameraPrefab);
                activeCameras.Add(EYE.CENTER, centerCam);
                if (targetDisplay >= 0)
                {
                    centerCam.targetDisplay = targetDisplay;
                    if (UnityEngine.Display.displays.Length > targetDisplay) UnityEngine.Display.displays[targetDisplay].Activate();
                }
            }
        }
    }
          
}

