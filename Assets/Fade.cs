using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour {

	public  IEnumerator Fade3D (Transform t, float targetAlpha, float duration)
     {
         Renderer sr = t.GetComponent<Renderer> ();
         float diffAlpha = (targetAlpha - sr.sharedMaterial.color.a);
 
         float counter = 0;
         while (counter < duration) {
             float alphaAmount = sr.sharedMaterial.color.a + (Time.deltaTime * diffAlpha) / duration;
			 sr.sharedMaterial.color = new Color (sr.sharedMaterial.color.r, sr.sharedMaterial.color.g, sr.sharedMaterial.color.b, alphaAmount);
             counter += Time.deltaTime;

             yield return null;
         }
		 sr.sharedMaterial.color = new Color (sr.sharedMaterial.color.r, sr.sharedMaterial.color.g, sr.sharedMaterial.color.b, targetAlpha);
         
     }
}
