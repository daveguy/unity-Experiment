using UnityEngine;
using System.Collections;

public class Staircase{
	public bool matte;
	public float currentDiff;

	public float baseAngle;

	static float upRate = 0.5f;
	static float downRate = 0.5f;

	public Staircase(bool matte, float baseAngle, float startingDiff){
		this.matte = matte;
		this.baseAngle = baseAngle;
		this.currentDiff = startingDiff;
	}
}

public class PlayerScript : MonoBehaviour {

	public GameObject planeMatte;
	public GameObject planeGlossy;

	private GameObject planeConstant;
	private GameObject planeVariable;
	private Staircase staircase;

	enum LastViewed {constant, variable};
	private LastViewed lastViewed;
	private bool matte = true;
	private float startingDiff = 30;
	// Use this for initialization
	void Start () {
		staircase = new Staircase (matte, -45, startingDiff);
		setFirst ();
		//set ();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			Debug.Log ("click");
			set();
		}
	}

	void setFirst(){
		planeConstant = Instantiate (planeMatte) as GameObject;
		planeVariable = Instantiate (planeMatte) as GameObject;
		reset (planeConstant, staircase.baseAngle);
		planeConstant.transform.Rotate (0, Random.Range (0, 360), 0, Space.Self);
		planeConstant.transform.Translate (Random.Range (0, 0.25f), 0, Random.Range (0, 0.25f), Space.Self);
		reset (planeVariable, staircase.baseAngle);
		planeVariable.transform.Rotate (0, Random.Range (0, 360), 0, Space.Self);
		planeVariable.transform.Translate (Random.Range (0, 0.25f), 0, Random.Range (0, 0.25f), Space.Self);
		planeVariable.transform.Rotate((Random.value > 0.5 ? staircase.currentDiff : -staircase.currentDiff), 0, 0, Space.World);

		planeConstant.SetActive (true);
		lastViewed = LastViewed.constant;
	}

	void set(){
		reset (planeConstant, staircase.baseAngle);
		planeConstant.transform.Rotate (0, Random.Range (0, 360), 0, Space.Self);
		planeConstant.transform.Translate (Random.Range (0, 0.25f), 0, Random.Range (0, 0.25f), Space.Self);
		reset (planeVariable, staircase.baseAngle);
		planeVariable.transform.Rotate (0, Random.Range (0, 360), 0, Space.Self);
		planeVariable.transform.Translate (Random.Range (0, 0.25f), 0, Random.Range (0, 0.25f), Space.Self);
		planeVariable.transform.Rotate((Random.value > 0.5 ? staircase.currentDiff : -staircase.currentDiff), 0, 0, Space.World);
		if (lastViewed == LastViewed.constant) {
			planeConstant.SetActive (false);
			planeVariable.SetActive (true);
			lastViewed = LastViewed.variable;
		} else {
			planeConstant.SetActive (true);
			planeVariable.SetActive (false);
			lastViewed = LastViewed.constant;
		}
	}

	//reset surface before random rotation/translation
	void reset(GameObject plane, float baseAngle){
		
		resetLocal (plane);
		resetWorld (plane, baseAngle);
	}
	//helper methods for reseting, might have to be changed because blender's sometimes a pain
	void resetLocal(GameObject plane){
		//plane.transform.localPosition.Set(0, 0, 0);
		//plane.transform.localEulerAngles.Set(-90,0,0);
		plane.transform.localPosition = new Vector3(0,0,0);
		plane.transform.localEulerAngles = new Vector3 (-90, 0, 0);
	}

	void resetWorld(GameObject plane, float baseAngle){
		//plane.transform.eulerAngles.Set(baseAngle, 0, 0);
		//plane.transform.position.Set(0, 0, .75f);
		plane.transform.eulerAngles = new Vector3(baseAngle, 0, 0);
		plane.transform.position = new Vector3 (0, 0, 0.75f);
	}
}
