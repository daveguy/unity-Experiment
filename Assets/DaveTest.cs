using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DaveTest : MonoBehaviour {

	public GameObject cube;
	private bool fade;
	bool first;

	// Use this for initialization
	void Start ()
	{
		StartCoroutine(thing1());
	}


	IEnumerator thing1 ()
	{
		print ("Start");
		cube.GetComponent<Fade>().isFinished = false;
		StartCoroutine (thing ());
		while (!cube.GetComponent<Fade>().isFinished) {
			yield return null;
		}
		print("End");
	}
	
	IEnumerator thing ()
	{
     	yield return new WaitForSecondsRealtime(1);
		yield return StartCoroutine(cube.GetComponent<Fade>().Fade3D(cube.transform, 0, 0.25f));
		yield return new WaitForSecondsRealtime(1);
		yield return StartCoroutine(cube.GetComponent<Fade>().Fade3D(cube.transform, 1, 0.25f));
		//fade = false;
		
	}
}
