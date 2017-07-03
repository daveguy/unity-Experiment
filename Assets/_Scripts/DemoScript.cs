using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class answerRecorder{
	public bool isFinished;

	private string[] responses;
	private string[] truths;
	private int currentSample;
	private string filename;

	public answerRecorder(int numberOfSamples, string filename){
		responses = new string[numberOfSamples];
		truths = new string[numberOfSamples];
		currentSample = 0;
		isFinished = false;
		this.filename = filename;
	}

	public void recordResponses(string truth, string response){
		responses [currentSample] = response;
		truths [currentSample] = truth;
		currentSample++;
		if (currentSample == responses.Length) {
			isFinished = true;
//			outputResult ();
		}
	}

	public void outputResult ()
	{
		StreamWriter sw = File.AppendText("output/demo-"+filename);
		sw.Write ("USERRESPONSES:,");
		for (int i = 0; i < responses.Length; i++) {
			sw.Write (responses [i]);
			if (i < responses.Length - 1) {
				sw.Write (",");
			}
		}
		sw.Write("\n");
		sw.Write("TRUTHS: ,");
		for (int i = 0; i < truths.Length; i++) {
			sw.Write (truths [i]);
			if (i < truths.Length - 1) {
				sw.Write (",");
			}
		}
		sw.Close();
	}
}

public class DemoScript : MonoBehaviour {

	public GameObject surfaceHorizontal;
	public GameObject surfaceVertical;
	public float[] distances;
	public Transform initCameraPos;
	public float maxPermX;
	public float maxPermZ;
//	public float fadeDuration;

	private GameObject currentSurface;
	private int currentDistanceIndex;
	private answerRecorder recorder;
	private int numSamples = 10;
	private string filename;

	//variables for scaling
	private Vector3 initScale;
	private float initHaloScale;
	private float initLightScale;
	private float initZDist;

	void Start () {
		filename = string.Format("{0}.csv", System.DateTime.Now.ToString("yyyy-MMM-dd-HH-mm-ss"));
//		System.IO.Directory.CreateDirectory("output/");
//		StreamWriter sw = File.AppendText("output/demo-"+filename);
//		sw.WriteLine("The following is the demo results");
//		sw.Close();

		deactivateAll ();
		resetLights (surfaceVertical);
		permuteLights (surfaceVertical);
		resetLights (surfaceHorizontal);
		permuteLights (surfaceHorizontal);
		currentSurface = surfaceVertical;
		currentSurface.SetActive (true); //change this for a method to randomly pick a surface
		recorder = new answerRecorder (numSamples, filename);
		currentDistanceIndex = 0;

		//set initial values for scaling
		initScale = currentSurface.transform.localScale;
		initZDist = currentSurface.transform.position.z;
		initLightScale = currentSurface.transform.Find ("lights/Spotlight").GetComponent<Light> ().range;
		initHaloScale = currentSurface.transform.Find ("lights/Spotlight/halo").GetComponent<Light> ().range;
	}
	

	void Update ()
	{
		if (!recorder.isFinished) {
			if (Input.GetKeyDown (KeyCode.LeftControl)) {
				if (currentSurface.transform.Find ("vertical")) {
					recorder.recordResponses ("vertical", "horizontal");
				} else {
					recorder.recordResponses ("horizontal", "horizontal");
				}
				swapSurface ();
			} else if (Input.GetKeyDown (KeyCode.RightControl)) {
				if (currentSurface.transform.Find ("vertical")) {
					recorder.recordResponses ("vertical", "vertical");
				} else {
					recorder.recordResponses ("horizontal", "vertical");
				}
				swapSurface ();
			} else if (Input.GetKeyDown (KeyCode.S)) {
				scale ();
			}
		} else {
			currentSurface.SetActive (false);
			//do nothing for now, should have a message to say we're done.
		}

	}

	void scale(){
		currentDistanceIndex = (currentDistanceIndex + 1) % distances.Length;
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
				xPerm = Random.Range (-maxPermX, maxPermX);
				zPerm = Random.Range (-maxPermZ, maxPermZ);
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

	void deactivateAll(){	
		surfaceVertical.SetActive (false);
		surfaceHorizontal.SetActive (false);		
	}
}
