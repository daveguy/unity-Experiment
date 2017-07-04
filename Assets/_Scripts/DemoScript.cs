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
		StreamWriter sw = File.AppendText("output/demo-"+filename);
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

//holds all my conditions and sets up the scene
public class Condition{
	public enum Conditions { NEARVV = 0, NEARVH = 1, NEARHH = 2, FARVV = 3, FARVH = 4, FARHH = 5}; //, NEARVFARH = 6, FARVNEARH = 7};  add these two later, possibly by using a distance for each surface?
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
		setNextCondition();
	}

	public float getDistance ()
	{
		return currentCondition==Conditions.NEARHH || currentCondition==Conditions.NEARVH || currentCondition==Conditions.NEARVV ? nearDist : farDist;
	}

	public GameObject getSurfaceOne ()
	{
		return surfaceVertical;
	}

	public GameObject getSurfaceTwo()
	{
		return surfaceVertical;
	}

	public void setNextCondition ()
	{
		int nextCondition;
		bool finished = false;
		while (!finished) {
			nextCondition = Random.Range (0, System.Enum.GetNames (typeof(Conditions)).Length - 1);
			if (currentCondition != (Conditions)nextCondition) {
				currentCondition = (Conditions)nextCondition;
				finished = true;
			}
		}
	}

	public string getCondition ()
	{
		return currentCondition.ToString();
	}
}

public class DemoScript : MonoBehaviour {

	public GameObject surfaceHorizontal;
	public GameObject surfaceVertical;
	public int numSamples;//this is number of samples at each distance in distances
	public Transform initCameraPos;
	public float maxPermX;
	public float maxPermZ;
	public float[] distances;

	private int[] distanceCounts;
	private GameObject currentSurface;
	private int currentDistanceIndex;
	private answerRecorder recorder;
	private string filename;
	private bool isFinished;

	private Condition condition;

	//variables for scaling
	private Vector3 initScale;
	private float initHaloScale;
	private float initLightScale;
	private float initZDist;

	void Start () {
		//wriute headers to output
		filename = string.Format("{0}.csv", System.DateTime.Now.ToString("yyyy-MMM-dd-HH-mm-ss"));
		System.IO.Directory.CreateDirectory("output/");
		StreamWriter sw = File.AppendText("output/demo-"+filename);
		sw.WriteLine("The following is the demo results");
		sw.Close();

		condition = new Condition(distances[0], distances[1], surfaceHorizontal, surfaceVertical);

		//set initial values for scaling
		initScale = surfaceVertical.transform.localScale;
		initZDist = surfaceVertical.transform.position.z;
		initLightScale = surfaceVertical.transform.Find ("lights/Spotlight").GetComponent<Light> ().range;
		initHaloScale = surfaceVertical.transform.Find ("lights/Spotlight/halo").GetComponent<Light> ().range;

		deactivateAll ();
		recorder = new answerRecorder (numSamples*distances.Length, filename);
		distanceCounts = new int[distances.Length];
		scale ();
		currentSurface = surfaceVertical;
		getNextSurface();
	}
	

	void Update ()
	{
		if (!isFinished) {
			if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				swapSurface ();
			} else if (Input.GetKeyDown (KeyCode.RightArrow)) {
				swapSurface ();
			}else if(Input.GetKeyDown(KeyCode.Space)){
				recorder.recordResponses (distances [currentDistanceIndex].ToString(), currentSurface.Equals (surfaceVertical) ? "vertical" : "horizontal");
				getNextSurface ();
				if (!finished ()) {
					scale ();
				} else {
					isFinished = true;
					recorder.outputResult ();
					deactivateAll ();
				}
			} else if (Input.GetKeyDown (KeyCode.S)) {
				if (!finished ()) {
					scale ();
				}
			}
		} else {
			//do nothing for now, should have a message to say we're done.
		}

	}

	void scale(){
		currentDistanceIndex = getNextDistance();
		float scale = distances [currentDistanceIndex] / initZDist;
		scaleSurface (surfaceVertical, scale);
		scaleSurface (surfaceHorizontal, scale);
		resetLights (surfaceVertical);
		permuteLights (surfaceVertical);
		resetLights (surfaceHorizontal);
		permuteLights (surfaceHorizontal);
	}

	void scaleSurface(GameObject surface, float scale){
		surface.transform.position = new Vector3 (surface.transform.position.x, 1-scale, distances [currentDistanceIndex]);
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

	void swapSurface(){
		currentSurface.SetActive (false);
		if (currentSurface == surfaceHorizontal) {
			currentSurface = surfaceVertical;
			initCameraPos.position = new Vector3 (0, 1, 0);
		} else {
			currentSurface = surfaceHorizontal;
			initCameraPos.position = new Vector3 (0, 1, -1000);
		}
		currentSurface.SetActive (true);
	}

	//randomly returns one of the two surfaces
	void getNextSurface(){
		currentSurface.SetActive (false);
		currentSurface =  Random.value > 0.5 ? surfaceHorizontal : surfaceVertical;
		currentSurface.SetActive (true);
	}

	//choose next distance to display and increment the appropriate counter
	int getNextDistance(){
		int nextIndex = 0;
		if (!finished ()) {
			bool found = false;
			while (!found) {
				nextIndex = Random.Range (0, distances.Length);
				if (nextIndex != currentDistanceIndex && distanceCounts [nextIndex] < numSamples) {
					found = true;
				}
			}
			distanceCounts [nextIndex]++;
		}
		return nextIndex;
	}

	bool finished(){
		bool finished = true;
		for (int i = 0; i < distanceCounts.Length; i++) {
			if (distanceCounts [i] < numSamples) {
				finished = false;
			}
		}
		return finished;
	}


	void deactivateAll(){	
		surfaceVertical.SetActive (false);
		surfaceHorizontal.SetActive (false);		
	}
}
