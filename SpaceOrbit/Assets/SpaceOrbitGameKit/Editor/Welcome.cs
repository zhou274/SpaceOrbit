using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class Welcome : EditorWindow {

	bool groupEnabled;
	private GUIStyle welcomeStyle = null;
	[MenuItem("Window/SpaceOrbit")]

	public static void Initialize() {
		Welcome window = (Welcome)EditorWindow.GetWindow (typeof (Welcome), true, "Information panel");
		GUIStyle style = new GUIStyle();
		window.position = new Rect(196, 196, sizeWidth, sizeHeight);
		window.minSize = new Vector2(sizeWidth, sizeHeight);
		window.maxSize = new Vector2(sizeWidth, sizeHeight);
		window.welcomeStyle = style;
		window.Show();
	}
	
	static float sizeWidth = 630;
	static float sizeHeight = 500;
	void OnGUI() {
		if(welcomeStyle == null)
			return;

		if (GUI.Button(new Rect(10, 10, 300, 150), "View the ReadMe file"))
			Application.OpenURL(Application.dataPath + "/SpaceOrbitGameKit/Readme/");

		if (GUI.Button(new Rect(10, 300 + 30, 300, 150), "Watch the trailer on Youtuube"))
			Application.OpenURL("https://www.youtube.com/watch?v=1VxD0nrwgXI");

		if (GUI.Button(new Rect(10, 150 + 20, 300, 150), "Quickly test the Android demo (APK)"))
			Application.OpenURL("http://www.finalbossgame.com/space-orbit-unity3d/space-orbit-unity3d.apk");

		if (GUI.Button(new Rect(20 + 300, 150 + 20, 300, 150), "More"))
			Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/search/page=1/sortby=popularity/query=publisher:11082");;

		if (GUI.Button(new Rect(20 + 300, 10, 300, 150), "Kindly rate this game kit"))
			Application.OpenURL("http://u3d.as/12RL");

		if (GUI.Button(new Rect(20 + 300, 300 + 30, 300, 150), "Never prompt again")){
			DoClose();
			PlayerPrefs.SetInt("DocumentationOpened", 1);
			PlayerPrefs.Save();
		}
	}

	void DoClose() {
		Close();
	}
}

[InitializeOnLoad]
class StartupHelper {

	static StartupHelper() {
		EditorApplication.update += Startup;
	}

	static void Startup() {
		if(Time.realtimeSinceStartup < 1)
			return;

		EditorApplication.update -= Startup;
		if (!PlayerPrefs.HasKey("DocumentationOpened"))
			Welcome.Initialize();
	}
} 

