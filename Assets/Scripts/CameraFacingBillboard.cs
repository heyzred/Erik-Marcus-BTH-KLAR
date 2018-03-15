using UnityEngine;
using System.Collections;

public class CameraFacingBillboard : MonoBehaviour {
	
	// En update metod är en metod som är kallad varje frame.
	void Update () {
		Camera cam = Camera.main;

		transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,
			cam.transform.rotation * Vector3.up);
	}

}
