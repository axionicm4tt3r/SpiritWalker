using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpirit : MonoBehaviour
{
	const float WAYPOINT_REACHED_DISTANCE = 0.1f;
	const float WAYPOINT_DISTANCE_BETWEEN_PATH_NODES = 0.5f;
	const float TIMESCALE_TIME_SLOW = 0.6f;
	const float TIMESCALE_TIME_NORMAL = 1f;
	const float TIMESCALE_TIME_CONSTANT = 1f;

	public GameObject playerBodyPrefab;

	private float maxSpeedNormal = 5;
	private float maxSpeedSpirit = 8;
	private float maxSpeedRecall = 20;

	private Vector2 playerSpiritVelocity;
	private Vector2 playerBodyVelocity;

	public bool SpiritMode { get; private set; }

	private LineRenderer spiritLink;

	private bool isRecallingBody;
	private GameObject playerBody;
	private Queue<Vector2> pathToFollow;
	private Vector2 lastItemAddedToPath;

	private Rigidbody2D playerSpiritRigidbody;
	private Rigidbody2D playerBodyRigidbody;

	void Start()
	{
		pathToFollow = new Queue<Vector2>();
		playerSpiritRigidbody = GetComponent<Rigidbody2D>();
		spiritLink = GetComponent<LineRenderer>();
	}

	void Update()
	{
		ModifySpiritTag();
		HandleMovementInputs();
		HandleInteractionInputs();
		RestoreTimescale();
		BuildSpiritPath();
	}

	void FixedUpdate()
	{
		playerSpiritRigidbody.velocity = playerSpiritVelocity;

		if (playerBodyRigidbody != null)
			playerBodyRigidbody.velocity = playerBodyVelocity;
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
			SpiritMode = !SpiritMode;

			if (SpiritMode)
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
		if (!playerBody)
		{
			playerBody = GameObject.Instantiate(playerBodyPrefab, transform.position, transform.rotation) as GameObject;
			playerBodyRigidbody = playerBody.GetComponent<Rigidbody2D>();
		}

		pathToFollow.Clear();
		spiritLink.SetPositions(new Vector3[] { });
		spiritLink.SetPosition(spiritLink.positionCount, transform.position);
		pathToFollow.Enqueue(transform.position);
		
		lastItemAddedToPath = transform.position;
	}

	private void ExitSpiritMode()
	{
		isRecallingBody = true;
		//spiritLink.SetPositions(spiritLink.)
	}

	private void RestoreTimescale()
	{
		if (SpiritMode)
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

	private void ModifySpiritTag()
	{
		if ((SpiritMode || isRecallingBody) && tag != "PlayerSpirit")
		{
			tag = "PlayerSpirit";
			return;
		}
		else if (tag != "PlayerBody")
		{
			tag = "PlayerBody";
			return;
		}
	}

	private void BuildSpiritPath()
	{
		if (SpiritMode)
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
			playerSpiritVelocity = Vector2.zero;

			if (pathToFollow.Count > 0)
			{
				var nextPositionToMoveTo = pathToFollow.Peek();
				spiritLink.GetPosition(0);
				var distance = Vector3.Distance(playerBody.transform.position, nextPositionToMoveTo);

				Vector2 inputDirection = (nextPositionToMoveTo - (Vector2)playerBody.transform.position).normalized;
				//if (inputDirection.magnitude > 0)
				//{
					playerBodyVelocity = inputDirection * maxSpeedRecall;
					//Vector3 velocity = inputDirection * maxSpeedRecall;
					//playerBody.transform.position = Vector3.SmoothDamp(playerBody.transform.position, nextPositionToMoveTo, ref velocity, 1f);
				//}

				if (distance <= WAYPOINT_REACHED_DISTANCE)
				{
					pathToFollow.Dequeue();
				}
			}
			else
			{
				Destroy(playerBody);
				playerBody = null;
				playerBodyRigidbody = null;
				isRecallingBody = false;
			}
		}
		else
		{
			var currentMaxSpeed = SpiritMode ? maxSpeedSpirit : maxSpeedNormal;
			Vector2 inputDirection = new Vector2(horizontalInput, verticalInput).normalized;
			playerSpiritVelocity = inputDirection * currentMaxSpeed;
		}
	}
}
