using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DaveTest : MonoBehaviour {

	public GameObject cube;
	enum Stat {fading, other};
	Stat stat;

	// Use this for initialization
	void Start ()
	{
		stat = Stat.other;
	}

	void Update(){
		if (! (stat == Stat.fading)) {
			stat = Stat.fading;
			StartCoroutine(thing());
		}
	}


//	IEnumerator thing1 ()
//	{
//		cube.GetComponent<Fade>().isFinished = false;
//		StartCoroutine (thing ());
//		while (!cube.GetComponent<Fade>().isFinished) {
//			yield return null;
//		}
//	}
	
	IEnumerator thing ()
	{
		print("THING");
     	yield return new WaitForSecondsRealtime(1);
		yield return StartCoroutine(cube.GetComponent<Fade>().Fade3D(0, 0.25f));
		yield return new WaitForSecondsRealtime(1);
		yield return StartCoroutine(cube.GetComponent<Fade>().Fade3D(1, 0.25f));
		print("END THING");
		stat = Stat.other;
		
	}
}
