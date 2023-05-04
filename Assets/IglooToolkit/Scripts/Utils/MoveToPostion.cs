using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPostion : MonoBehaviour
{
    public Vector3[] positions;
    public float speed = 1;
    private int currentPos = 0;
    void Update()
    {
        if (positions.Length > 0) {
            float step = speed * Time.deltaTime; 
            transform.position = Vector3.MoveTowards(transform.position, positions[currentPos], step); 
            if (transform.position == positions[currentPos]) currentPos += 1;
            currentPos = currentPos%positions.Length;
        }
    }
}
