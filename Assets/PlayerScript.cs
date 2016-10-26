using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Staircase{
	public bool matte;
	public float currentDiff;

	public float baseAngle;

	static float upRate = 1.2f;
	static float downRate = 0.8f;

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
	public GameObject focusPoint;
	public float viewTime;
	public float viewFocusTime;
	public GameObject mask;
	public Text message;

	private GameObject currentPlane;
	private Staircase[] allStaircases;
	private Staircase currentStaircase;
	enum LastViewed {CONSTANT, VARIABLE};
	private LastViewed lastViewed;
	enum Status {VIEWFOCUS, FIRSTSURFACE, SECONDSURFACE, RESPONSE};
	private System.DateTime surfaceStartTime;
	private Status status;//1 = first surface, 2 =  second surface, 3 = display focus sphere, 4 = wait for response


	private bool matte = true;
	private bool focusTime = true;
	private float startingDiff = 15;

	// Use this for initialization
	void Start () {
		allStaircases = new Staircase[1];
		allStaircases [0] = new Staircase (matte, -45, startingDiff);
		changeStaircase ();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			Debug.Log ("click");
			set();
		}
		if (status ==Status.FIRSTSURFACE && (System.DateTime.Now - surfaceStartTime).TotalSeconds > viewTime) {
			status = Status.SECONDSURFACE;
			set ();
		} else if (status == Status.SECONDSURFACE && (System.DateTime.Now - surfaceStartTime).TotalSeconds > viewTime) {
			status = Status.RESPONSE;
			setWait ();
		}else if(status == Status.RESPONSE){
			if (Input.GetKeyDown (KeyCode.LeftControl)) {
				currentStaircase.feedbackRight ();
				changeStaircase ();
			}
			if (Input.GetKeyDown (KeyCode.RightControl)) {
				currentStaircase.feedbackWrong ();
				changeStaircase ();
			}
		}
	}

	void changeStaircase(){
		currentStaircase = allStaircases [0];//fix this to pick random, unfinished staircase
		if (currentStaircase.matte) {
			currentPlane = planeMatte;
		} else {
			currentPlane = planeGlossy;
		}
		mask.SetActive (false);
		message.text = "";
		status = Status.FIRSTSURFACE;
		currentPlane.SetActive (true);
		set ();
	}

	void set(){
		if (status == Status.FIRSTSURFACE) {// pick which pair to show first
			lastViewed = Random.value > 0.5 ? LastViewed.CONSTANT : LastViewed.VARIABLE;
			}
		if (lastViewed == LastViewed.CONSTANT) {
				reset (currentPlane, currentStaircase.baseAngle);
				currentPlane.transform.Rotate (-currentStaircase.currentDiff, 0, 0, Space.World);
				currentPlane.transform.Rotate (0, Random.Range (0, 360), 0, Space.Self);
				currentPlane.transform.Translate (Random.Range (0, 0.25f), 0, Random.Range (0, 0.25f), Space.Self);
			lastViewed = LastViewed.VARIABLE;
			} else {
				reset (currentPlane, currentStaircase.baseAngle);
				currentPlane.transform.Rotate (0, Random.Range (0, 360), 0, Space.Self);
				currentPlane.transform.Translate (Random.Range (0, 0.25f), 0, Random.Range (0, 0.25f), Space.Self);
			lastViewed = LastViewed.CONSTANT;
			}
			surfaceStartTime = System.DateTime.Now;

	}

	void setWait(){
		mask.SetActive (true);
		currentPlane.SetActive (false);
		message.text = "Please make a selection\nleft control for the first surface\t right control for the second surface";
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
