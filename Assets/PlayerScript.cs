using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
	public float viewTime;
	public GameObject mask;
	public Text message;

	private GameObject planeConstant;
	private GameObject planeVariable;
	private Staircase staircase;
	enum LastViewed {constant, variable};
	private LastViewed lastViewed;
	private System.DateTime surfaceStartTime;

	private int status;//1 = first surface, 2 =  second surface, 3 = wait for choice

	private bool matte = true;
	private float startingDiff = 15;
	// Use this for initialization
	void Start () {
		staircase = new Staircase (matte, -45, startingDiff);
		planeConstant = Instantiate (planeMatte) as GameObject;
		planeVariable = Instantiate (planeMatte) as GameObject;
		mask.SetActive (false);
		message.text = "";
		status = 1;
		set ();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			Debug.Log ("click");
			set();
		}
		message.text = status.ToString() + "      " + lastViewed.ToString();
		if (status == 1 && (System.DateTime.Now - surfaceStartTime).TotalSeconds > viewTime) {
			status = 2;
			set ();
		} else if (status == 2 && (System.DateTime.Now - surfaceStartTime).TotalSeconds > viewTime) {
			status = 3;
			setWait ();
		} else if(status == 3){
			if (Input.GetKeyDown (KeyCode.LeftControl)) {
				staircase.feedbackRight ();
				mask.SetActive (false);
				message.text = "";
				status = 1;
				set ();
			}
			if (Input.GetKeyDown (KeyCode.RightControl)) {
				staircase.feedbackWrong ();
				mask.SetActive (false);
				message.text = "";
				status = 1;
				set ();
			}
		}
	}

	void set(){
		if (status == 1) {// pick which pair to show first
			lastViewed = Random.value > 0.5 ? LastViewed.constant : LastViewed.variable;
		}
		if (lastViewed == LastViewed.constant) {
			reset (planeVariable, staircase.baseAngle);
			planeVariable.transform.Rotate(-staircase.currentDiff, 0, 0, Space.World);
			planeVariable.transform.Rotate (0, Random.Range (0, 360), 0, Space.Self);
			planeVariable.transform.Translate (Random.Range (0, 0.25f), 0, Random.Range (0, 0.25f), Space.Self);
			planeConstant.SetActive (false);
			planeVariable.SetActive (true);
			lastViewed = LastViewed.variable;
		} else {
			reset (planeConstant, staircase.baseAngle);
			planeConstant.transform.Rotate (0, Random.Range (0, 360), 0, Space.Self);
			planeConstant.transform.Translate (Random.Range (0, 0.25f), 0, Random.Range (0, 0.25f), Space.Self);
			planeConstant.SetActive (true);
			planeVariable.SetActive (false);
			lastViewed = LastViewed.constant;
		}
		//firstSurface = false;
		surfaceStartTime = System.DateTime.Now;
		//Debug.Log (staircase.currentDiff);
	}

	void setWait(){
		mask.SetActive (true);
		//message.text = "Please make a selection\nleft control for the first surface\t right control for the second surface";
		planeConstant.SetActive (false);
		planeVariable.SetActive (false);
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
