using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickDestroy : MonoBehaviour {

	/// <summary>
	/// Quickly destroys the given object after the delay provided by the developer.
	/// </summary>

	public float destroyDelay = 1.25f;

	void Start () {
		Destroy (gameObject, destroyDelay);
	}

}
