using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IglooExample2 : MonoBehaviour
{
    public GameObject followObject;
    Igloo.IglooManager iglooManager;

    private void Awake() {
        iglooManager = Igloo.IglooManager.Instance;
        if (iglooManager == null) Debug.LogError("Igloo Manager must be added to the Scene");
    }

    public void FollowPlayer() {
        iglooManager.igloo.GetComponent<Igloo.FollowObjectTransform>().enabled = false;
        iglooManager.igloo.GetComponent<Igloo.PlayerManager>().UsePlayer = true;
    }

    public void FollowObject() {
        iglooManager.igloo.GetComponent<Igloo.FollowObjectTransform>().enabled = true;
        iglooManager.igloo.GetComponent<Igloo.FollowObjectTransform>().followObject = followObject;
        iglooManager.igloo.GetComponent<Igloo.PlayerManager>().UsePlayer = false;
    }
}
