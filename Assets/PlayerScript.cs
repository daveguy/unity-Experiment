using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class Staircase{
	public bool matte;
	public float currentDiff;
	public float baseAngle;
	public bool finished;
	public float viewAngle;

	enum LastFeedback {RIGHT, WRONG};
	private LastFeedback lastFeedback;
	private float[] reversals;
	private float[] results;
	int currentStep = 0;
	private int maxReversals = 12;
	private int numReversalsToUse = 10;
	private int reversalCount;
	private string filename;
	private float finalResult;

	//0.8 comes from Haomin's code, 2.81 is corresponding value to hit one up-one down value from "Forced-choice staircases with fixed step sizes: asymptotic and small-sample properties"
	static float upRate = 2.81f;
	static float downRate = 0.8f;

	public Staircase(bool matte, float viewAngle, float startingDiff, string filename){
		this.matte = matte;
		this.viewAngle = viewAngle;
		this.baseAngle = viewAngle - 90;
		this.currentDiff = startingDiff;
		this.filename = filename;
		this.reversals = new float[maxReversals];
		this.finished = false;
		this.reversalCount = 0;
		this.results = new float[500];
	}

	public void feedbackRight ()
	{
		results[currentStep] = currentDiff;
		currentStep++;
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
			outputResult();
		}
	}

	public void feedbackWrong ()
	{
		results[currentStep] = currentDiff;
		currentStep++;
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
			outputResult();
		}
	}

	public string makeLabel(){
		return "(" + (matte? "matte":"glossy") + "+viewAngle=" + viewAngle + ")";
	}

	public void outputResult ()
	{
		finalResult = 0;
		for (int i = 0; i < numReversalsToUse; i++)
		{
			finalResult += Mathf.Log(reversals[maxReversals - i - 1], 2);
		}
		finalResult /= numReversalsToUse;
		finalResult = Mathf.Pow(2, finalResult);
		string label = makeLabel();
		StreamWriter sw = File.AppendText("output/staircase-"+filename);
		sw.Write(label+",");
		for (int i = 0; i < currentStep; i++)
		{
			sw.Write("{0},", results[i]);
		}
		sw.Write("\n");
		sw.Close();
		sw = File.AppendText("output/result-"+filename);
		sw.WriteLine(label+",{0}", finalResult);
		sw.Close();
	}
}

public class PlayerScript : MonoBehaviour {

	public GameObject planeMatte;
	public GameObject planeGlossy;
	public GameObject focusPoint;
	public GameObject surfaceLight;
	public float viewTime;
	public float viewFocusTime;
	public GameObject mask;
	public Text message;
	public Text errorMessage;
	public float fadeDuration;

	private GameObject currentPlane;
	private Staircase[] allStaircases;
	private Staircase currentStaircase;
	enum LastViewed {CONSTANT, VARIABLE};
	private LastViewed lastViewed;
	enum Status {VIEWFOCUS, FIRSTSURFACE, SECONDSURFACE, RESPONSE, FADE};
	private Status status;
	private System.DateTime surfaceStartTime;
	private System.DateTime focusStartTime;
	private string filename;

	private bool finished = false;
	private bool matte = true;
	private bool focusTime;
	private float startingDiff = 15;

	// Use this for initialization
	//Angle input should be view angle, not angle for unity
	void Start () {
		filename = string.Format("{0}.csv", System.DateTime.Now.ToString("yyyy-MMM-dd-HH-mm-ss"));
		System.IO.Directory.CreateDirectory("output/");
		StreamWriter sw = File.AppendText("output/staircase-"+filename);
		sw.WriteLine("The following is the staircase data.");
		sw.Close();
		sw = File.AppendText("output/result-"+filename);
		sw.WriteLine("The following is the result data.");
		sw.Close();

		Renderer ren = planeGlossy.GetComponent<Renderer> ();
		ren.sharedMaterial.color = new Color(ren.sharedMaterial.color.r, ren.sharedMaterial.color.g, ren.sharedMaterial.color.b, 0f);
		ren = planeMatte.GetComponent<Renderer> ();
		ren.sharedMaterial.color = new Color(ren.sharedMaterial.color.r, ren.sharedMaterial.color.g, ren.sharedMaterial.color.b, 0f);
		allStaircases = new Staircase[1];
		allStaircases [0] = new Staircase (false, 45, startingDiff, filename);
		//allStaircases [1] = new Staircase (false, 45, startingDiff);
		changeStaircase ();
	}

	// Update is called once per frame
	void Update ()
	{
		//errorMessage.text = status.ToString() + "\n" + focusTime.ToString() + "\n" + focusPoint.activeInHierarchy;
		if (finished) {
			setFinished ();
		}
		else if (focusTime){
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
			if (Input.GetKeyDown (KeyCode.LeftArrow) ) {
				if (lastViewed == LastViewed.VARIABLE) {
					currentStaircase.feedbackRight ();
					changeStaircase ();
				}else{
					currentStaircase.feedbackWrong ();
					changeStaircase ();
				}
			}
			else if (Input.GetKeyDown (KeyCode.RightArrow)) {
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

	void changeStaircase ()
	{
		currentStaircase = getStaircase ();
		if (currentStaircase == null) {
			//we're done
			finished = true;
			return;
		} else {
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

	void set ()
	{
		focusPoint.SetActive (false);
//		currentPlane.SetActive (true);

		if (status == Status.FIRSTSURFACE) {// pick which pair to show first
			lastViewed = Random.value > 0.5 ? LastViewed.CONSTANT : LastViewed.VARIABLE;
		}
		if (lastViewed == LastViewed.CONSTANT) {
			if (currentStaircase.baseAngle - currentStaircase.currentDiff < -90) {
				currentStaircase.currentDiff = 90 + currentStaircase.baseAngle;
			}
			reset (currentPlane, currentStaircase.baseAngle);
			currentPlane.transform.Rotate (-currentStaircase.currentDiff, 0, 0, Space.World);
			currentPlane.transform.Rotate (0, Random.Range (0, 360), 0, Space.Self);
			currentPlane.transform.Translate (Random.Range (0, 0.25f), 0, Random.Range (0, 0.25f), Space.Self);
			surfaceLight.transform.eulerAngles = new Vector3((90 + (currentStaircase.baseAngle - currentStaircase.currentDiff))*2, 0, 0);
			lastViewed = LastViewed.VARIABLE;
		} else {
			reset (currentPlane, currentStaircase.baseAngle);
			currentPlane.transform.Rotate (0, Random.Range (0, 360), 0, Space.Self);
			currentPlane.transform.Translate (Random.Range (0, 0.25f), 0, Random.Range (0, 0.25f), Space.Self);
			surfaceLight.transform.eulerAngles = new Vector3((90 + currentStaircase.baseAngle)*2, 0, 0);
			lastViewed = LastViewed.CONSTANT;
		}
		surfaceStartTime = System.DateTime.Now;
	}

	void setWait(){
		//mask.SetActive (true);
		currentPlane.SetActive (false);
		message.text = "Please make a selection\nleft control for the first surface\t right control for the second surface";
	}

	void setFocus(){
		focusPoint.SetActive (true);
		//StartCoroutine(fade(0));
		currentPlane.SetActive (false);
		focusTime = true;
		focusStartTime = System.DateTime.Now;
	}

	void setFinished (){
		mask.SetActive(true);
		currentPlane.SetActive(false);
		message.text = "The Expirement is finished. Thank you for participating";
	}

	void fade (float targetAlpha)
	{
		currentPlane.GetComponent<Fade>().isFinished = false;
		currentPlane.GetComponent<Fade> ().Fade3D (targetAlpha, fadeDuration);
		while (!currentPlane.GetComponent<Fade> ().isFinished) {
//			yield return null;
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
