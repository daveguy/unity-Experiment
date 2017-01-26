using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DaveTest : MonoBehaviour {

	public GameObject cube;


	// Use this for initialization
	void Start ()
	{
		StartCoroutine(thing());
	}
	
	IEnumerator thing ()
	{
     	yield return new WaitForSecondsRealtime(1);
		yield return StartCoroutine(cube.GetComponent<Fade>().Fade3D(cube.transform, 0, 0.25f));
		yield return new WaitForSecondsRealtime(1);
		yield return StartCoroutine(cube.GetComponent<Fade>().Fade3D(cube.transform, 1, 0.25f));
		
	}
}
