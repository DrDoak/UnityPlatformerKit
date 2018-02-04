using UnityEngine;
using System.Collections.Generic;

//Class allowing basic self-propelled movement for objects in 2D plane.
public enum Direction {
	LEFT,
	UP,
	RIGHT,
	DOWN
}
[RequireComponent (typeof (BoxCollider2D))]
public class PhysicsTD : MonoBehaviour {

	public LayerMask collisionMask;

	const float skinWidth = .015f;
	int horizontalRayCount = 4;
	int verticalRayCount = 4;

	public Vector2 SelfInput = Vector2.zero;
	public Vector2 accumulatedVelocity = Vector2.zero;
	public Direction Dir;
	public bool canMove = true;
	public bool AttemptingMovement = false;
	public float DecelerationRatio = 1.0f;

	float horizontalRaySpacing;
	float verticalRaySpacing;
	public Vector2 velocity;

	BoxCollider2D bCollider;
	RaycastOrigins raycastOrigins;
	public CollisionInfo collisions;
	SpriteRenderer sprite;
	List<Vector2> CharForces = new List<Vector2>();
	List<float> timeForces = new List<float>();
	Vector2 playerForce = new Vector2(); 
	Vector2 spawnPos;
	bool resetPos = false;
	AnimatorSprite m_anim;

	float m_initialOffsetX;

	void Start() {
		bCollider = GetComponent<BoxCollider2D> ();
		float newBOffY = bCollider.offset.y + skinWidth;
		m_initialOffsetX = bCollider.offset.x;
		bCollider.offset = new Vector2(m_initialOffsetX,newBOffY);
		sprite = GetComponent<SpriteRenderer> ();
		CalculateRaySpacing ();
		canMove = true;
		setDirection (Dir);
		onSpawn ();
		m_anim = GetComponent<AnimatorSprite> ();
	}
	void onSpawn() {
		if (resetPos) {
			transform.position = new Vector3 (spawnPos.x, spawnPos.y, transform.position.z);
			accumulatedVelocity = Vector2.zero;
			resetPos = false;
		}
	}
	public void setSpawnPos(Vector2 sp) {
		resetPos = true;
		spawnPos = sp;
	}

	public void Move(Vector2 velocity) {
		//Move (velocity, Vector2.zero);
	}

	void FixedUpdate() {
		if (accumulatedVelocity.sqrMagnitude > 0.3f) {
			accumulatedVelocity *= (1.0f - Time.fixedDeltaTime * DecelerationRatio * 3.0f);
		} else {
			accumulatedVelocity = Vector2.zero;
		}
		processMovement ();
	}

	void processMovement() {
		if (!canMove) {
			SelfInput = Vector2.zero;
		}
		playerForce = playerForce * Time.fixedDeltaTime;
		velocity.x = playerForce.x;
		velocity.y = playerForce.y;

		for (int i = CharForces.Count - 1; i >= 0; i--) {
			Vector2 selfVec = CharForces [i];
			if (timeForces [i] < Time.fixedDeltaTime) {
				velocity += (selfVec * Time.fixedDeltaTime);
			} else {
				velocity += (selfVec * Time.fixedDeltaTime);
			}
			timeForces [i] = timeForces [i] - Time.fixedDeltaTime;
			if (timeForces [i] < 0f) {
				CharForces.RemoveAt (i);
				timeForces.RemoveAt (i);
			}
		}

		velocity += (accumulatedVelocity * Time.fixedDeltaTime);

		UpdateRaycastOrigins ();
		collisions.Reset ();
		////Debug.Log ("Movement Update: SelfInputX: " + SelfInput.x);
		if (velocity.x != 0 || SelfInput.x != 0) {
			HorizontalCollisions (ref velocity);
		}
		if (velocity.y != 0) {
			VerticalCollisions (ref velocity);
		}
		transform.Translate (velocity);
	}
	//Apply a quick "burst" of velocity to the player in a certain direction that 
	// will quickly decellerate
	public void addToVelocity(Vector2 veloc )
	{
		accumulatedVelocity += veloc;
	}
	//Apply a constant Velocity to the object for a certain duration
	public void addSelfForce(Vector2 force, float duration) {
		CharForces.Add (force);
		timeForces.Add (duration);
	}
	public void Move(Vector2 veloc, Vector2 input) {
		SelfInput = input;
		playerForce = veloc;
	}

	void HorizontalCollisions(ref Vector2 velocity) {
		float directionX;
		if (velocity.x == 0) {
			directionX = Mathf.Sign (SelfInput.x);
		} else {
			directionX = Mathf.Sign (velocity.x);
		}
		float rayLength = Mathf.Max(0.05f,Mathf.Abs (velocity.x) + skinWidth);

		for (int i = 0; i < horizontalRayCount; i ++) {
			Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
			if (i == horizontalRayCount - 1) {
				rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			} else {
				rayOrigin += Vector2.up * (horizontalRaySpacing/2f * i);
			}
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
			if (hit) {
				//Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength,Color.red);
			} else {
				//Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength,Color.green);
			}

			if (hit && !hit.collider.isTrigger) {
				velocity.x = (hit.distance - skinWidth) * directionX;
				rayLength = hit.distance;
			}
		}

	}

	void VerticalCollisions(ref Vector2 velocity) {
		float directionY;
		if (velocity.x == 0) {
			directionY = Mathf.Sign (SelfInput.y);
		} else {
			directionY = Mathf.Sign (velocity.y);
		}
		float rayLength = Mathf.Abs (velocity.y) + skinWidth;
		for (int i = 0; i < verticalRayCount; i ++) {
			Vector2 rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
			if (hit && !hit.collider.isTrigger && hit.collider.gameObject != gameObject) {
				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;
				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}
		}
	}

	void UpdateRaycastOrigins() {
		Bounds bounds = bCollider.bounds;

		bounds.Expand (skinWidth );
		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x , bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	void CalculateRaySpacing() {
		Bounds bounds = bCollider.bounds;
		bounds.Expand (skinWidth );

		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}

	public struct CollisionInfo {
		public bool above, below;
		public bool left, right;
		public void Reset() {
			above = below = false;
			left = right = false;
		}
	}
	public void setDirection(Direction m_dir) {
		Dir = m_dir;
		if (sprite) {
			if (Dir == Direction.LEFT) {
				sprite.flipX = true;
			} else {
				sprite.flipX = false;
			}
		}
	}
	public void TurnToTransform(Transform t) {}
}
