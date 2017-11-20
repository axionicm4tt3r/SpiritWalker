using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerSpirit : MonoBehaviour
{
	const float WAYPOINT_REACHED_DISTANCE = 0.1f;
	const float WAYPOINT_DISTANCE_BETWEEN_PATH_NODES = 0.2f;
	const float TIMESCALE_TIME_SLOW = 0.4f;
	const float TIMESCALE_TIME_NORMAL = 1f;
	const float TIMESCALE_TIME_CONSTANT = 2f;

	public GameObject playerBodyPrefab;

	private float maxSpeedNormal = 5;
	private float accelerationNormal = 12;
	private float maxSpeedSpirit = 12;
	private float accelerationSpirit = 20;
	private float maxSpeedRecall = 25;
	private float accelerationRecall = 40;

	private Vector2 playerSpiritVelocity;
	private Vector2 playerBodyVelocity;

	public bool SpiritMode { get; private set; }

	private LineRenderer spiritLink;

	private bool isRecallingBody;
	private GameObject playerBody;
	private Queue<Vector3> pathToFollow;
	private Vector2 lastItemAddedToPath;

	private Rigidbody2D playerSpiritRigidbody;
	private Rigidbody2D playerBodyRigidbody;

	private Animator playerAnimator;

	void Start()
	{
		pathToFollow = new Queue<Vector3>();
		playerSpiritRigidbody = GetComponent<Rigidbody2D>();
		playerAnimator = GetComponent<Animator>();
		spiritLink = GetComponent<LineRenderer>();
	}

	void Update()
	{
		ModifySpiritTag();
		HandleMovementInputs();
		HandleInteractionInputs();
		RestoreTimescale();
		BuildSpiritPath();

		//playerSpiritRigidbody.velocity = playerSpiritVelocity;
	}

	void FixedUpdate()
	{
		//HandleMovementInputs();
		//playerSpiritRigidbody.velocity = playerSpiritVelocity;

		//if (playerBodyRigidbody != null)
		//	playerBodyRigidbody.velocity = playerBodyVelocity;
	}

	void LateUpdate()
	{
		playerAnimator.SetFloat("HorizontalSpeed",playerSpiritVelocity.x);
		playerAnimator.SetFloat("VerticalSpeed", playerSpiritVelocity.y);
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
		pathToFollow.Enqueue(transform.position);
		spiritLink.positionCount = pathToFollow.Count;
		spiritLink.SetPositions(pathToFollow.ToArray());

		lastItemAddedToPath = transform.position;
	}

	private void ExitSpiritMode()
	{
		isRecallingBody = true;
		spiritLink.positionCount = pathToFollow.Count;
		spiritLink.SetPositions(pathToFollow.ToArray());
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
		else if (!(SpiritMode || isRecallingBody) && tag != "PlayerBody")
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
				spiritLink.positionCount = pathToFollow.Count;
				spiritLink.SetPositions(pathToFollow.ToArray());
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
				
				var distance = Vector3.Distance(playerBody.transform.position, nextPositionToMoveTo);

				Vector3 differenceVector = (nextPositionToMoveTo - playerBody.transform.position);
				Vector3 inputDirection = differenceVector.normalized;
				if (inputDirection.magnitude > 0)
				{
					//playerBodyVelocity = inputDirection * maxSpeedRecall;
					Vector3 velocity = inputDirection * maxSpeedRecall;
					playerBody.transform.position = Vector3.SmoothDamp(playerBody.transform.position, nextPositionToMoveTo, ref velocity, 1f);
				}

				if (distance <= WAYPOINT_REACHED_DISTANCE)
				{
					pathToFollow.Dequeue();
					spiritLink.positionCount = pathToFollow.Count;
					spiritLink.SetPositions(pathToFollow.ToArray());
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
			var currentAcceleration = SpiritMode ? accelerationSpirit : accelerationNormal;
			Vector2 inputDirection = new Vector2(horizontalInput, verticalInput).normalized;

			playerSpiritRigidbody.velocity += inputDirection * currentAcceleration * Time.deltaTime;

			if (playerSpiritRigidbody.velocity.magnitude > currentMaxSpeed)
				playerSpiritRigidbody.velocity = inputDirection.normalized * currentMaxSpeed;
		}
	}
}
