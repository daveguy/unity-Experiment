using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Recorder{

	public string[] trials;
	public string[] responses;

	private int currentTrial;
	private string filename;

	public Recorder(int numTrials, string filename){
		this.trials = new string[numTrials];
		this.responses = new string[numTrials];
		currentTrial = 0;
		this.filename = filename;
	}

	public void recordResult(string trial, string response){
		trials [currentTrial] = trial;
		responses [currentTrial] = response;
		currentTrial++;
	}

	public void outputResult ()
	{
		StreamWriter sw = File.AppendText("outputExperiment2/demo-"+filename);
		sw.Write("CONDITION: ,");
		for (int i = 0; i < trials.Length; i++) {
			sw.Write (trials [i]);
			if (i < trials.Length - 1) {
				sw.Write (",");
			}
		}
		sw.Write("\n");
		sw.Write ("USERRESPONSE:,");
		for (int i = 0; i < responses.Length; i++) {
			sw.Write (responses [i]);
			if (i < responses.Length - 1) {
				sw.Write (",");
			}
		}
		sw.Close();
	}
}

public class ExpermentScript : MonoBehaviour {

	public GameObject surfaceOne;
	public GameObject surfaceTwo;
	public int numSamples;
	public float[] distances;
	public float[] angles;
	public float maxPermX;
	public float maxPermZ;
	public bool useLights = true;
	public GameObject mask;
	public float maskTime;

	//variables for scaling
	private Vector3 initScale;
	private float initHaloScale;
	private float initLightScale;
	private float initZDist;

	private int currentDistIndex;
	private int currentAngleIndex;
	private int[] trialCounts;
	private int currentTrialIndex;
	private bool isFinished;
	private bool lightsOn;
	private string filename;
	private Recorder recorder;
	private bool showMask;
	private System.DateTime maskStartTime;
	private GameObject currentSurface;

	void Start () {
		//write headers to output
		filename = string.Format("{0}.csv", System.DateTime.Now.ToString("yyyy-MMM-dd-HH-mm-ss"));
		System.IO.Directory.CreateDirectory("outputExperiment2/");
		StreamWriter sw = File.AppendText("outputExperiment2/demo-"+filename);
		sw.WriteLine("The following are the experiment results. Format is distance:angle:lightsOn");
		sw.Close();

		//set initial values for scaling
		initScale = surfaceOne.transform.localScale;
		initZDist = surfaceOne.transform.position.z;
		initLightScale = surfaceOne.transform.Find ("lights/Spotlight").GetComponent<Light> ().range;
		initHaloScale = surfaceOne.transform.Find ("lights/Spotlight/halo").GetComponent<Light> ().range;

		deactivateAll();
		isFinished = false;
		lightsOn = true;
		showMask = false;
		trialCounts = new int[useLights ? distances.Length * angles.Length * 2 : distances.Length * angles.Length];
		recorder = new Recorder (trialCounts.Length * numSamples, filename);
		currentSurface = surfaceOne;
		setNextIndex (surfaceOne);
		scale (surfaceOne, distances [currentDistIndex]);
		angle (surfaceOne, angles [currentAngleIndex]);
		currentSurface.SetActive(true);
	}

	void Update () {
		if (!isFinished) {
			if (!showMask) {
				if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)){
					showMask = true;
					mask.SetActive (true);
					swapSurfaces();
					maskStartTime = System.DateTime.Now;
				}else if (Input.GetKeyDown (KeyCode.Space)) {
					recorder.recordResult (distances [currentDistIndex] + ":" + angles [currentAngleIndex] + ":" + lightsOn, "userResponse");
					showMask = true;
					mask.SetActive (true);
					if (!finished ()) {
						setNextIndex (currentSurface);
						scale (currentSurface, distances [currentDistIndex]);
						angle (currentSurface, angles [currentAngleIndex]);
						maskStartTime = System.DateTime.Now;
					} else {
						isFinished = true;
						recorder.outputResult ();
					}
			}else{
					testInputs ();
				}
			} else if ((System.DateTime.Now - maskStartTime).TotalSeconds > maskTime) {
				showMask = false;
				mask.SetActive (false);
			}
		} else {
			deactivateAll();
			//do nothingfor now, experiment over
		}
	}

	void swapSurfaces ()
	{	
		currentSurface.SetActive (false);
		if (currentSurface == surfaceOne) {
			currentSurface = surfaceTwo;
		} else {
			currentSurface = surfaceOne;
		}
		currentSurface.SetActive(true);
	}

	void testInputs(){
		if (Input.GetKeyDown (KeyCode.S)) {
			currentDistIndex = (currentDistIndex + 1) % distances.Length;
			scale (currentSurface, distances [currentDistIndex]);
		} else if (Input.GetKeyDown (KeyCode.A)) {
			currentAngleIndex = (currentAngleIndex + 1) % angles.Length;
			angle (currentSurface, angles [currentAngleIndex]);
		} else if (Input.GetKeyDown (KeyCode.L)) {
			lightsOn = lightsOn == false;
			toggleLights (currentSurface, lightsOn);
		} 
	}

	void setNextIndex(GameObject surface){
		bool finished = false;
		int nextIndex = 0;
		while (!finished) {
			nextIndex = Random.Range (0, trialCounts.Length);
			if ((nextIndex != currentTrialIndex && trialCounts[nextIndex] < numSamples) || onlyThisLeft(nextIndex)) {
				finished = true;
				currentTrialIndex = nextIndex;
				trialCounts [currentTrialIndex]++;
				int desired = nextIndex >= distances.Length * angles.Length ? nextIndex - (distances.Length * angles.Length) : nextIndex;
//				print ("current trial index: " + currentTrialIndex);
//				print ("desired: " + desired);
				if (currentTrialIndex >= distances.Length * angles.Length) {
					lightsOn = false;
				} else {
					lightsOn = true;
				}
				toggleLights (surface, lightsOn);
				if (desired > 0) {
					currentDistIndex = desired % distances.Length;
					currentAngleIndex = (int)Mathf.Floor (desired / distances.Length);
				} else {//to avoid division by 0
					currentDistIndex = desired;
					currentAngleIndex = desired;
				}
			}
		}
//		print ("Current distance index: " + currentDistIndex);
//		print ("current angle indexe: " + currentAngleIndex);
	}

	bool onlyThisLeft(int index){
		bool onlyThisLeft = true;
		for (int i = 0; i < trialCounts.Length; i++) {
			if (trialCounts [i] < numSamples  && i != index) {
				onlyThisLeft = false;
			}
		}
		return onlyThisLeft;
	}

	bool finished(){
		bool finished = true;
		for (int i = 0; i < trialCounts.Length; i++) {
			if (trialCounts [i] < numSamples) {
				finished = false;
			}
		}
		return finished;
	}

	void scale(GameObject surface, float scaleDistance){
		scaleSurface (surface, scaleDistance );
		resetLights (surface);
		permuteLights (surface);
	}

	void scaleSurface(GameObject surface, float scaleDistance){
		float scale = scaleDistance / initZDist;
		surface.transform.position = new Vector3 (surface.transform.position.x, 1-scale, scaleDistance);
		surface.transform.localScale = new Vector3 (initScale.x * scale, initScale.y * scale, initScale.z * scale);
		Light[] lights = surface.transform.Find ("lights").GetComponentsInChildren<Light> ();
		foreach (Light l in lights) {
			if (l.name.Equals ("halo")) {
				l.range = initHaloScale * scale;
			} else {
				l.range = initLightScale * scale;
			}
		}
	}

	void permuteLights(GameObject surface){
		Transform[] children = surface.GetComponentsInChildren<Transform> ();
		float xPerm;
		float zPerm;
		foreach (Transform t in children) {
			if(t.name.Equals("Spotlight")){
				xPerm = Random.value*maxPermX*2 - maxPermX;
				zPerm = Random.value*maxPermZ*2 - maxPermZ;
				t.transform.localPosition = new Vector3 (t.transform.localPosition.x + xPerm, t.transform.localPosition.y, t.transform.localPosition.z + zPerm);
			}
		}
	}

	void resetLights(GameObject surface){
		Transform[] children = surface.transform.GetComponentsInChildren<Transform> ();
		foreach (Transform t in children) {
			if (t.name.Equals ("Spotlight")) {
				Vector3 initPos = t.Find ("initPosition").localPosition;
				t.localPosition = new Vector3 (initPos.x, initPos.y, initPos.z);
			}
		}
	}

	void toggleLights(GameObject surface, bool state){
		Transform[] children = surface.transform.GetComponentsInChildren<Transform> (true);
		foreach (Transform t in children) {
			if (t.name.Equals ("halo")) {
				t.gameObject.SetActive (state);
			}
		}
	}

	void angle(GameObject surface, float angle){
		surface.transform.localEulerAngles = new Vector3 (-90 + angle, 0, 0);
	}

	void deactivateAll(){	
		surfaceOne.SetActive (false);
		surfaceTwo.SetActive (false);		
	}
}
