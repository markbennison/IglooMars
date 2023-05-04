using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IglooExample7 : MonoBehaviour
{
    Igloo.UIManager uiManager;

    void Start(){
        uiManager = Igloo.IglooManager.Instance.GetUIManager();
        uiManager.SetUIVisible(true);
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.U)) uiManager.SetUIVisible(true);
        if (Input.GetKeyDown(KeyCode.I)) uiManager.SetUIVisible(false);
    }

    public void SetFollowCursor(bool state) {
        uiManager.SetFollowCursor(state);
    }
}
