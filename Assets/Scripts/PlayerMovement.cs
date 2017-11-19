using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	const float WAYPOINT_REACHED_DISTANCE = 0.05f;
	const float WAYPOINT_DISTANCE_BETWEEN_PATH_NODES = 0.5f;
	const float TIMESCALE_TIME_SLOW = 0.6f;
	const float TIMESCALE_TIME_NORMAL = 1f;
	const float TIMESCALE_TIME_CONSTANT = 1f;

	public GameObject spiritModeTransitionBodyPrefab;

	private float maxSpeedNormal = 5;
	private float maxSpeedSpirit = 8;
	private float maxSpeedRecall = 20;
	private Vector2 playerVelocity;

	private bool spiritMode;
	private bool isRecallingBody;
	private GameObject humanBody;
	private Queue<Vector2> pathToFollow;
	private Vector2 lastItemAddedToPath;


	private Rigidbody2D playerRigidbody;

	void Start()
	{
		pathToFollow = new Queue<Vector2>();
		playerRigidbody = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		HandleMovementInputs();
		HandleInteractionInputs();
		RestoreTimescale();
		BuildSpiritPath();
	}

	void FixedUpdate()
	{
		playerRigidbody.velocity = playerVelocity;
	}

	private void HandleMovementInputs()
	{
		float verticalInput = Input.GetAxisRaw("Vertical");
		float horizontalInput = Input.GetAxisRaw("Horizontal");

		CalculateMovement(verticalInput, horizontalInput);
	}

	private void HandleInteractionInputs()
	{
		if (Input.GetButtonDown("Jump"))
		{
			spiritMode = !spiritMode;

			if (spiritMode)
			{
				EnterSpiritMode();
			}
			else
			{
				ExitSpiritMode();
			}
		}
	}

	private void EnterSpiritMode()
	{
		if (!humanBody)
			humanBody = GameObject.Instantiate(spiritModeTransitionBodyPrefab, transform.position, transform.rotation) as GameObject;

		pathToFollow.Clear();
		pathToFollow.Enqueue(transform.position);
		lastItemAddedToPath = transform.position;
	}

	private void ExitSpiritMode()
	{
		var positionToSwitchTo = humanBody.transform.position;
		Destroy(humanBody);
		humanBody = null;

		transform.position = positionToSwitchTo;
		isRecallingBody = true;
	}

	private void RestoreTimescale()
	{
		if (spiritMode)
		{
			if (Time.timeScale > TIMESCALE_TIME_SLOW)
			{
				Time.timeScale -= (TIMESCALE_TIME_CONSTANT * Time.deltaTime);

				if (Time.timeScale < TIMESCALE_TIME_SLOW)
					Time.timeScale = TIMESCALE_TIME_SLOW;
			}
		}
		else
		{
			if (Time.timeScale < TIMESCALE_TIME_NORMAL)
			{
				Time.timeScale += (TIMESCALE_TIME_CONSTANT * Time.deltaTime);

				if (Time.timeScale > TIMESCALE_TIME_NORMAL)
					Time.timeScale = TIMESCALE_TIME_NORMAL;
			}
		}
	}

	private void BuildSpiritPath()
	{
		if (spiritMode)
		{
			if (Vector3.Distance(transform.position, lastItemAddedToPath) >= WAYPOINT_DISTANCE_BETWEEN_PATH_NODES)
			{
				pathToFollow.Enqueue(transform.position);
				lastItemAddedToPath = transform.position;
			}
		}
	}

	private void CalculateMovement(float verticalInput, float horizontalInput)
	{
		if (isRecallingBody)
		{
			playerVelocity = Vector2.zero;

			if (pathToFollow.Count > 0)
			{
				var nextPositionToMoveTo = pathToFollow.Peek();
				var distance = Vector3.Distance(transform.position, nextPositionToMoveTo);

				Vector2 inputDirection = (nextPositionToMoveTo - (Vector2)transform.position).normalized;
				if (inputDirection.magnitude > 0)
				{
					Vector3 velocity = inputDirection * maxSpeedRecall;
					transform.position = Vector3.SmoothDamp(transform.position, nextPositionToMoveTo, ref velocity, 1f);
				}

				if (distance <= WAYPOINT_REACHED_DISTANCE)
				{
					pathToFollow.Dequeue();
				}
			}
			else
			{
				isRecallingBody = false;
			}
		}
		else
		{
			var currentMaxSpeed = spiritMode ? maxSpeedSpirit : maxSpeedNormal;
			Vector2 inputDirection = new Vector2(horizontalInput, verticalInput).normalized;
			playerVelocity = inputDirection * currentMaxSpeed;
		}
	}
}
