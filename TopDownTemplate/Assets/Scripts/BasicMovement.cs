using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof (PhysicsTD))]
[RequireComponent (typeof (Attackable))]
public class BasicMovement : MonoBehaviour {

	// m_physics 
	public bool IsCurrentPlayer = false;
	float m_accel = .1f;
	public float MoveSpeed = 8.0f;
	float m_velocityXSmoothing;
	float m_velocityYSmoothing;
	//-------------------

	PhysicsTD m_physics;
	Attackable m_attackable;
	AnimatorSprite m_anim;

	float inputX = 0.0f;
	float inputY = 0.0f;
	public bool autonomy = true;

	bool targetSet = false;
	bool targetObj = false;
	Vector3 targetPoint;
	public float minDistance = 1.0f;
	public float abandonDistance = 10.0f;
	public PhysicsTD followObj;
	Vector2 Velocity;

	internal void Start()  {
		m_anim = GetComponent<AnimatorSprite> ();
		m_physics = GetComponent<PhysicsTD> ();
		m_attackable = GetComponent<Attackable> ();
		Reset ();
		if (followObj != null) {
			setTarget (followObj);
		}
	}

	public void Reset() {
		m_attackable.alive = true;
	}

	public void onHitConfirm(GameObject otherObj) {}

	void Update() {
		if (IsCurrentPlayer) {
			playerMovement ();
		} else {
			npcMovement ();
		}
		//m_physics logic
		float targetVelocityX = inputX * MoveSpeed;
		float targetVelocityY = inputY * MoveSpeed;
		Velocity.x = Mathf.SmoothDamp (Velocity.x, targetVelocityX, ref m_velocityXSmoothing, m_accel);
		Velocity.y = Mathf.SmoothDamp (Velocity.y, targetVelocityY, ref m_velocityYSmoothing, m_accel);
		Vector2 input = new Vector2 (inputX, inputY);
		m_physics.Move (Velocity, input);
		m_physics.AttemptingMovement = (inputX != 0.0f || inputY != 0.0f);
	}


	internal void playerMovement() {
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
			if (inputX > 0.1f) {
				m_physics.setDirection (Direction.RIGHT);
			} else if (inputX < -0.1f) {
				m_physics.setDirection (Direction.LEFT);
			}
			if (inputY  > 0.1f) {
				m_physics.setDirection (Direction.UP);
			} else if (inputY < -0.1f) {
				m_physics.setDirection (Direction.DOWN);
			}
			if (Input.GetButton ("Fire1")) {
				GetComponent<Fighter> ().tryAttack ("default");
			}
		}
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
			if (m_physics.canMove && dist > minDistance) {
				if (point.x - transform.position.x  > 0.1f) {
					inputX = 1.0f;
					m_physics.setDirection (Direction.RIGHT);
				} else if (transform.position.x - point.x > 0.1f) {
					inputX = -1.0f;
					m_physics.setDirection (Direction.LEFT);
				}
				if (point.y - transform.position.y  > 0.1f) {
					inputY = 1.0f;
					m_physics.setDirection (Direction.UP);
				} else if (transform.position.y - point.y > 0.1f) {
					inputY = -1.0f;
					m_physics.setDirection (Direction.DOWN);
				}
			}
		}

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

	void setTarget(PhysicsTD target) {
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