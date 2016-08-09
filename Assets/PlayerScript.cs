using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

	public Transform plane;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			Debug.Log ("click");
			reset();
			plane.Rotate (0, Random.Range (0, 360), 0);
			plane.transform.Translate (Random.Range (0, 2), 0, Random.Range (0, 2));
		}
	}

	//reset surface before random rotation/translation
	void reset(){
		resetLocal ();
		resetWorld ();
	}

	void resetLocal(){
		plane.transform.localPosition = new Vector3(0, 0, 0);
		plane.transform.localEulerAngles = new Vector3(0,0,0);
	}

	void resetWorld(){
		plane.transform.eulerAngles = new Vector3(45, 180, 0);
		plane.transform.position = new Vector3(0, 0, .75f);
	}
}
