using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorCollisionHelper : MonoBehaviour {

	public Door parent;
	// Use this for initialization
	void Start () {
		parent = gameObject.GetComponentInParent<Door>();
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "PlayerBody")
			parent.TriggerDoorCollision();
	}
}
