using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Debugger : MonoBehaviour {

	public Text debugText;
	public GameObject player;

	
	void Update ()
	{
		StringBuilder debugTextString = new StringBuilder();

		debugTextString.AppendLine("POS: \n\tX=" + player.transform.position.x + "\n\tY=" + player.transform.position.y);
		debugTextString.AppendLine("Timescale: " + Time.timeScale);


		if (debugText.text != debugTextString.ToString() && !String.IsNullOrEmpty(debugTextString.ToString()))
			debugText.text = debugTextString.ToString();
	}
}
