using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {

    float speed = 0;
    void Start() {
        speed = Random.Range(-100, 100);
    }
	void Update () {
        transform.Rotate(transform.TransformDirection(Vector3.up), speed * Time.deltaTime);		
	}
}
