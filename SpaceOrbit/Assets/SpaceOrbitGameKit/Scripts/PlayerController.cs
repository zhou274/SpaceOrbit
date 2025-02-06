using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	/// <summary>
	/// Player controller class handles player movement, direction, speed, and it also assign the player a new random image (texture) after each gameover. 
	/// It also handles player collisions with the coins, as we process the collection of coins in this class.
	/// </summary>

	public static float orbitRadius = 2.75f;		//The radius of the orbit the player should rotate arounds.
													//if you want to change this,. you need to manualy change the scale of the 
													//"WhiteCircle" object in the scene

	public static float rotationSpeed = 0.5f;		//how fast should player rotate around the orbit
	public static int dir = 1;						//rotation direction

	public static bool canMove;
	public static bool canTap;
	public static float rotationBaseTime;

	public Material[] availablePlayerImage; 
	private int playerImageId;

	public AudioClip eatCoin;


	/// <summary>
	/// Init
	/// </summary>
	void Awake () {
		rotationBaseTime = 0;
		dir = 1;
		playerImageId = 0;
		canMove = false;
		canTap = true;
	}


	/// <summary>
	/// Start
	/// </summary>
	void Start () {

		//Optional - change player's image on each gameover
		if (GameController.level == 1) {
			//set a new image
			playerImageId = Random.Range (0, availablePlayerImage.Length);
			PlayerPrefs.SetInt ("PlayerImageId", playerImageId);
			GetComponent<Renderer> ().material = availablePlayerImage [playerImageId];
		} else {
			//if we passed the first level
			playerImageId = PlayerPrefs.GetInt("PlayerImageId", 0);
			GetComponent<Renderer> ().material = availablePlayerImage [playerImageId];
		}

	}


	/// <summary>
	/// FSM
	/// </summary>
	void Update () {

		if (GameController.isGameFinished)
			return;

		//start the game upon receiving the first input from player.
		if (Input.GetMouseButtonDown (0) && !canMove && canTap) {
			canTap = false;
			canMove = true;
			GameController.isGameStarted = true;
			StartCoroutine(reactiveTap ());
		}

		//change the move direction on each tap received from the player
		if (Input.GetMouseButtonDown (0) && canMove && canTap) {
			canTap = false;
			dir *= -1;
			StartCoroutine(reactiveTap ());
		}

		if(canMove)
			moveAroundOrbit ();
		
	}


	/// <summary>
	/// Move the player around the given orbit
	/// </summary>
	void moveAroundOrbit() {

		if (GameController.isGameFinished)
			return;

		rotationBaseTime += Time.deltaTime * dir;
		//print ("rotationBaseTime: " + rotationBaseTime);

		//set the position of the ship on the orbit based on the time, speed, current level, and orbit size!
		transform.position = new Vector3(Mathf.Sin(rotationBaseTime * (rotationSpeed + (float)GameController.level/20) * Mathf.PI) * orbitRadius,
										Mathf.Cos(rotationBaseTime * (rotationSpeed + (float)GameController.level/20) * Mathf.PI) * orbitRadius,
										transform.position.z);

		//set rotation & direction
		if(dir == 1)
			transform.rotation = Quaternion.Euler(0, 180, rotationBaseTime * (rotationSpeed + (float)GameController.level/20) * 180);
		else
			transform.rotation = Quaternion.Euler (0, 0, rotationBaseTime * (rotationSpeed + (float)GameController.level/20) * -180);
	}


	/// <summary>
	/// let the player interact again
	/// </summary>
	IEnumerator reactiveTap() {
		yield return new WaitForSeconds (0.05f);
		canTap = true;
	}


	/// <summary>
	/// Let the player collect coins
	/// </summary>
	void OnCollisionEnter(Collision c) {

		if (c.collider.gameObject.tag == "Coin") {
			GameController.remainingCoins--;				//deduct 1 from remaining coins
			GameController.totalGatheredCoin++;				//increase total number of collected coins
			playSfxOneshot (eatCoin);
			Destroy (c.collider.gameObject);
		}
	}
		

	/// <summary>
	/// Plays the given audio (oneshot version)
	/// </summary>
	void playSfxOneshot ( AudioClip _clip  ){
		GetComponent<AudioSource>().PlayOneShot(_clip);
	}
}
