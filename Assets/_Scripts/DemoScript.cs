using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class answerRecorder{
	private string[] responses;
	private string[] distances;
	private int currentSample;
	private string filename;

	public answerRecorder(int numberOfSamples, string filename){
		responses = new string[numberOfSamples];
		distances = new string[numberOfSamples];
		currentSample = 0;
		this.filename = filename;
	}

	public void recordResponses(string distance, string response){
		responses [currentSample] = response;
		distances [currentSample] = distance;
		currentSample++;
	}

	public void outputResult ()
	{
		StreamWriter sw = File.AppendText("outputExperiment1/experiment1-"+filename);
		sw.Write("DISTANCE: ,");
		for (int i = 0; i < distances.Length; i++) {
			sw.Write (distances [i]);
			if (i < distances.Length - 1) {
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

public enum Conditions { NEARVV = 0, NEARVH = 1, NEARHH = 2, FARVV = 3, FARVH = 4, FARHH = 5, NEARVFARH = 6, FARVNEARH = 7};

//holds all my conditions and sets up the scene
public class Condition{
	public Conditions currentCondition;

	public float nearDist;
	public float farDist;
	public GameObject surfaceHorizontal;
	public GameObject surfaceVertical;

	public Condition (float nearDist, float farDist, GameObject surfaceHorizontal, GameObject surfaceVertical)
	{
		this.nearDist = nearDist;
		this.farDist = farDist;
		this.surfaceVertical = surfaceVertical;
		this.surfaceHorizontal = surfaceHorizontal;
//		setNextCondition();
	}

	public float getDistanceOne ()
	{
		return currentCondition==Conditions.NEARHH || currentCondition==Conditions.NEARVH || currentCondition==Conditions.NEARVV || currentCondition==Conditions.NEARVFARH ? nearDist : farDist;
	}

	public float getDistanceTwo ()
	{
		return currentCondition==Conditions.NEARHH || currentCondition==Conditions.NEARVH || currentCondition==Conditions.NEARVV ||currentCondition == Conditions.FARVNEARH ? nearDist : farDist;
	}

//	public GameObject getSurfaceOne ()
//	{
////		return currentCondition == Conditions.NEARVH || currentCondition == Conditions.NEARVV || currentCondition == Conditions.FARVH || currentCondition == Conditions.FARVV ? surfaceVertical : surfaceHorizontal;
//		return surfaceVertical;
//	}
//
//	public GameObject getSurfaceTwo()
//	{
////		return currentCondition == Conditions.FARHH || currentCondition == Conditions.FARVH || currentCondition == Conditions.NEARHH || currentCondition == Conditions.NEARVH ? surfaceHorizontal : surfaceVertical;
//		return surfaceHorizontal;
//	}

	public void setNextCondition ()
	{
		int nextCondition;
		bool finished = false;
		int loopCount = 0;//
		while (!finished) {
			nextCondition = Random.Range (0, System.Enum.GetNames (typeof(Conditions)).Length);
			if (currentCondition != (Conditions)nextCondition) {
				currentCondition = (Conditions)nextCondition;
				finished = true;
			}
			loopCount++;
			if (loopCount > 1000) {
				Debug.Log("infinite loop in setNextCondition");
				finished = true;
			}
		}
	}

	public string getCondition ()
	{
		return currentCondition.ToString();
	}

	public int getNumConditions ()
	{
		return System.Enum.GetNames (typeof(Conditions)).Length;
	}

}

public class DemoScript : MonoBehaviour {

	public GameObject surfaceOne;
	public GameObject surfaceTwo;
	public int numSamples;//this is number of samples at each distance in distances
	public Transform initCameraPos;
	public float maxPermX;
	public float maxPermZ;
	public float[] distances;

//	private int[] distanceCounts;
	private GameObject currentSurface;
//	private int currentDistanceIndex;
	private answerRecorder recorder;
	private string filename;
	private bool isFinished;

	private Condition condition;
	private int[] trialCounts;
	private float distanceSurfaceOne;
	private float distanceSurfaceTwo;
//	private GameObject surfaceOne;
//	private GameObject surfaceTwo;

	//variables for scaling
	private Vector3 initScale;
	private float initHaloScale;
	private float initLightScale;
	private float initZDist;

	void Start () {
		//write headers to output
		filename = string.Format("{0}.csv", System.DateTime.Now.ToString("yyyy-MMM-dd-HH-mm-ss"));
		System.IO.Directory.CreateDirectory("outputExperiment1/");
		StreamWriter sw = File.AppendText("outputExperiment1/demo-"+filename);
		sw.WriteLine("The following are the experiment results");
		sw.Close();

		condition = new Condition(distances[0], distances[1], surfaceOne, surfaceOne);

		//set initial values for scaling
		initScale = surfaceOne.transform.localScale;
		initZDist = surfaceOne.transform.position.z;
		initLightScale = surfaceOne.transform.Find ("lights/Spotlight").GetComponent<Light> ().range;
		initHaloScale = surfaceOne.transform.Find ("lights/Spotlight/halo").GetComponent<Light> ().range;

		deactivateAll ();
		trialCounts = new int[condition.getNumConditions()];
		recorder = new answerRecorder (numSamples*condition.getNumConditions() + 50, filename);
		currentSurface = surfaceOne;
		getNextCondition();
//		distanceCounts = new int[distances.Length];
		print(condition.currentCondition.ToString());
//		print((int)condition.currentCondition);
		scale (surfaceOne, distanceSurfaceOne);
		scale(surfaceTwo, distanceSurfaceTwo);
//		getNextSurface();
	}
	

	void Update ()
	{
		if (!isFinished) {
			if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				swapSurface ();
			} else if (Input.GetKeyDown (KeyCode.RightArrow)) {
				swapSurface ();
			}else if(Input.GetKeyDown(KeyCode.Space)){
				recorder.recordResponses (condition.currentCondition.ToString(), currentSurface.Equals (surfaceOne) ? "vertical" : "horizontal");
//				getNextSurface ();
				if (!finished ()) {
					getNextCondition();
					print(condition.currentCondition.ToString());
//					print((int)condition.currentCondition);
					scale (surfaceOne, distanceSurfaceOne);
					scale(surfaceTwo, distanceSurfaceTwo);
				} else {
					isFinished = true;
					recorder.outputResult ();
					deactivateAll ();
				}
			} else if (Input.GetKeyDown (KeyCode.S)) {
				if (!finished ()) {
					getNextCondition();
					scale (surfaceOne, distanceSurfaceOne);
					scale(surfaceTwo, distanceSurfaceTwo);
				}
			}
		} else {
			//do nothing for now, should have a message to say we're done.
		}

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

	void swapSurface ()
	{
		currentSurface.SetActive (false);
		if (currentSurface == surfaceOne) {//swap surface
			currentSurface = surfaceTwo;
		} else {
			currentSurface = surfaceOne;
		}
		if (currentSurface == surfaceOne) {
			if (condition.currentCondition == Conditions.FARHH || condition.currentCondition == Conditions.NEARHH) {
				initCameraPos.position = new Vector3 (0, 1, -1000);
			} else {
				initCameraPos.position = new Vector3 (0, 1, 0);
			}
		} else {
			if (condition.currentCondition == Conditions.FARVV || condition.currentCondition == Conditions.NEARVV) {
				initCameraPos.position = new Vector3 (0, 1, 0);
			} else {
				initCameraPos.position = new Vector3 (0, 1, -1000);
			}
		}
		currentSurface.SetActive (true);
	}

	//sets the next condition and loads surfaces and distances, randomly sets on of the surfaces as currentSurface
	void getNextCondition ()
	{
		currentSurface.SetActive(false);
		condition.setNextCondition ();
		bool finished = false;
		int loopCount = 0;
		while (!finished) {
			if (trialCounts [(int)condition.currentCondition] < numSamples) {
				finished = true;
			} else {
				condition.setNextCondition();
			}
			loopCount++;
			if (loopCount > 1000) {
				Debug.Log("infinite loop in getNextCondition");
				finished = true;
			}
		}
//		surfaceOne = condition.getSurfaceOne();
//		surfaceTwo = condition.getSurfaceTwo();
		distanceSurfaceOne = condition.getDistanceOne();
		distanceSurfaceTwo = condition.getDistanceTwo();
		trialCounts[(int)condition.currentCondition]++;
		print(printArray());

		currentSurface =  Random.value > 0.5 ? surfaceOne : surfaceTwo;
		if (currentSurface == surfaceOne) {
			if (condition.currentCondition == Conditions.FARHH || condition.currentCondition == Conditions.NEARHH) {
				initCameraPos.position = new Vector3 (0, 1, -1000);
			} else {
				initCameraPos.position = new Vector3 (0, 1, 0);
			}
		} else {
			if (condition.currentCondition == Conditions.FARVV || condition.currentCondition == Conditions.NEARVV) {
				initCameraPos.position = new Vector3 (0, 1, 0);
			} else {
				initCameraPos.position = new Vector3 (0, 1, -1000);
			}
		}
		currentSurface.SetActive (true);
	}

	string printArray ()
	{
		string res = "";
		foreach (int i in trialCounts) {
			res = res + " " + i;
		}
		return res;
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


	void deactivateAll(){	
		surfaceOne.SetActive (false);
		surfaceTwo.SetActive (false);		
	}
}
