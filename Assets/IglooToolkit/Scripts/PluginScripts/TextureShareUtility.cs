using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_WEBGL
using Klak.Ndi;
#endif
#if UNITY_STANDALONE_WIN
using Klak.Spout;
#endif

namespace Igloo {
    public static class TextureShareUtility 
    {
        public enum TextureShareMode {NONE,SPOUT,NDI}
        public static void AddTextureSender(TextureShareMode shareMode, GameObject go, string senderName,ref RenderTexture texture) {
            switch (shareMode) {
                case TextureShareMode.NONE:
                    break;
                case TextureShareMode.SPOUT:
#if UNITY_STANDALONE_WIN
                    if (go.GetComponent<SpoutSender>()) {
                        SpoutSender[] senders = go.GetComponents<SpoutSender>();
                        foreach (SpoutSender sender in senders) {
                            if (sender.senderName == senderName) UnityEngine.Object.DestroyImmediate(sender);
                        }
                    }
                    SpoutSender spoutSender = go.AddComponent<SpoutSender>();
                    spoutSender.senderName = senderName;
                    spoutSender.sourceTexture = texture;
#endif
                    break;
                case TextureShareMode.NDI:
#if !UNITY_WEBGL
                    if (go.GetComponent<NdiSender>()) {
                        NdiSender[] senders = go.GetComponents<NdiSender>();
                        foreach (NdiSender sender in senders) {
                            if (sender.senderName == senderName) UnityEngine.Object.DestroyImmediate(sender);
                        }
                    }
                    NdiSender ndiSender = go.AddComponent<NdiSender>();
                    ndiSender.senderName = senderName;
                    ndiSender.sourceTexture = texture;
                    break;
                default:
#endif
                    break;
            }
        }

        public static void RemoveAllSendersFromObject(GameObject go) {
#if UNITY_STANDALONE_WIN
            if (go.GetComponent<SpoutSender>()) {
                SpoutSender[] senders = go.GetComponents<SpoutSender>();
                foreach (SpoutSender sender in senders) {
                    UnityEngine.Object.DestroyImmediate(sender);
                }
            }
#endif
#if !UNITY_WEBGL
            if (go.GetComponent<NdiSender>()) {
                NdiSender[] senders = go.GetComponents<NdiSender>();
                foreach (NdiSender sender in senders) {
                    UnityEngine.Object.DestroyImmediate(sender);
                }
            }
#endif
        }
    }

}
