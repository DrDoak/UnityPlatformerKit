using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof (PhysicsSS))]
[RequireComponent (typeof (Attackable))]
public class BasicMovement : MonoBehaviour {

	// m_physics 
	public bool IsCurrentPlayer = false;
	public float JumpHeight = 4.0f;
	public float TimeToJumpApex = .4f;

	float m_accelerationTimeAirborne = .2f;
	float m_accelerationTimeGrounded = .1f;
	public float MoveSpeed = 8.0f;

	float m_gravity;
	float m_jumpVelocity;
	Vector2 velocity;
	Vector2 m_jumpVector;
	float m_velocityXSmoothing;
	//-------------------
	PhysicsSS m_physics;
	Attackable m_attackable;
	AnimatorSprite m_anim;

	public bool canDoubleJump = true;

	float inputX = 0.0f;
	float inputY = 0.0f;
	float jumpPersist = 0.0f;
	float timeSinceLastDash = 0.0f;

	public bool autonomy = true;

	bool targetSet = false;
	bool targetObj = false;
	Vector3 targetPoint;
	public float minDistance = 1.0f;
	public float abandonDistance = 10.0f;
	public PhysicsSS followObj;

	internal void Start()  {
		m_anim = GetComponent<AnimatorSprite> ();
		m_physics = GetComponent<PhysicsSS> ();
		m_attackable = GetComponent<Attackable> ();
		Reset ();
		m_gravity = -(2 * JumpHeight) / Mathf.Pow (TimeToJumpApex, 2);
		m_physics.setGravityScale (m_gravity * (1.0f/60f));
		m_jumpVelocity = Mathf.Abs(m_gravity) * TimeToJumpApex;
		m_jumpVector = new Vector2 (0f, m_jumpVelocity);
	}

	public void Reset() {
		m_attackable.energy = 0.0f;
		m_attackable.alive = true;
	}
	public void onHitConfirm(GameObject otherObj) {
	}

	void Update() {
		if (IsCurrentPlayer) {
			playerMovement ();
		} else {
			npcMovement ();
		}
	}

	internal void playerMovement() {
		if (m_physics.onGround) {canDoubleJump = true;}
		inputX = 0.0f;
		inputY = 0.0f;
		if (!autonomy && m_physics.canMove && targetSet) {
			if (targetObj) {
				if (followObj == null) {
					endTarget ();
					return;
				}
				targetPoint = followObj.transform.position;
			}
			moveToPoint (targetPoint);
		}else if (m_physics.canMove && autonomy) {
			inputY = Input.GetAxis ("Vertical");
			inputX = Input.GetAxis("Horizontal");
			if (Input.GetButtonDown("Jump")) {
				if (inputY < -0.9f) {
					GetComponent<PhysicsSS>().setDropTime(1.0f);
				}
				else if (m_physics.collisions.below) {
					m_physics.addSelfForce (m_jumpVector, 0f);
					jumpPersist = 0.2f;
				} else if (canDoubleJump) {
					velocity.y = m_jumpVelocity;
					m_physics.addSelfForce (m_jumpVector, 0f);
					canDoubleJump = false;
				}
			}
		}
		//m_physics logic
		float targetVelocityX = inputX * MoveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref m_velocityXSmoothing, (m_physics.collisions.below)?m_accelerationTimeGrounded:m_accelerationTimeAirborne);
		Vector2 input = new Vector2 (inputX, inputY);
		m_physics.Move (velocity, input);
		m_physics.AttemptingMovement = (inputX != 0.0f);
	}

	//=== NPC movement ====

	void npcMovement () {
		if (targetSet) {
			if (targetObj) {
				if (followObj == null) {
					endTarget ();
					return;
				}
				targetPoint = followObj.transform.position;
			}
			moveToPoint (targetPoint);
		}
	}

	public void moveToPoint(Vector3 point) {
		inputX = 0.0f;
		inputY = 0.0f;

		float dist = Vector3.Distance (transform.position, point);
		if (dist > abandonDistance || dist < minDistance) {
			endTarget ();
		} else {
			if (m_physics.canMove) {
				if (point.x > transform.position.x) {
					if (dist > minDistance)
						inputX = 1.0f;
					m_physics.setFacingLeft (false);

				} else {
					if (dist > minDistance)
						inputX = -1.0f;
					m_physics.setFacingLeft (true);
				}
			}
		}
		float targetVelocityX = inputX * MoveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref m_velocityXSmoothing, (m_physics.collisions.below)?m_accelerationTimeGrounded:m_accelerationTimeAirborne);
		Vector2 input = new Vector2 (inputX, inputY);

		if (m_physics.canMove && (m_physics.falling == "left" || m_physics.falling == "right") && m_physics.collisions.below) {
			m_physics.addSelfForce (new Vector2 (0f, m_jumpVelocity), 0f);
		}
		m_physics.Move (velocity, input);
		m_physics.AttemptingMovement = (inputX != 0.0f);
	}
	public void setTargetPoint(Vector3 point, float proximity) {
		setTargetPoint (point, proximity, float.MaxValue);
	}
	public void setTargetPoint(Vector3 point, float proximity,float max) {
		targetPoint = point;
		minDistance = proximity;
		abandonDistance = max;
		targetSet = true;
	}

	void setTarget(PhysicsSS target) {
		targetObj = true;
		targetSet = true;
		followObj = target;
	}
	public void endTarget() {
		targetSet = false;
		targetObj = false;
		followObj = null;
		minDistance = 0.2f;
	}
}