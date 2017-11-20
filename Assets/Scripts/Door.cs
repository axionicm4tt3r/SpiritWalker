using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, TriggerableObject, SwitchableObject
{

	public float closeDelay = 5;
	public float doorOpenSpeedInSeconds = 0.8f;
	public bool isOpen;
	public Transform doorSprite;
	public Transform openPosition;
	public Transform closedPosition;

	private bool willClose;
	private float doorLerpPosition;

	// Update is called once per frame
	void Update()
	{
		MoveDoorToPosition();
	}

	public void TriggerDoorCollision()
	{
		Debug.Log("Collision on door gets triggered");
		OpenDoor();
		CloseDoorAfterDelay();
	}

	public void TriggerObject()
	{
		TriggerDoorCollision();
	}

	public void SwitchOn()
	{
		OpenDoor();
	}

	public void SwitchOff()
	{
		CloseDoorAfterDelay();
	}

	private void MoveDoorToPosition()
	{
		if (doorLerpPosition < doorOpenSpeedInSeconds)
			doorLerpPosition += Time.deltaTime;

		if (isOpen)
			doorSprite.position = Vector3.Lerp(openPosition.position, closedPosition.position, doorLerpPosition / doorOpenSpeedInSeconds);
		else
			doorSprite.position = Vector3.Lerp(closedPosition.position, openPosition.position, doorLerpPosition / doorOpenSpeedInSeconds);
	}

	private void CloseDoorAfterDelay()
	{
		if (!willClose)
		{
			Invoke("CloseDoor", closeDelay);
			willClose = true;
		}
	}

	private void OpenDoor()
	{
		if (!isOpen)
		{
			Debug.Log("Opening door");
			isOpen = true;
			willClose = false;
			doorLerpPosition = 0f;
		}
	}

	private void CloseDoor()
	{
		if (isOpen)
		{
			Debug.Log("Closing door");
			isOpen = false;
			doorLerpPosition = 0f;
		}
	}
}
