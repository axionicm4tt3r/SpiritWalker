using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour {

	public GameObject[] linkedObjects;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "PlayerBody")
		{
			Debug.Log("Click!");

			foreach (var linkedObject in linkedObjects)
			{
				linkedObject.GetComponent<SwitchableObject>().SwitchOn();
			}
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "PlayerBody")
		{
			foreach (var linkedObject in linkedObjects)
			{
				linkedObject.GetComponent<SwitchableObject>().SwitchOff();
			}
		}
	}
}
