using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

	public GameObject planeMatte;
	public GameObject planeGlossy;

	private GameObject plane;
	enum type {matte, glossy};
	private type t;
	// Use this for initialization
	void Start () {
		t = type.matte;
		plane = Instantiate (planeMatte) as GameObject;
		plane.SetActive (true);
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			Debug.Log ("click");
			reset();
			plane.transform.Rotate (0, Random.Range (0, 360), 0, Space.Self);
			plane.transform.Translate (Random.Range (0, 0.25f), 0, Random.Range (0, 0.25f), Space.Self);
		}
	}

	//reset surface before random rotation/translation
	void reset(){
		
		resetLocal ();
		resetWorld ();
	}

	void resetLocal(){
		plane.transform.localPosition = new Vector3(0, 0, 0);
		plane.transform.localEulerAngles = new Vector3(-90,0,0);
	}

	void resetWorld(){
		plane.transform.eulerAngles = new Vector3(-45, 0, 0);
		plane.transform.position = new Vector3(0, 0, .75f);
	}
}
