using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;
using StarkSDKSpace;
using UnityEngine.Analytics;

public class GameController : MonoBehaviour {

	/// <summary>
	/// Main game controller handles game states, updating scores, showing information on UI, creating new coins at the start of each round
	/// and managing gameover sequence. All the static variables used inside gamecontroller are available to use in other classes.
	/// </summary>

	public static int level;					//Current game level - Starts with 1
	public static int totalCoins = 30;			//Total coins available in each level.
	public static int remainingCoins;			//remaining coins player need to collect to finish the level
	public static int totalGatheredCoin;		//total number of collected coins before gameover
	public static bool isGameStarted;			//flag
	public static bool isGameFinished;			//flag
	public static bool isGameOver;				//flag

	//reference to UI objects
	public GameObject uiLevelNumber;
	public GameObject uiScore;
	public GameObject uiTapToStart;
	public GameObject uiRestartBtn;
	public GameObject uiFinishPlane;
	public GameObject uiYourScore;
	public GameObject uiBestScore;
	public GameObject uiNewBestScore;
	public GameObject uiLogo;

	public GameObject coin;						//coin prefab
		
	//private variables
	private Vector3 position;
	private float positionPrecision;
	private bool gameoverRunFlag;
	private bool isAdvancing;
	private int bestSavedScore;
	private GameObject adManager;               //reference to ad manager object
	public GameObject Player;
    public GameObject explosionFx;
	public GameObject ContinuePanel;
	public static GameController instance;

    public string clickid;
    private StarkAdManager starkAdManager;
    /// <summary>
    /// Init
    /// </summary>
    void Awake () {
		instance = this;
		bestSavedScore = PlayerPrefs.GetInt("BestSavedScore", 0);
		level = PlayerPrefs.GetInt("GameLevel", 1);
		//Manual override
		//level = 2;

		adManager = GameObject.FindGameObjectWithTag ("AdManager");

		totalGatheredCoin = PlayerPrefs.GetInt ("TotalGatheredCoin", 0);
		uiRestartBtn.SetActive (false);
		uiFinishPlane.SetActive (false);
		positionPrecision = totalCoins / 19.0f;
		remainingCoins = totalCoins;
		isGameStarted = false;
		isGameFinished = false;
		isGameOver = false;
		isAdvancing = false;
		gameoverRunFlag = false;
		StartCoroutine(createCoinsInOrbit ());

	}

	void Start () {

		//show current level on UI
		uiLevelNumber.GetComponent<TextMesh> ().text = level.ToString ();

		if (level == 1) {
			uiLogo.SetActive (true);
		} else {
			uiLogo.SetActive (false);
		}

	}


	/// <summary>
	/// FSM
	/// </summary>
	void Update () {

		//check for level finish state
		if (remainingCoins <= 0) {
			advanceLevel ();
		}

		//hide "tapToStart" when game is started
		if (isGameStarted && uiTapToStart && !isGameOver) {
			uiTapToStart.SetActive (false);
			uiLogo.SetActive (false);
		}

		//Monitor score and update on UI
		uiScore.GetComponent<TextMesh> ().text = totalGatheredCoin.ToString ();

		//check for game finish event
		if (isGameOver) {
			StartCoroutine (runGameover());
		}

		//debug
		//print("remainingCoins: " + remainingCoins);
	}


	/// <summary>
	/// This will be called from other controllers.
	/// We need to run it just once.
	/// </summary>
	IEnumerator runGameover() {

		if (gameoverRunFlag)
			yield break;
		gameoverRunFlag = true;

		//show current score on UI
		uiYourScore.GetComponent<TextMesh> ().text = totalGatheredCoin.ToString ();
		if (bestSavedScore < totalGatheredCoin) {
			//save new score as best score
			bestSavedScore = totalGatheredCoin;
			PlayerPrefs.SetInt("BestSavedScore", bestSavedScore);
			uiNewBestScore.SetActive (true);
		}
		//show best score on UI
		uiBestScore.GetComponent<TextMesh> ().text = bestSavedScore.ToString ();

		//show a full screen ad every now and then (default: once in each 4 gameover)
		if (Random.value > 0.75f) {
			if (adManager)
				adManager.GetComponent<AdManager> ().showInterstitial ();
		}

		yield return new WaitForSeconds (0.75f);
		uiRestartBtn.SetActive (true);
		uiFinishPlane.SetActive (true);
		uiLogo.SetActive (true);
	}


	/// <summary>
	/// Advance to higher levels with increased difficulty.
	/// </summary>
	void advanceLevel() {

		if (isAdvancing)
			return;
		isAdvancing = true;

		isGameFinished = true;
		level++;
		PlayerPrefs.SetInt ("GameLevel", level);
		PlayerPrefs.SetInt ("TotalGatheredCoin", totalGatheredCoin);
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);

	}
	public void SelectContinue()
	{
		Time.timeScale = 0;
		ContinuePanel.SetActive(true);
	}
	public void ContinueGame()
	{
        ShowVideoAd("192if3b93qo6991ed0",
            (bol) => {
                if (bol)
                {
                    Time.timeScale = 1;
                    ContinuePanel.SetActive(false);



                    clickid = "";
                    getClickid();
                    apiSend("game_addiction", clickid);
                    apiSend("lt_roi", clickid);


                }
                else
                {
                    StarkSDKSpace.AndroidUIManager.ShowToast("观看完整视频才能获取奖励哦！");
                }
            },
            (it, str) => {
                Debug.LogError("Error->" + str);
                //AndroidUIManager.ShowToast("广告加载异常，请重新看广告！");
            });
        
    }
    /// <summary>
    /// 播放插屏广告
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="errorCallBack"></param>
    /// <param name="closeCallBack"></param>
    public void ShowInterstitialAd(string adId, System.Action closeCallBack, System.Action<int, string> errorCallBack)
    {
        starkAdManager = StarkSDK.API.GetStarkAdManager();
        if (starkAdManager != null)
        {
            var mInterstitialAd = starkAdManager.CreateInterstitialAd(adId, errorCallBack, closeCallBack);
            mInterstitialAd.Load();
            mInterstitialAd.Show();
        }
    }
    public void GameOver()
	{
        ShowInterstitialAd("1lcaf5895d5l1293dc",
            () => {
                Debug.LogError("--插屏广告完成--");

            },
            (it, str) => {
                Debug.LogError("Error->" + str);
            });
        Time.timeScale = 1;
        ContinuePanel.SetActive(false);
        print("Game Over...");
        GameObject expl = Instantiate(explosionFx, Player.transform.position, Quaternion.Euler(0, 180, 0)) as GameObject;
        expl.name = "ExplosionFx";
        PlayerPrefs.SetInt("GameLevel", 1);
        PlayerPrefs.SetInt("TotalGatheredCoin", 0);
        GameController.isGameFinished = true;
        GameController.isGameOver = true;
        Player.gameObject.GetComponent<Renderer>().enabled = false;
        //Destroy(gameObject);
    }
	/// <summary>
	/// Create and position the coins on the orbit in realtime!
	/// </summary>
	IEnumerator createCoinsInOrbit() {
		for (int i = 1; i <= totalCoins; i++) {
			position = new Vector3 (Mathf.Sin( (float)i / (Mathf.PI * positionPrecision) ) * PlayerController.orbitRadius, Mathf.Cos( (float)i / (Mathf.PI * positionPrecision) ) * PlayerController.orbitRadius, 0.1f);
			GameObject c = Instantiate (coin, position, Quaternion.Euler (0, 180, 0)) as GameObject;
			c.name = "Coin-" + i.ToString ();
			yield return new WaitForSeconds (0.01f);
		}
	}
    public void getClickid()
    {
        var launchOpt = StarkSDK.API.GetLaunchOptionsSync();
        if (launchOpt.Query != null)
        {
            foreach (KeyValuePair<string, string> kv in launchOpt.Query)
                if (kv.Value != null)
                {
                    Debug.Log(kv.Key + "<-参数-> " + kv.Value);
                    if (kv.Key.ToString() == "clickid")
                    {
                        clickid = kv.Value.ToString();
                    }
                }
                else
                {
                    Debug.Log(kv.Key + "<-参数-> " + "null ");
                }
        }
    }

    public void apiSend(string eventname, string clickid)
    {
        TTRequest.InnerOptions options = new TTRequest.InnerOptions();
        options.Header["content-type"] = "application/json";
        options.Method = "POST";

        JsonData data1 = new JsonData();

        data1["event_type"] = eventname;
        data1["context"] = new JsonData();
        data1["context"]["ad"] = new JsonData();
        data1["context"]["ad"]["callback"] = clickid;

        Debug.Log("<-data1-> " + data1.ToJson());

        options.Data = data1.ToJson();

        TT.Request("https://analytics.oceanengine.com/api/v2/conversion", options,
           response => { Debug.Log(response); },
           response => { Debug.Log(response); });
    }


    /// <summary>
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="closeCallBack"></param>
    /// <param name="errorCallBack"></param>
    public void ShowVideoAd(string adId, System.Action<bool> closeCallBack, System.Action<int, string> errorCallBack)
    {
        starkAdManager = StarkSDK.API.GetStarkAdManager();
        if (starkAdManager != null)
        {
            starkAdManager.ShowVideoAdWithId(adId, closeCallBack, errorCallBack);
        }
    }


}
