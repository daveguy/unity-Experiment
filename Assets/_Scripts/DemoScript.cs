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

	public GameObject[] surfaces;
	public float fadeDuration;

	private int surfaceIndex;
	private GameObject currentSurface;
	private answerRecorder recorder;
	private int numSamples = 10;
	private string filename;
	private bool waitForFade;

	void Start () {
		filename = string.Format("{0}.csv", System.DateTime.Now.ToString("yyyy-MMM-dd-HH-mm-ss"));
		System.IO.Directory.CreateDirectory("output/");
		StreamWriter sw = File.AppendText("output/demo-"+filename);
		sw.WriteLine("The following is the demo results");
		sw.Close();

		deactivateAll ();
		surfaceIndex = 0;
		currentSurface = surfaces [surfaceIndex];
		currentSurface.SetActive (true);
		recorder = new answerRecorder (numSamples, filename);

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
				StartCoroutine(getNextSurface ());
			} else if (Input.GetKeyDown (KeyCode.RightControl)) {
				if (currentSurface.transform.Find ("vertical")) {
					recorder.recordResponses ("vertical", "vertical");
				} else {
					recorder.recordResponses ("horizontal", "vertical");
				}
				StartCoroutine(getNextSurface ());
			}
		} else {
			currentSurface.SetActive (false);
			//do nothing for now, should have a message to say we're done.
		}

	}


	IEnumerator getNextSurface(){
		waitForFade = true;
		yield return fade(0);
		currentSurface.SetActive (false);
		surfaceIndex = (surfaceIndex + 1) % surfaces.Length;
		currentSurface = surfaces [surfaceIndex];
		currentSurface.SetActive (true);
		yield return fade(1);
		waitForFade = false;
	}

	IEnumerator fade (float targetAlpha)
	{
		yield return StartCoroutine(currentSurface.GetComponent<Fade> ().Fade3D (targetAlpha, fadeDuration));
	}

	void deactivateAll(){
		foreach (GameObject g in surfaces) {
			Renderer ren = g.GetComponent<Renderer> ();
			ren.sharedMaterial.color = new Color(ren.sharedMaterial.color.r, ren.sharedMaterial.color.g, ren.sharedMaterial.color.b, 0f);
			g.SetActive (false);
		}
			
	}
}
