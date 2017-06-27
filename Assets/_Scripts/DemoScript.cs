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


	private int surfaceIndex;
	private GameObject currentSurface;
	private answerRecorder recorder;
	private int numSamples = 10;
	private string filename;

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
	

	void Update () {
		if (!recorder.isFinished) {
			if (Input.GetKeyDown (KeyCode.Space)) {
				if (currentSurface.transform.Find ("vertical")) {
					recorder.recordResponses ("vertical", "userResponse");
				} else {
					recorder.recordResponses ("horizontal", "userResponse");
				}
				getNextSurface ();
			}
		} else {
			currentSurface.SetActive (false);
			//do nothing for now, should have a message to say we're done.
		}

	}

	void getNextSurface(){
		currentSurface.SetActive (false);
		surfaceIndex = (surfaceIndex + 1) % surfaces.Length;
		currentSurface = surfaces [surfaceIndex];
		currentSurface.SetActive (true);
//		if (currentSurface.transform.Find("vertical")) {
//			print ("Text exists");
//		} else {
//			print ("Text does not exist");
//		}
	}

	void deactivateAll(){
		foreach (GameObject g in surfaces) {
			g.SetActive (false);
		}
			
	}
}
