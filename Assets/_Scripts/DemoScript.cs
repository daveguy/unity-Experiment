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
			outputResult ();
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

	public GameObject lambertianSurfaceVertical;
	public GameObject lambertianSurfaceHorizontal;
	public float[] distances;
//	public float fadeDuration;

	private GameObject currentSurface;
	private int currentDistanceIndex;
	private answerRecorder recorder;
	private int numSamples = 10;
	private string filename;
	private bool waitForFade;

	//variables for scaling
	private Vector3 initScale;
	private float initHaloScale;
	private float initLightScale;
	private float initZDist;

	void Start () {
		filename = string.Format("{0}.csv", System.DateTime.Now.ToString("yyyy-MMM-dd-HH-mm-ss"));
		System.IO.Directory.CreateDirectory("output/");
		StreamWriter sw = File.AppendText("output/demo-"+filename);
		sw.WriteLine("The following is the demo results");
		sw.Close();

		deactivateAll ();
		getNextSurface();
		recorder = new answerRecorder (numSamples, filename);
		waitForFade = false;
		currentDistanceIndex = distances.Length - 1;

		//set initial values for scaling
		initScale = currentSurface.transform.localScale;
		initZDist = currentSurface.transform.position.z;
		initLightScale = currentSurface.transform.Find ("lights/Spotlight").GetComponent<Light> ().range;
		initHaloScale = currentSurface.transform.Find ("lights/Spotlight/halo").GetComponent<Light> ().range;
	}
	

	void Update ()
	{
		if (waitForFade) {
			//do nothing
		}
		else if (!recorder.isFinished) {
			if (Input.GetKeyDown (KeyCode.LeftControl)) {
				if (currentSurface.transform.Find ("vertical")) {
					recorder.recordResponses ("vertical", "horizontal");
				} else {
					recorder.recordResponses ("horizontal", "horizontal");
				}
				getNextSurface ();
			} else if (Input.GetKeyDown (KeyCode.RightControl)) {
				if (currentSurface.transform.Find ("vertical")) {
					recorder.recordResponses ("vertical", "vertical");
				} else {
					recorder.recordResponses ("horizontal", "vertical");
				}
				getNextSurface ();
			} else if (Input.GetKeyDown (KeyCode.Space)) {
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
		currentSurface.transform.position = new Vector3 (currentSurface.transform.position.x, 1-scale, distances [currentDistanceIndex]);
		currentSurface.transform.localScale = new Vector3 (initScale.x * scale, initScale.y * scale, initScale.z * scale);
		Light[] lights = currentSurface.transform.Find ("lights").GetComponentsInChildren<Light> ();
		foreach (Light l in lights) {
			if (l.name.Equals ("halo")) {
				l.range = initHaloScale * scale;
			} else {
				l.range = initLightScale * scale;
			}
		}
	}


	void getNextSurface(){
//		waitForFade = true;
//		yield return fade(0);
		currentSurface = lambertianSurfaceVertical;
		currentSurface.SetActive (true);
//		yield return fade(1);
//		waitForFade = false;
	}

//	IEnumerator fade (float targetAlpha)
//	{
//		yield return StartCoroutine(currentSurface.GetComponent<Fade> ().Fade3D (targetAlpha, fadeDuration));
//	}

	void deactivateAll(){
	
		lambertianSurfaceVertical.SetActive (false);
		lambertianSurfaceHorizontal.SetActive (false);
//		Renderer ren = g.GetComponent<Renderer> ();
//		ren.sharedMaterial.color = new Color(ren.sharedMaterial.color.r, ren.sharedMaterial.color.g, ren.sharedMaterial.color.b, 0f);

		
	}
}
