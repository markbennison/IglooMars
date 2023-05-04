using UnityEngine;
using UnityEditor;
using System.Diagnostics;

namespace Igloo
{
    [CustomEditor(typeof(IglooManager))]
    public class IglooManagerEditor : Editor
    {
        Texture logo;
        bool advanced   = false;
        //bool showUI     = false;

        public override void OnInspectorGUI() {
            if (logo) {
                GUILayout.BeginHorizontal();
                GUILayout.Box(logo, GUILayout.Width(200), GUILayout.Height(108), GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
            }
            else {
                bool hasPro = UnityEditorInternal.InternalEditorUtility.HasPro();
                if (hasPro) logo = Resources.Load<Texture>("IglooLogoWhite");
                else logo = Resources.Load<Texture>("IglooLogoBlack");
            }

            DrawDefaultInspector();

            IglooManager manager = (IglooManager)target;

            //manager.createOnAwake   = EditorGUILayout.Toggle("Create On Awake", manager.createOnAwake);
            //manager.saveOnQuit      = EditorGUILayout.Toggle("Save On Quit", manager.saveOnQuit);
            //GUILayout.Space(10);
            //manager.igloo           = (GameObject)EditorGUILayout.ObjectField("Igloo Prefab", manager.igloo, typeof(GameObject), true);
            //manager.cameraPrefab    = (GameObject)EditorGUILayout.ObjectField("Camera Prefab", manager.cameraPrefab, typeof(GameObject), true);
            //manager.followObject    = (GameObject)EditorGUILayout.ObjectField("Follow Object", manager.followObject, typeof(GameObject), true);
            //
            //GUILayout.Space(10);
            //GUILayout.Label("3D UI Settings", EditorStyles.boldLabel);
            //GUILayout.Space(5);
            //showUI = EditorGUILayout.Toggle("Use 3D UI ", showUI);
            //if (showUI) {
            //    manager.canvasUI    = (Canvas)EditorGUILayout.ObjectField("UI Canvas", manager.canvasUI, typeof(Canvas), true); 
            //    manager.cursorUI    = (RectTransform)EditorGUILayout.ObjectField("UI Cursor", manager.cursorUI, typeof(RectTransform), true); 
            //    manager.textureUI   = (RenderTexture)EditorGUILayout.ObjectField("UI Texture", manager.textureUI, typeof(RenderTexture), true); 
            //}
            
            GUILayout.Space(20);
            if (GUILayout.Button("Open Settings File")) {       
                string path = System.IO.Path.Combine(Application.streamingAssetsPath, "IglooSettings.xml");
                Process.Start(path);
            }

            GUILayout.Space(10f);

            if (Utils.IsAdvancedMode()) {
                if (advanced) {
                    if (GUILayout.Button("Hide Advanced")) {
                        advanced = false;
                    }
                    GUILayout.Space(10f);

                    if (GUILayout.Button("Save current settings")){
                        manager.SaveSettings();
                    }
                }
                else {
                    if (GUILayout.Button("Show Advanced")) {
                        advanced = true;

                    }
                }
            }
           
        }
    }
}

