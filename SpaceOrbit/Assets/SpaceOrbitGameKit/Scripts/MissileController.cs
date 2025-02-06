using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileController : MonoBehaviour {

	/// <summary>
	/// Missile controller uses a simple AI to move towards the player after instantiation.
	/// The first approach is random movement. We use a random space around player ship, select a point inside
	/// this space and move the missile towards it.
	/// The second approach will try to guess the exact position of the player on the circle (based on time and angle)
	/// You are free to use any approach you see fit.
	/// </summary>

	public static float missileSpeed = 0.1f;		//default missile speed. Increase with care!
	public GameObject explosionFx;					//explosion fx when missile hits the player.

	private Vector3 destination;	
	private GameObject target;
	private Vector3 startingPosition;
	private Vector3 moveDirection;
	private float positionPredictionX;
	private float positionPredictionY;
	


	/// <summary>
	/// Init
	/// </summary>
	void Awake () {

		target = GameObject.FindGameObjectWithTag ("Player");
		startingPosition = transform.position;

		//semi-random approach
		//positionPredictionX = Random.Range (-4.0f, 4.0f) * PlayerController.rotationSpeed;
		//positionPredictionY = Random.Range (-4.0f, 4.0f) * PlayerController.rotationSpeed;

		//precise approach
		positionPredictionX = Mathf.Sin( (PlayerController.rotationBaseTime + Random.Range(0.1f * PlayerController.dir, 1.8f * PlayerController.dir) ) * (PlayerController.rotationSpeed + (float)GameController.level/20) * Mathf.PI) * PlayerController.orbitRadius;
		positionPredictionY = Mathf.Cos( (PlayerController.rotationBaseTime + Random.Range(0.1f * PlayerController.dir, 1.8f * PlayerController.dir) ) * (PlayerController.rotationSpeed + (float)GameController.level/20) * Mathf.PI) * PlayerController.orbitRadius;;

		//get the vector between current position and player position
		moveDirection = (target.transform.position + new Vector3(positionPredictionX, positionPredictionY, 0)) - startingPosition;
		//print ("moveDirection: " + moveDirection);

	}


	/// <summary>
	/// FSM
	/// </summary>
	void Update () {

		//move the missile
		transform.Translate ( moveDirection.normalized * missileSpeed * (1 + (GameController.level / 5)), Space.World);

	}


	/// <summary>
	/// collision events
	/// </summary>
	void OnCollisionEnter(Collision c) {

		if (GameController.isGameFinished) {
			Destroy (gameObject);
			return;
		}

		//when missile hits a coin, we deactive its physics and let it move outside of view
		if (c.collider.gameObject.tag == "Coin") {
			GetComponent<Rigidbody> ().isKinematic = true;
			Destroy (gameObject, 2);
		}

		//if we hit the player, the game is over. we need to save/reset the current game parameters and start a new game
		if (c.collider.gameObject.tag == "Player") {
            Destroy(gameObject);
            GameController.instance.Player=c.collider.gameObject;
			if(GameController.instance.Player!=null)
			{
				GameController.instance.SelectContinue();
			}
			//print("Game Over...");
			//GameObject expl = Instantiate(explosionFx, c.collider.transform.position, Quaternion.Euler(0, 180, 0)) as GameObject;
			//expl.name = "ExplosionFx";
			//PlayerPrefs.SetInt("GameLevel", 1);
			//PlayerPrefs.SetInt("TotalGatheredCoin", 0);
			//GameController.isGameFinished = true;
			//GameController.isGameOver = true;
			//c.collider.gameObject.GetComponent<Renderer>().enabled = false;
			
		}
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
