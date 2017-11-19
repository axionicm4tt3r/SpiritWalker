﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour {

	public GameObject[] linkedObjects;
	// Use this for initialization
	void Start () {
		
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Player")
		{
			Debug.Log("Click!");

			foreach (var linkedObject in linkedObjects)
			{
				linkedObject.GetComponent<TriggerableObject>().TriggerObject();
			}
		}
	}
}
