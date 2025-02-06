using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class UserInputManager : MonoBehaviour {

	/// <summary>
	/// This class is responsible for handling all input/touch events on UI buttons and elements.
	/// </summary>

	private bool canTap = true;	
	public AudioClip tapSfx;


	/// <summary>
	/// Init.
	/// </summary>
	void Awake (){		
		Time.timeScale = 1.0f;
		Time.fixedDeltaTime = 0.02f;
	}


	/// <summary>
	/// FSM
	/// </summary>
	void Update (){
		
		//touch control
		if(canTap)
			touchManager();
		
		//debug restart
		if(Input.GetKeyDown(KeyCode.R)) {
			SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
		}
	}


	/// <summary>
	/// This function monitor player touches on UI buttons.
	/// detects both touch and clicks and can be used with editor, handheld device and 
	/// every other platforms at once.
	/// </summary>
	void touchManager (){
		
		if(Input.GetMouseButtonUp(0)) {
			
			RaycastHit hitInfo;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hitInfo)) {
				
				GameObject objectHit = hitInfo.transform.gameObject;

				//if we are interacting with other UI elements
				switch(objectHit.name) {
				case "Btn-Restart":
					SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
					break;	
				}
			}
		}
	}
		

	/// <summary>
	/// This function animates a button by modifying it's scales on x-y plane.
	/// can be used on any element to simulate the tap effect.
	/// </summary>
	IEnumerator animateButton ( GameObject _btn  ){
		canTap = false;
		Vector3 startingScale = _btn.transform.localScale;	//initial scale	
		Vector3 destinationScale = startingScale * 1.1f;	//target scale

		//Scale up
		float t = 0.0f; 
		while (t <= 1.0f) {
			t += Time.deltaTime * 9;
			_btn.transform.localScale = new Vector3(Mathf.SmoothStep(startingScale.x, destinationScale.x, t),
				_btn.transform.localScale.y,
				Mathf.SmoothStep(startingScale.z, destinationScale.z, t));
			yield return 0;
		}

		//Scale down
		float r = 0.0f; 
		if(_btn.transform.localScale.x >= destinationScale.x) {
			while (r <= 1.0f) {
				r += Time.deltaTime * 9;
				_btn.transform.localScale = new Vector3(Mathf.SmoothStep(destinationScale.x, startingScale.x, r),
					_btn.transform.localScale.y,
					Mathf.SmoothStep(destinationScale.z, startingScale.z, r));
				yield return 0;
			}
		}

		if(r >= 1)
			canTap = true;
	}


	/// <summary>
	/// Plays the given audio
	/// </summary>
	void playSfx ( AudioClip _clip  ){
		GetComponent<AudioSource>().clip = _clip;
		if(!GetComponent<AudioSource>().isPlaying) {
			GetComponent<AudioSource>().Play();
		}
	}

}