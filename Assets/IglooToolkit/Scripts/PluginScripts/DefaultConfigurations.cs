using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace Igloo {
    public static class DefaultConfigurations
    {
        public static Settings CylinderFiveCamera() {
            // Todo 
            return EquirectangularCylinder();
        }

        public static Settings Equirectangular360() {
            // Todo 
            return EquirectangularCylinder();
        }

        public static Settings Cave() {
            Settings settings = new Settings();

            SystemSettings systemSettings = new SystemSettings();
            systemSettings.targetFPS = 0;
            systemSettings.vSyncMode = 0;

            settings.SystemSettings = systemSettings;

            NetworkSettings networkSettings = new NetworkSettings();
            networkSettings.inPort          = 9007;
            networkSettings.outPort         = 9001;
            networkSettings.outIP           = "127.0.0.1";

            settings.NetworkSettings = networkSettings;

            DisplaySettings displaySettings             = new DisplaySettings();
            displaySettings.Name                        = "IglooUnity";
            displaySettings.useCompositeTexture         = false;
            displaySettings.useCubemapToEquirectangular = false;
            displaySettings.useFramepackTopBottom3D     = false;
            displaySettings.textureShareMode            = 0;
            displaySettings.horizontalFOV               = 360f;
            displaySettings.verticalFOV                 = 70f;
            displaySettings.equirectangularTexuteRes    = new Vector2IntItem(8000, 1000);

            HeadSettings headSettings       = new HeadSettings();
            headSettings.headRotationOffset = new Vector3Item(Vector3.zero);
            headSettings.headPositionOffset = new Vector3Item(0, 1, 0);
            headSettings.leftEyeOffset      = new Vector3Item(-0.05f, 0, 0);
            headSettings.rightEyeOffset     = new Vector3Item(0.05f, 0, 0);

            displaySettings.HeadSettings = headSettings;


            int numCams = 4;
            DisplayItem[] displayItems = new DisplayItem[numCams];

            for (int i = 0; i < numCams; i++) {
                DisplayItem displayItem = new DisplayItem();

                displayItem.Name                = displaySettings.Name + (i + 1).ToString();
                displayItem.fov                 = 90f;
                displayItem.cubemapFace         = i;
                displayItem.is3D                = false;
                displayItem.isFisheye           = false;
                displayItem.isRendering         = true;
                displayItem.isRenderTextures    = false;
                displayItem.textureShareMode    = 0;
                displayItem.nearClipPlane       = 0.01f;
                displayItem.farClipPlane        = 1000f;
                if (i < 4) displayItem.cameraRotation = new Vector3Item(0, (i * 90f), 0f);
                displayItem.renderTextureSize   = new Vector2IntItem(2000, 2000);
                displayItem.fisheyeStrength     = new Vector2Item(0, 0);
                displayItem.isOffAxis           = true;
                if (i == 0) {
                    displayItem.viewportRotation = new Vector3Item(0, -90, 0);
                    displayItem.viewportPosition = new Vector3Item(-1,1,0);
                }
                else if (i == 1) {
                    displayItem.viewportRotation = new Vector3Item(0, 0, 0);
                    displayItem.viewportPosition = new Vector3Item(0, 1, 1);
                }
                else if (i == 2) {
                    displayItem.viewportRotation = new Vector3Item(0, 90, 0);
                    displayItem.viewportPosition = new Vector3Item(1, 1, 0);
                }
                else if (i == 3) {
                    displayItem.viewportRotation = new Vector3Item(90, 0, 0);
                    displayItem.viewportPosition = new Vector3Item(0, 0, 0);
                }
                displayItem.viewportSize = new Vector2Item(2.0f, 2.0f);
                displayItem.targetDisplay = i+1;
                displayItems[i] = displayItem;
            }

            displaySettings.Displays = displayItems;

            settings.DisplaySettings = displaySettings;

            PlayerSettings playerSettings   = new PlayerSettings();
            playerSettings.Name             = "player";
            playerSettings.usePlayer        = true;
            playerSettings.rotationInput    = 0;
            playerSettings.rotationMode     = 0;
            playerSettings.movementInput    = 0;
            playerSettings.movementMode     = 2;
            playerSettings.runSpeed         = 10;
            playerSettings.walkSpeed        = 5;
            playerSettings.smoothTime       = 10;
            playerSettings.isCrosshair3D    = false;
            playerSettings.crosshairHideMode = 0;

            settings.PlayerSettings = playerSettings;


            UISettings uiSettings       = new UISettings();
            uiSettings.useUI            = true;
            uiSettings.screenName       = "Cylinder";
            uiSettings.screenPos        = new Vector3Item(Vector3.zero);
            uiSettings.screenRot        = new Vector3Item(Vector3.zero);
            uiSettings.screenScale      = new Vector3Item(6.0f, 2.1f, 6.0f);
            uiSettings.followCrosshair  = true;
            uiSettings.followSpeed      = 10;

            settings.UISettings = uiSettings;

            return settings;

        }

        public static Settings EquirectangularCylinder() {

            Settings settings = new Settings();

            SystemSettings systemSettings = new SystemSettings();
            systemSettings.targetFPS = 0;
            systemSettings.vSyncMode = 0;

            settings.SystemSettings = systemSettings;

            NetworkSettings networkSettings = new NetworkSettings();
            networkSettings.inPort  = 9007;
            networkSettings.outPort = 9001;
            networkSettings.outIP   = "127.0.0.1";

            settings.NetworkSettings = networkSettings;

            DisplaySettings displaySettings = new DisplaySettings();
            displaySettings.Name                        = "IglooUnity";
            displaySettings.useCompositeTexture         = false;
            displaySettings.useCubemapToEquirectangular = true;
            displaySettings.useFramepackTopBottom3D     = false;
            displaySettings.textureShareMode            = 1;
            displaySettings.horizontalFOV               = 360f;
            displaySettings.verticalFOV                 = 70f;
            displaySettings.equirectangularTexuteRes    = new Vector2IntItem(8000, 1000);

            HeadSettings headSettings = new HeadSettings();
            headSettings.headRotationOffset = new Vector3Item(Vector3.zero);
            headSettings.headPositionOffset = new Vector3Item(0, 1.8f, 0);
            headSettings.leftEyeOffset      = new Vector3Item(-0.05f, 0, 0);
            headSettings.rightEyeOffset     = new Vector3Item(0.05f, 0, 0);
            headSettings.headtracking = false;

            displaySettings.HeadSettings = headSettings;


            int numCams = 4;
            DisplayItem[] displayItems = new DisplayItem[numCams];

            for (int i = 0; i < numCams; i++) {
                DisplayItem displayItem = new DisplayItem();

                displayItem.Name                = displaySettings.Name + (i + 1).ToString();
                displayItem.fov                 = 90f;
                displayItem.cubemapFace         = i;
                displayItem.is3D                = false;
                displayItem.isFisheye           = false;
                displayItem.isRendering         = true;
                displayItem.isRenderTextures    = true;
                displayItem.textureShareMode    = 0;
                displayItem.nearClipPlane       = 0.01f;
                displayItem.farClipPlane        = 1000f;
                if (i < 4) displayItem.cameraRotation = new Vector3Item(0, (i * 90f), 0f);
                displayItem.renderTextureSize   = new Vector2IntItem(2000, 2000);
                displayItem.fisheyeStrength     = new Vector2Item(0, 0);
                displayItem.isOffAxis           = false;
                displayItem.viewportRotation    = new Vector3Item(Vector3.zero);
                displayItem.viewportPosition    = new Vector3Item(Vector3.zero);
                displayItem.viewportSize        = new Vector2Item(Vector2.zero);
                displayItem.targetDisplay       = -1;

                displayItems[i] = displayItem;
            }

            displaySettings.Displays = displayItems;

            settings.DisplaySettings = displaySettings;

            PlayerSettings playerSettings = new PlayerSettings();
            playerSettings.Name             = "player";
            playerSettings.usePlayer        = true;
            playerSettings.rotationInput    = 0;
            playerSettings.rotationMode     = 0;
            playerSettings.movementInput    = 0;
            playerSettings.movementMode     = 2;
            playerSettings.runSpeed         = 10;
            playerSettings.walkSpeed        = 5;
            playerSettings.smoothTime       = 10;            
            playerSettings.isCrosshair3D    = false;
            playerSettings.crosshairHideMode = 0;
            
            settings.PlayerSettings = playerSettings;


            UISettings uiSettings = new UISettings();
            uiSettings.useUI            = true;
            uiSettings.screenName       = "Cylinder";
            uiSettings.screenPos        = new Vector3Item(Vector3.zero);
            uiSettings.screenRot        = new Vector3Item(Vector3.zero);
            uiSettings.screenScale      = new Vector3Item(6.0f, 2.1f, 6.0f);
            uiSettings.followCrosshair  = true;
            uiSettings.followSpeed      = 10;

            settings.UISettings = uiSettings;

            return settings;
        }

        public static Settings EquirectangularFull() {

            Settings settings = new Settings();

            SystemSettings systemSettings = new SystemSettings();
            systemSettings.targetFPS = 0;
            systemSettings.vSyncMode = 0;

            settings.SystemSettings = systemSettings;

            NetworkSettings networkSettings = new NetworkSettings();
            networkSettings.inPort = 9007;
            networkSettings.outPort = 9001;
            networkSettings.outIP = "127.0.0.1";

            settings.NetworkSettings = networkSettings;

            DisplaySettings displaySettings = new DisplaySettings();
            displaySettings.Name = "IglooUnity";
            displaySettings.useCompositeTexture = false;
            displaySettings.useCubemapToEquirectangular = true;
            displaySettings.useFramepackTopBottom3D = false;
            displaySettings.textureShareMode = 1;
            displaySettings.horizontalFOV = 360f;
            displaySettings.verticalFOV = 180f;
            displaySettings.equirectangularTexuteRes = new Vector2IntItem(8000, 4000);

            HeadSettings headSettings = new HeadSettings();
            headSettings.headRotationOffset = new Vector3Item(Vector3.zero);
            headSettings.headPositionOffset = new Vector3Item(0, 1.8f, 0);
            headSettings.leftEyeOffset = new Vector3Item(-0.05f, 0, 0);
            headSettings.rightEyeOffset = new Vector3Item(0.05f, 0, 0);
            headSettings.headtracking = false;


            displaySettings.HeadSettings = headSettings;


            int numCams = 6;
            DisplayItem[] displayItems = new DisplayItem[numCams];

            for (int i = 0; i < numCams; i++) {
                DisplayItem displayItem = new DisplayItem();

                displayItem.Name = displaySettings.Name + (i + 1).ToString();
                displayItem.fov = 90f;
                displayItem.cubemapFace = i;
                displayItem.is3D = false;
                displayItem.isFisheye = false;
                displayItem.isRendering = true;
                displayItem.isRenderTextures = true;
                displayItem.textureShareMode = 0;
                displayItem.nearClipPlane = 0.01f;
                displayItem.farClipPlane = 1000f;
                if (i < 4) displayItem.cameraRotation = new Vector3Item(0, (i * 90f), 0f);
                else if (i==4) displayItem.cameraRotation = new Vector3Item(-90f, 0f, 0f);
                else if (i==5) displayItem.cameraRotation = new Vector3Item(90f, 0f, 0f);
                displayItem.renderTextureSize = new Vector2IntItem(2000, 2000);
                displayItem.fisheyeStrength = new Vector2Item(0, 0);
                displayItem.isOffAxis = false;
                displayItem.viewportRotation = new Vector3Item(Vector3.zero);
                displayItem.viewportPosition = new Vector3Item(Vector3.zero);
                displayItem.viewportSize = new Vector2Item(Vector2.zero);
                displayItem.targetDisplay = -1;

                displayItems[i] = displayItem;
            }

            displaySettings.Displays = displayItems;

            settings.DisplaySettings = displaySettings;

            PlayerSettings playerSettings = new PlayerSettings();
            playerSettings.Name = "player";
            playerSettings.usePlayer = true;
            playerSettings.rotationInput = 0;
            playerSettings.rotationMode = 0;
            playerSettings.movementInput = 0;
            playerSettings.movementMode = 2;
            playerSettings.runSpeed = 10;
            playerSettings.walkSpeed = 5;
            playerSettings.smoothTime = 10;
            playerSettings.isCrosshair3D = false;
            playerSettings.crosshairHideMode = 0;

            settings.PlayerSettings = playerSettings;


            UISettings uiSettings = new UISettings();
            uiSettings.useUI = true;
            uiSettings.screenName = "Cylinder";
            uiSettings.screenPos = new Vector3Item(Vector3.zero);
            uiSettings.screenRot = new Vector3Item(Vector3.zero);
            uiSettings.screenScale = new Vector3Item(6.0f, 2.1f, 6.0f);
            uiSettings.followCrosshair = true;
            uiSettings.followSpeed = 10;

            settings.UISettings = uiSettings;

            return settings;
        }

        public static Settings Cube() {

            Settings settings = new Settings();

            SystemSettings systemSettings = new SystemSettings();
            systemSettings.targetFPS = 0;
            systemSettings.vSyncMode = 0;

            settings.SystemSettings = systemSettings;

            NetworkSettings networkSettings = new NetworkSettings();
            networkSettings.inPort = 9007;
            networkSettings.outPort = 9001;
            networkSettings.outIP = "127.0.0.1";

            settings.NetworkSettings = networkSettings;

            DisplaySettings displaySettings = new DisplaySettings();
            displaySettings.Name = "IglooUnity";
            displaySettings.useCompositeTexture = true;
            displaySettings.useCubemapToEquirectangular = false;
            displaySettings.useFramepackTopBottom3D = false;
            displaySettings.textureShareMode = 1;
            displaySettings.horizontalFOV = 360f;
            displaySettings.verticalFOV = 180f;
            displaySettings.equirectangularTexuteRes = new Vector2IntItem(8000, 4000);

            HeadSettings headSettings = new HeadSettings();
            headSettings.headRotationOffset = new Vector3Item(Vector3.zero);
            headSettings.headPositionOffset = new Vector3Item(0, 1.8f, 0);
            headSettings.leftEyeOffset = new Vector3Item(-0.05f, 0, 0);
            headSettings.rightEyeOffset = new Vector3Item(0.05f, 0, 0);
            headSettings.headtracking = false;

            displaySettings.HeadSettings = headSettings;


            int numCams = 4;
            DisplayItem[] displayItems = new DisplayItem[numCams];

            for (int i = 0; i < numCams; i++) {
                DisplayItem displayItem = new DisplayItem();

                displayItem.Name = displaySettings.Name + (i + 1).ToString();
                displayItem.fov = 90f;
                displayItem.cubemapFace = i;
                displayItem.is3D = false;
                displayItem.isFisheye = false;
                displayItem.isRendering = true;
                displayItem.isRenderTextures = true;
                displayItem.textureShareMode = 0;
                displayItem.nearClipPlane = 0.01f;
                displayItem.farClipPlane = 1000f;
                if (i < 4) displayItem.cameraRotation = new Vector3Item(0, (i * 90f), 0f);
                displayItem.renderTextureSize = new Vector2IntItem(2000, 2000);
                displayItem.fisheyeStrength = new Vector2Item(0, 0);
                displayItem.isOffAxis = false;
                displayItem.viewportRotation = new Vector3Item(Vector3.zero);
                displayItem.viewportPosition = new Vector3Item(Vector3.zero);
                displayItem.viewportSize = new Vector2Item(Vector2.zero);
                displayItem.targetDisplay = -1;

                displayItems[i] = displayItem;
            }

            displaySettings.Displays = displayItems;

            settings.DisplaySettings = displaySettings;

            PlayerSettings playerSettings = new PlayerSettings();
            playerSettings.Name = "player";
            playerSettings.usePlayer = true;
            playerSettings.rotationInput = 0;
            playerSettings.rotationMode = 0;
            playerSettings.movementInput = 0;
            playerSettings.movementMode = 2;
            playerSettings.runSpeed = 10;
            playerSettings.walkSpeed = 5;
            playerSettings.smoothTime = 10;
            playerSettings.isCrosshair3D = false;
            playerSettings.crosshairHideMode = 0;

            settings.PlayerSettings = playerSettings;


            UISettings uiSettings = new UISettings();
            uiSettings.useUI = true;
            uiSettings.screenName = "Cylinder";
            uiSettings.screenPos = new Vector3Item(Vector3.zero);
            uiSettings.screenRot = new Vector3Item(Vector3.zero);
            uiSettings.screenScale = new Vector3Item(6.0f, 2.1f, 6.0f);
            uiSettings.followCrosshair = true;
            uiSettings.followSpeed = 10;

            settings.UISettings = uiSettings;

            return settings;
        }
    }

}
