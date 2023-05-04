using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Igloo {

    [RequireComponent(typeof(NetworkManager))]
    [System.Serializable]
    public class IglooManager : MonoBehaviour
    {
        private Serializer serializer;
        [HideInInspector]
        public Settings settings = null;

        public bool createOnAwake = true;
        public bool saveOnQuit = true;
        public bool dontDestroyOnLoad = false;
        private bool sendStartupMessage = false;

        public GameObject igloo;
        public GameObject cameraPrefab;
        public GameObject followObject;
        [HideInInspector] public GameObject parentObject;

        // network
        private NetworkManager networkManager;
        public  NetworkManager GetNetworkManager() { return networkManager; }

        // player
        private PlayerManager playerManager;
        public  PlayerManager GetPlayerManager() { return playerManager; }

        // display
        private DisplayManager displayManager;
        public  DisplayManager GetDisplayManager() { return displayManager; }

        // ui
        public  Canvas canvasUI;
        public  RectTransform cursorUI;
        public  RenderTexture textureUI;
        private UIManager uiManager;
        public  UIManager GetUIManager() { return uiManager; }

        // window
        public WindowManager windowManager; 

        // system
        private int vSyncMode; 
        private int targetFPS;

        private static IglooManager instance;
        public static IglooManager Instance {
            get {
                if (instance == null) instance = FindObjectOfType<IglooManager>();
                return instance;
            }
        }

        void Awake() {
            if (Application.isPlaying && dontDestroyOnLoad) DontDestroyOnLoad(this.gameObject);

            // network setup
            if (serializer == null) serializer = new Serializer();
            settings = null;
            string path = System.IO.Path.Combine(Application.streamingAssetsPath + "/", "IglooSettings");
            settings = serializer.Load(path);
            networkManager = GetComponent<NetworkManager>();
            if (settings != null) networkManager.SetSettings(settings.NetworkSettings);
            networkManager.Setup();
            if (createOnAwake) CreateIgloo();
        }

        /// <summary>
        /// Creates a new Igloo 
        /// </summary>
        /// <param name="parent">optional parameter which allows to the Igloo prefab instantiated by CreateIgloo() 
        /// to be parented to the given object. If unassigned the prefab will be parented to the 
        /// IglooManager GameObject. </param>
        /// <param name="follow">optional parameter which assigns the followTransform property</param>
        public void CreateIgloo (GameObject parent = null, GameObject follow = null) {

            if (follow) followObject = follow;
            if (parent) parentObject = parent;

            // load settings 
            if (serializer == null){
                serializer = new Serializer();
                settings = null;
            }

            // if settings cannot be loaded , setup using the default cylinder configuration
            if (settings == null) {
                Debug.LogWarning("Igloo - IglooManager could not load settings, using default instead");
                settings = DefaultConfigurations.EquirectangularCylinder();
            }
            else Debug.Log("Igloo settings loaded OK");

            // instantite igloo prefab if not already assigned 
            if (!igloo) {
                igloo = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Igloo"));
                igloo.transform.parent = parent == null ? this.gameObject.transform : parent.transform;
                igloo.transform.localEulerAngles = Vector3.zero;
                igloo.transform.localPosition = Vector3.zero;
            }

            displayManager  = igloo.GetComponent<DisplayManager>();
            playerManager   = igloo.GetComponent<PlayerManager>();
            uiManager       = igloo.GetComponentInChildren<UIManager>();


            Vector3 tempPos = igloo.transform.localPosition;
            Vector3 tempRot = igloo.transform.localEulerAngles;
            playerManager.GetComponent<CharacterController>().enabled = false;
                igloo.transform.localEulerAngles    = tempRot;
                igloo.transform.localPosition       = tempPos;
            playerManager.GetComponent<CharacterController>().enabled = false;

            // display setup
            if (settings.DisplaySettings != null) {
                if (cameraPrefab) displayManager.cameraPrefab = cameraPrefab;
                else displayManager.cameraPrefab = null;
                displayManager.Setup(settings.DisplaySettings);
            }
            else Debug.LogWarning("Igloo - no igloo display setting found");
            
            // player setup
            if (settings.PlayerSettings != null)playerManager.SetSettings(settings.PlayerSettings);
            else Debug.LogWarning("Igloo - no igloo player settings found");

            // follow object
            if (followObject) {
                igloo.GetComponent<Igloo.FollowObjectTransform>().enabled = true;
                igloo.GetComponent<Igloo.FollowObjectTransform>().followObject = followObject;
                igloo.GetComponent<PlayerManager>().UsePlayer = false;
                Debug.Log("Igloo - follow trasform assigned, igloo player system disabled");
            }

            // ui setup
            uiManager.SetSettings(settings.UISettings);
            if (canvasUI && cursorUI && textureUI) {
                uiManager.canvas    = canvasUI;
                uiManager.cursor    = cursorUI;
                uiManager.texture   = textureUI;
                uiManager.Setup();
            }

            if (!GetComponent<WindowManager>()) windowManager = this.gameObject.AddComponent<WindowManager>();
            if (settings.WindowSettings != null) {
                windowManager.SetSettings(settings.WindowSettings);
                windowManager.SetupWindows();
            }

            // system settings
            if (settings.SystemSettings != null) SetSystemSettings(settings.SystemSettings);     
            
        }

        /// <summary>
        /// Removes the Igloo prefab from the scene if it exists
        /// </summary>
        /// <param name="save"></param>
        public void RemoveIgloo(bool save = true) {
            if (igloo) {
                if (save) SaveSettings();
                DestroyImmediate(igloo);
            }            
        }

        /// <summary>
        /// Saves current settings to the IglooSettings
        /// </summary>
        public void SaveSettings() {
            settings = new Settings {version = Utils.GetVersion()};
            if (igloo) settings.DisplaySettings = igloo.GetComponent<DisplayManager>().GetSettings();
            if (igloo) settings.PlayerSettings  = igloo.GetComponent<PlayerManager>().GetSettings();
            if (igloo) settings.UISettings      = GetUIManager().GetSettings();
            settings.SystemSettings = GetSystemSettings();
            if (networkManager) settings.NetworkSettings    = networkManager.GetSettings();
            if (windowManager) {
                WindowSettings wSettings = windowManager.GetSettings();
                if (wSettings != null) settings.WindowSettings = wSettings;
            }
            if (serializer == null) serializer = new Serializer();
            string path = System.IO.Path.Combine(Application.streamingAssetsPath + "/", "IglooSettings");
            if (!Directory.Exists(Application.streamingAssetsPath)) Directory.CreateDirectory(Application.streamingAssetsPath);
            serializer.Save(path, settings);
        }

        private void SetSystemSettings(SystemSettings settings) {
            vSyncMode = settings.vSyncMode;
            targetFPS = settings.targetFPS;
            Application.targetFrameRate = targetFPS;
            if (vSyncMode >= 0  && vSyncMode <= 2) QualitySettings.vSyncCount = vSyncMode;
            sendStartupMessage = settings.sendStartupMessage;
            if (sendStartupMessage) StartCoroutine ("SendStartupMessage");
            Debug.Log("Igloo - system settings loaded, vsync mode: " + vSyncMode + " ,target FPS: " + targetFPS);
        }
        IEnumerator SendStartupMessage() {
            // Wait for camera setup to complete
            yield return new WaitForSeconds(1.5f);
            if (networkManager) {
                Igloo.NetworkMessage msg2 = new NetworkMessage();
                msg2.address = "/externalApplication/selected";
                msg2.AddStringArgument(settings.DisplaySettings.Name);
                networkManager.SendMessage(msg2);

                Igloo.NetworkMessage msg = new NetworkMessage();
                msg.address = "/externalApplication/selected/enabled";
                msg.AddBoolArgument(true);
                networkManager.SendMessage(msg);                
            }
        }

        private SystemSettings GetSystemSettings() {
            SystemSettings settings = new SystemSettings {
                targetFPS = targetFPS,
                vSyncMode = vSyncMode,
                sendStartupMessage = sendStartupMessage
            };
            return settings; 
        }

        private void OnApplicationQuit() {
            //if (saveOnQuit)SaveSettings();
        }

        private void OnDestroy()  {
            Debug.Log("Igloo - Igloo manager destructor called");
            if (saveOnQuit && igloo) SaveSettings();
            if (igloo)Destroy(igloo);
        }

    }
}

