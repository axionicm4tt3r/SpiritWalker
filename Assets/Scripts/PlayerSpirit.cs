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
	private float accelerationNormal = 25;
	private float dragNormal = 15;
	private float maxSpeedSpirit = 40;
	private float accelerationSpirit = 75;
	private float dragSpirit = 30;
	private float maxSpeedRecall = 75;
	private float accelerationRecall = 120;

	public bool SpiritMode { get; private set; }

	private LineRenderer spiritLink;

	private bool isRecallingBody;
	private GameObject playerBody;
	private Queue<Vector3> pathToFollow;
	private Vector2 lastItemAddedToPath;

	private Rigidbody2D playerSpiritRigidbody;
	private Rigidbody2D playerBodyRigidbody;

	private Vector2 playerInputs;
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
	}

	void LateUpdate()
	{
		playerAnimator.SetFloat("HorizontalSpeed",playerInputs.x);
		playerAnimator.SetFloat("VerticalSpeed", playerInputs.y);
	}

	private void HandleMovementInputs()
	{
		float verticalInput = Input.GetAxisRaw("Vertical");
		float horizontalInput = Input.GetAxisRaw("Horizontal");
		playerInputs = new Vector2(horizontalInput, verticalInput);

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

		var physicalWorld = GameObject.FindGameObjectWithTag("WorldTilemap").FindTaggedObjectInChildren("PhysicalWorld");
		physicalWorld.SetActive(false);
		var spiritWorld = GameObject.FindGameObjectWithTag("WorldTilemap").FindTaggedObjectInChildren("SpiritWorld");
		spiritWorld.SetActive(true);
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
			playerSpiritRigidbody.velocity = Vector2.zero;

			if (pathToFollow.Count > 0)
			{
				var nextPositionToMoveTo = pathToFollow.Peek();
				
				var distance = Vector3.Distance(playerBody.transform.position, nextPositionToMoveTo);

				Vector3 differenceVector = (nextPositionToMoveTo - playerBody.transform.position);
				Vector3 inputDirection = differenceVector.normalized;
				if (inputDirection.magnitude > 0)
				{
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
				CompleteSpiritRecall();
			}
		}
		else
		{
			var currentMaxSpeed = SpiritMode ? maxSpeedSpirit : maxSpeedNormal;
			var currentAcceleration = SpiritMode ? accelerationSpirit : accelerationNormal;
			var currentDrag = SpiritMode ? dragSpirit : dragNormal;

			Vector2 inputDirection = new Vector2(horizontalInput, verticalInput).normalized;

			playerSpiritRigidbody.velocity += inputDirection * currentAcceleration * Time.deltaTime;
			playerSpiritRigidbody.velocity -= playerSpiritRigidbody.velocity / currentDrag;

			if (playerSpiritRigidbody.velocity.magnitude > currentMaxSpeed)
				playerSpiritRigidbody.velocity = playerSpiritRigidbody.velocity.normalized * currentMaxSpeed;
		}
	}

	private void CompleteSpiritRecall()
	{
		Destroy(playerBody);
		playerBody = null;
		playerBodyRigidbody = null;
		isRecallingBody = false;

		var physicalWorld = GameObject.FindGameObjectWithTag("WorldTilemap").FindTaggedObjectInChildren("PhysicalWorld");
		physicalWorld.SetActive(true);
		var spiritWorld = GameObject.FindGameObjectWithTag("WorldTilemap").FindTaggedObjectInChildren("SpiritWorld");
		spiritWorld.SetActive(false);
	}
}
