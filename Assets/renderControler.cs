using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class renderControler : MonoBehaviour {


	public Transform initCameraPos;
	
	// Update is called once per frame
	void Update () {
		GetComponent<Renderer>().material.SetVector("_initCameraPos", new Vector4(initCameraPos.position.x, initCameraPos.position.y, initCameraPos.position.z, 1f));
//		GetComponent<Renderer>().material.SetVector("_initCameraPos", new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
	}
}
