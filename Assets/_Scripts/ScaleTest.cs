using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleTest : MonoBehaviour {

	public GameObject obj;

	private Vector3 initScale;
	private float initHaloScale;
	private float initZDist;
	// Use this for initialization
	void Start () {
		initScale = obj.transform.localScale;
		initZDist = obj.transform.position.z;
		Transform halo = obj.transform.Find ("light").transform.Find("halo");
		initHaloScale = halo.GetComponent<Light> ().range;
	}

	// Update is called once per frame
	void Update () {
		float scale = obj.transform.position.z / initZDist;
		obj.transform.localScale = new Vector3 (initScale.x * scale, initScale.y * scale, initScale.z * scale);
		Transform halo = obj.transform.Find ("light").transform.Find("halo");
		halo.GetComponent<Light> ().range = initHaloScale * scale;
	}
}
