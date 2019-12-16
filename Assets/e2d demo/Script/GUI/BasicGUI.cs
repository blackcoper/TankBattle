using UnityEngine;
using System.Collections;

public class BasicGUI : MonoBehaviour
{
	private const float FPS_UPDATE_INTERVAL = 0.5f;
	private float fpsAccum = 0;
	private int fpsFrames = 0;
	private float fpsTimeLeft = FPS_UPDATE_INTERVAL;
	private float fps = 0;
	private GUIStyle labelStyle;
	void Update()
	{
		fpsTimeLeft -= Time.deltaTime;
		fpsAccum += Time.timeScale / Time.deltaTime;
		fpsFrames++;

		if (fpsTimeLeft <= 0)
		{
			fps = fpsAccum / fpsFrames;
			fpsTimeLeft = FPS_UPDATE_INTERVAL;
			fpsAccum = 0;
			fpsFrames = 0;
		}
	}
	void OnGUI()
	{
		
	 	labelStyle = new GUIStyle(GUI.skin.box);
	    labelStyle.fontSize = 24;
	 	labelStyle.normal.textColor = Color.red;
		GUILayout.BeginArea(new Rect(Screen.width/2-100, 5, 200, 36));
		GUILayout.Label("FPS: " + fps.ToString("f1"),labelStyle);
		GUILayout.EndArea();
	}
}
