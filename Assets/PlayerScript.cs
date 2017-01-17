using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Staircase{
	public bool matte;
	public float currentDiff;
	public float baseAngle;
	public bool finished;

	enum LastFeedback {RIGHT, WRONG};
	private LastFeedback lastFeedback;
	private float[] reversals;
	private int maxReversals = 12;
	private int numReversalsToUse = 10;
	private int reversalCount;

	//0.8 comes from Haomin's code, 2.81 is corresponding value to hit one up-one down value from "Forced-choice staircases with fixed step sizes: asymptotic and small-sample properties"
	static float upRate = 2.81f;
	static float downRate = 0.8f;

	public Staircase(bool matte, float baseAngle, float startingDiff){
		this.matte = matte;
		this.baseAngle = baseAngle;
		this.currentDiff = startingDiff;
		this.reversals = new float[maxReversals];
		this.finished = false;
		this.reversalCount = 0;
	}

	public void feedbackRight ()
	{
		if (lastFeedback != LastFeedback.RIGHT) {
			reversals [reversalCount] = currentDiff;
			reversalCount++;
			lastFeedback = LastFeedback.RIGHT;
			currentDiff = currentDiff * downRate;
		} else {
			currentDiff = currentDiff * downRate;
		}
		if (reversalCount == maxReversals) {
			finished = true;
		}
	}

	public void feedbackWrong ()
	{
		if (lastFeedback != LastFeedback.WRONG) {
			reversals [reversalCount] = currentDiff;
			reversalCount++;
			lastFeedback = LastFeedback.WRONG;
			currentDiff = currentDiff * upRate;
		} else {
			currentDiff = currentDiff * upRate;
		}
		if (reversalCount == maxReversals) {
			finished = true;
		}
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
	public Text errorMessage;

	private GameObject currentPlane;
	private Staircase[] allStaircases;
	private Staircase currentStaircase;
	enum LastViewed {CONSTANT, VARIABLE};
	private LastViewed lastViewed;
	enum Status {VIEWFOCUS, FIRSTSURFACE, SECONDSURFACE, RESPONSE};
	private Status status;
	private System.DateTime surfaceStartTime;
	private System.DateTime focusStartTime;

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
		//errorMessage.text = status.ToString() + "\n" + focusTime.ToString() + "\n" + focusPoint.activeInHierarchy;
		if (focusTime){
			if ((System.DateTime.Now - focusStartTime).TotalSeconds > viewFocusTime) {
				focusTime = false;
				set ();
			}
		} else if (status ==Status.FIRSTSURFACE && (System.DateTime.Now - surfaceStartTime).TotalSeconds > viewTime) {
			status = Status.SECONDSURFACE;
			setFocus ();
		} else if (status == Status.SECONDSURFACE && (System.DateTime.Now - surfaceStartTime).TotalSeconds > viewTime) {
			status = Status.RESPONSE;
			setWait ();
		} else if(status == Status.RESPONSE){
			if (Input.GetKeyDown (KeyCode.LeftControl) ) {
				if (lastViewed == LastViewed.VARIABLE) {
					currentStaircase.feedbackRight ();
					changeStaircase ();
				}else{
					currentStaircase.feedbackWrong ();
					changeStaircase ();
				}
			}
			else if (Input.GetKeyDown (KeyCode.RightControl)) {
				if (lastViewed == LastViewed.CONSTANT) {
					currentStaircase.feedbackRight ();
					changeStaircase ();
				} else {
					currentStaircase.feedbackWrong ();
					changeStaircase ();
				}
			}
		}
	}

	void changeStaircase(){
		currentStaircase = getStaircase();
		if (currentStaircase.matte) {
			currentPlane = planeMatte;
		} else {
			currentPlane = planeGlossy;
		}
		mask.SetActive (false);
		message.text = "";
		status = Status.FIRSTSURFACE;
		setFocus ();
	}

	Staircase getStaircase (){
		Staircase s;
		if (!hasRemainingStaircase ()) {
			return null;
		}else{
			do{
				s = allStaircases[Random.Range(0, allStaircases.Length)];
			}while(s.finished);
		}
		return s;
	}

	bool hasRemainingStaircase ()
	{
		bool hasRemaining = false;
		foreach (Staircase s in allStaircases) {
			if (!s.finished) {
				hasRemaining = true;
				break;
			}
		}
		return hasRemaining;
	}

	void set(){
		focusPoint.SetActive (false);
		currentPlane.SetActive (true);
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

	void setFocus(){
		focusPoint.SetActive (true);
		currentPlane.SetActive (false);
		focusTime = true;
		focusStartTime = System.DateTime.Now;
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
