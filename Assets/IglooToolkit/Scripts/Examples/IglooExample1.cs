using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IglooExample1 : MonoBehaviour
{
    Igloo.IglooManager iglooManager;

    private void Awake() {
        iglooManager = Igloo.IglooManager.Instance;
    }

    public void CreateIgloo() {
        if (iglooManager)iglooManager.CreateIgloo();
    }

    public void RemoveIgloo() {
        if (iglooManager)iglooManager.RemoveIgloo();
    }

}
