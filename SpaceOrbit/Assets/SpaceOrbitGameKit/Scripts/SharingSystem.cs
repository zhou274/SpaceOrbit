﻿#pragma warning disable 414

using UnityEngine;
using System.Collections;
using System;

public class SharingSystem : MonoBehaviour {

	/// <summary>
	/// Native share system works only on Android!
	/// You need to use "OpenURL" to be able to share on other platforms.
	/// player can take a screenshot from inside the game and share it via social apps to promote your game.
	/// </summary>

	private string gameTitle = "Space Orbit!";		//the name of the game which will be shared with others on social media
	public AudioClip cameraSfx;						//screenshot sfx
	private bool canTap = true;						//flag to prevent double share
	public GameObject uiGameFinishPlane;			//we need to hide gameFinishPlane for a few seconds to be able to capture our shot
	public GameObject uiGameFinishLabel;			//...
	private GameObject player;		


	void Start() {
		player = GameObject.FindGameObjectWithTag ("Player");
	}


	/// <summary>
	/// FSM
	/// </summary>
	void Update () {
		if(canTap)
			StartCoroutine(touchManager());
	}
		

	/// <summary>
	/// Detect touch on share button
	/// </summary>
	private RaycastHit hitInfo;
	private Ray ray;
	IEnumerator touchManager () {
		
		//Mouse of touch?
		if(	Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Ended)  
			ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
		else if(Input.GetMouseButtonUp(0))
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		else
			yield break;
		
		if (Physics.Raycast(ray, out hitInfo)) {
			GameObject objectHit = hitInfo.transform.gameObject;
			switch(objectHit.name) {
				
			case "ShareButton":
				canTap = false;
				StartCoroutine (reactiveTap ());
				playSfx (cameraSfx);

				string path = Application.persistentDataPath;
				string imageName = "gameshot.png";
				string fullPath = path + "/" + imageName;

				uiGameFinishLabel.SetActive (false);
				uiGameFinishPlane.GetComponent<Renderer> ().enabled = false;

				//we want game logo and player image to be present in final screenshot, so we unhide them for a moment
				GetComponent<Renderer> ().enabled = false;
				player.GetComponent<Renderer> ().enabled = true;

				#if UNITY_ANDROID
				ScreenCapture.CaptureScreenshot (imageName);
				#endif

				#if UNITY_IOS
				Application.CaptureScreenshot (imageName);
				#endif

				#if UNITY_EDITOR
				ScreenCapture.CaptureScreenshot (fullPath);
				#endif

				yield return new WaitForSeconds(1.5f); //make sure our image has been saved.
				print ("Save Completed!!");
				print (fullPath);

				uiGameFinishLabel.SetActive (true);
				uiGameFinishPlane.GetComponent<Renderer> ().enabled = true;
				GetComponent<Renderer> ().enabled = true;
				player.GetComponent<Renderer> ().enabled = false;

				#if UNITY_ANDROID && !UNITY_EDITOR
				ShareImage(fullPath, gameTitle, gameTitle, "I'm enjoying " + gameTitle + " !!");
				#endif
				break;	
			}
		}
	}


	/// <summary>
	/// Shares the captured image with android Intents.
	/// </summary>
	public static void ShareImage(string imageFileName, string subject, string title, string message) {
		
		AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
		AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
		
		intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
		intentObject.Call<AndroidJavaObject>("setType", "image/*");
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), subject);
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TITLE"), title);
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), message);
		
		AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
		AndroidJavaObject fileObject = new AndroidJavaObject("java.io.File", imageFileName);
		AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromFile", fileObject);
		
		bool fileExist = fileObject.Call<bool>("exists");
		Debug.Log("File exist : " + fileExist);
		// Attach image to intent
		if (fileExist)
			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
		AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
		currentActivity.Call ("startActivity", intentObject);

	}


	//*****************************************************************************
	// Play sound clips
	//*****************************************************************************
	void playSfx ( AudioClip _clip  ){
		GetComponent<AudioSource>().clip = _clip;
		if(!GetComponent<AudioSource>().isPlaying) {
			GetComponent<AudioSource>().Play();
		}
	}


	/// <summary>
	/// Reactives the touch system.
	/// </summary>
	IEnumerator reactiveTap() {
		yield return new WaitForSeconds(2.0f);
		canTap = true;
	}
}
