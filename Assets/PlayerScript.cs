using UnityEngine;
using System.Collections;

public class Staircase{
	public bool matte;
	public float currentDiff;

	public float baseAngle;

	static float upRate = 1.5f;
	static float downRate = 0.5f;

	public Staircase(bool matte, float baseAngle, float startingDiff){
		this.matte = matte;
		this.baseAngle = baseAngle;
		this.currentDiff = startingDiff;
	}

	public void feedbackRight(){
		currentDiff = currentDiff * downRate;
	}

	public void feedbackWrong(){
		currentDiff = currentDiff * upRate;
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
	private float startingDiff = 15;
	// Use this for initialization
	void Start () {
		staircase = new Staircase (matte, -45, startingDiff);
		setFirst ();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			Debug.Log ("click");
			set();
		}
		if(Input.GetKeyDown(KeyCode.LeftControl)){
			staircase.feedbackRight ();
			set ();
		}
		if (Input.GetKeyDown (KeyCode.RightControl)) {
			staircase.feedbackWrong ();
			set ();
		}
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			Transform t = planeConstant.transform.GetChild(0);
			t.Translate (-5, 0, 0);
			//Debug.Log(planeConstant.transform.childCount);
		}
			
	}

	void setFirst(){
		planeConstant = Instantiate (planeMatte) as GameObject;
		planeVariable = Instantiate (planeMatte) as GameObject;
		reset (planeConstant, staircase.baseAngle);
		planeConstant.transform.Rotate (0, Random.Range (0, 360), 0, Space.Self);
		planeConstant.transform.Translate (Random.Range (0, 0.25f), 0, Random.Range (0, 0.25f), Space.Self);
		reset (planeVariable, staircase.baseAngle);
		planeVariable.transform.Rotate((Random.value > 0.5 ? staircase.currentDiff : -staircase.currentDiff), 0, 0, Space.World);
		planeVariable.transform.Rotate (0, Random.Range (0, 360), 0, Space.Self);
		planeVariable.transform.Translate (Random.Range (0, 0.25f), 0, Random.Range (0, 0.25f), Space.Self);

		planeConstant.SetActive (true);
		lastViewed = LastViewed.constant;
	}

	void set(){
		reset (planeConstant, staircase.baseAngle);
		planeConstant.transform.Rotate (0, Random.Range (0, 360), 0, Space.Self);
		planeConstant.transform.Translate (Random.Range (0, 0.25f), 0, Random.Range (0, 0.25f), Space.Self);
		reset (planeVariable, staircase.baseAngle);
		planeVariable.transform.Rotate((Random.value > 0.5 ? staircase.currentDiff : -staircase.currentDiff), 0, 0, Space.World);
		planeVariable.transform.Rotate (0, Random.Range (0, 360), 0, Space.Self);
		planeVariable.transform.Translate (Random.Range (0, 0.25f), 0, Random.Range (0, 0.25f), Space.Self);
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
		plane.transform.localPosition = new Vector3(0,0,0);
		plane.transform.localEulerAngles = new Vector3 (-90, 0, 0);
	}

	void resetWorld(GameObject plane, float baseAngle){
		plane.transform.eulerAngles = new Vector3(baseAngle, 0, 0);
		plane.transform.position = new Vector3 (0, 0, 0.75f);
	}
}
