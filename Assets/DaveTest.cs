using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DaveTest : MonoBehaviour {

	public GameObject cube;
	bool done;

	// Use this for initialization
	void Start ()
	{
		done = false;
		StartCoroutine (thing());
	}

	void Update(){
		if (done) {
			done = false;
			StartCoroutine (thing ());
		}
	}


	IEnumerator thing1 ()
	{
		cube.GetComponent<Fade>().isFinished = false;
		StartCoroutine (thing ());
		while (!cube.GetComponent<Fade>().isFinished) {
			yield return null;
		}
	}
	
	IEnumerator thing ()
	{
     	yield return new WaitForSecondsRealtime(1);
		yield return StartCoroutine(cube.GetComponent<Fade>().Fade3D(0, 0.25f));
		yield return new WaitForSecondsRealtime(1);
		yield return StartCoroutine(cube.GetComponent<Fade>().Fade3D(1, 0.25f));
		done = true;
		
	}
}
