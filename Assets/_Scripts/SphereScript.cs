using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereScript : MonoBehaviour {

	public int numSpheres;
	public float radius;
	public float seperation;
	public GameObject centerSphere;

	private GameObject newSphere;
	private GameObject[] spheres;

	private int oldNumSpheres;
	private float oldRadius;
	private float oldSeperation;

	// Use this for initialization
	void Start () {
		spheres = new GameObject[numSpheres*numSpheres];
		instantiateSpheres ();
		oldNumSpheres = numSpheres;
		oldRadius = radius;
		oldSeperation = seperation;
	}
	
	// Update is called once per frame
	void Update () {
		if (oldNumSpheres != numSpheres || oldRadius != radius || oldSeperation != seperation) {
			foreach (GameObject o in spheres) {
				Destroy (o);
			}
			spheres = new GameObject[numSpheres * numSpheres];
			instantiateSpheres ();
			oldNumSpheres = numSpheres;
			oldRadius = radius;
			oldSeperation = seperation;
		}
	}

	void instantiateSpheres(){
		centerSphere.transform.localScale = new Vector3(2*radius, 2*radius, 2*radius);
		for (int i = -numSpheres / 2; i <= numSpheres / 2; i++) {
			for (int j = -numSpheres / 2; j <= numSpheres / 2; j++) {
				newSphere = Instantiate (centerSphere, gameObject.transform, false);
				newSphere.transform.localPosition = new Vector3 (i * seperation, 0, j * seperation);
				spheres [(i + numSpheres/2)  + numSpheres*(j + numSpheres/2)] = newSphere;
			}
		}
	}
}
