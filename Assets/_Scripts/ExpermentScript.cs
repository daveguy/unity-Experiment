using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpermentScript : MonoBehaviour {

	public GameObject surface;
	public int numSamples;
	public float[] distances;
	public float[] angles;
	public float maxPermX;
	public float maxPermZ;
	public bool useLights = true;

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

	void Start () {

		//set initial values for scaling
		initScale = surface.transform.localScale;
		initZDist = surface.transform.position.z;
		initLightScale = surface.transform.Find ("lights/Spotlight").GetComponent<Light> ().range;
		initHaloScale = surface.transform.Find ("lights/Spotlight/halo").GetComponent<Light> ().range;

		isFinished = false;
		lightsOn = true;
		trialCounts = new int[useLights ? distances.Length * angles.Length * 2 : distances.Length * angles.Length];
		setNextIndex ();
		scale (surface, distances [currentDistIndex]);
		angle (surface, angles [currentAngleIndex]);
	}

	void Update () {
		if (!isFinished) {
			testInputs ();
		} else {
			surface.SetActive (false);
			//do nothingfor now, experiment over
		}
	}

	void testInputs(){
		if (Input.GetKeyDown (KeyCode.S)) {
			currentDistIndex = (currentDistIndex + 1) % distances.Length;
			scale (surface, distances [currentDistIndex]);
		} else if (Input.GetKeyDown (KeyCode.A)) {
			currentAngleIndex = (currentAngleIndex + 1) % angles.Length;
			angle (surface, angles [currentAngleIndex]);
		} else if (Input.GetKeyDown (KeyCode.L)) {
			lightsOn = lightsOn == false;
			toggleLights (surface, lightsOn);
		} else if (Input.GetKeyDown (KeyCode.Space)) {
			if (!finished ()) {
				setNextIndex ();
				scale (surface, distances [currentDistIndex]);
				angle (surface, angles [currentAngleIndex]);
			} else {
				isFinished = true;
			}
		}
	}

	void setNextIndex(){
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
}
