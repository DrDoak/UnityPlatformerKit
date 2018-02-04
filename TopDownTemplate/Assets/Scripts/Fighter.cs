using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (HitboxMaker))]
[RequireComponent (typeof (PhysicsTD))]

public class Fighter : MonoBehaviour {
	
	Dictionary<string,AttackInfo> attacks = new Dictionary<string,AttackInfo>();

	public string HurtAnimation = "hit";
	public string CurrentAttackName;

	string m_faction;
	PhysicsTD m_physics;
	AnimatorSprite m_anim;
	Attackable m_attackable;
	HitboxMaker m_hitboxMaker;
	AttackInfo m_currentAttack;
	bool m_hitboxCreated;
	float m_animationSpeed;
	bool m_startingNewAttack;

	[HideInInspector]
	public float recoveryTime = 0.0f;
	[HideInInspector]
	public float startUpTime = 0.0f;
	[HideInInspector]
	public float stunTime = 0.0f;
	[HideInInspector]
	public AudioClip AttackSound;
	public int hitCombo = 0;

	// Use this for initialization
	void Start () {
		init ();
	}

	protected void init() {
		m_anim = GetComponent<AnimatorSprite> ();
		m_physics = GetComponent<PhysicsTD> ();
		m_attackable = GetComponent<Attackable> ();
		m_faction = gameObject.GetComponent<Attackable> ().faction;
		m_hitboxMaker = GetComponent<HitboxMaker> ();
		endAttack ();
		AttackInfo[] at = gameObject.GetComponents<AttackInfo> ();
		foreach (AttackInfo a in at) {
			if (!attacks.ContainsKey(a.attackName))
				attacks.Add (a.attackName, a);
		}
		m_animationSpeed = 1f;
	}
	protected void update() {
		m_startingNewAttack = false;
		if (stunTime > 0.0f) {
			m_anim.Play (HurtAnimation, true);

			stunTime = Mathf.Max (0.0f, stunTime - Time.deltaTime);
			if (stunTime == 0.0f && m_attackable.alive) {
				endStun ();
			}
		} else if (!m_attackable.alive) {
			startHitState (3.0f);
		} else if (CurrentAttackName != "none") {
			m_currentAttack.timeSinceStart = m_currentAttack.timeSinceStart + Time.deltaTime;
			m_currentAttack.startUpTick ();

			if (m_hitboxCreated == false) {
				if (startUpTime <= (Time.deltaTime / 2)) {
					m_hitboxCreated = true;

					m_currentAttack.onAttack ();

					if (m_currentAttack.attackSoundFX != null) {
						AudioSource.PlayClipAtPoint (m_currentAttack.attackSoundFX, transform.position);
					}
					if (AttackSound != null) {
						AudioSource.PlayClipAtPoint (AttackSound, gameObject.transform.position);
					}
					if (m_currentAttack.attackFX) {
						addEffect (m_currentAttack.attackFX, m_currentAttack.recoveryTime + 0.2f);
					}

					if (m_currentAttack.CreateHitbox) {
						Vector2 kb = m_currentAttack.knockback;
						Vector2 realOff = m_currentAttack.HitboxOffset;
						float damage = m_currentAttack.damage;
						float stun = m_currentAttack.stun;
						m_hitboxMaker.addAttrs (m_currentAttack.hitType);
						realOff = gameObject.GetComponent<PhysicsTD> ().OrientVectorToDirection (m_currentAttack.HitboxOffset);
						m_hitboxMaker.createHitbox (m_currentAttack.HitboxScale, realOff, damage, stun, m_currentAttack.hitboxDuration, kb, true, m_faction, true);
					}
					m_anim.Play (m_currentAttack.RecoveryAnimation, true);
				} else {
					startUpTime = Mathf.Max (0.0f, startUpTime - Time.deltaTime);
				}

			} else {
				if (recoveryTime <= Time.deltaTime / 2.0f) {
					endAttack ();
				} else {
					m_currentAttack.recoveryTick ();
					recoveryTime = Mathf.Max (0.0f, recoveryTime - Time.deltaTime);
				}
			}
		} else {
			playAnimations ();
		}
	}
	internal void playAnimations() {
		if (m_physics.AttemptingMovement) {
			m_anim.Play ("walk", true);
		} else {
			m_anim.Play ("idle", true);
		}
	}
	// Update is called once per frame
	void Update () {
		update ();
	}

	void addEffect(GameObject attackFX,float lifeTime) {
		/*GameObject fx = GameObject.Instantiate (attackFX, transform);
		fx.GetComponent<disappearing> ().duration = m_currentAttack.recoveryTime;

		fx.GetComponent<disappearing> ().toDisappear = true;
		fx.GetComponent<Follow> ().followObj = gameObject;
		fx.GetComponent<Follow> ().followOffset = new Vector3 (0.0f, 0.0f, -3.0f);
		fx.GetComponent<Follow> ().toFollow = true;
		if (m_physics.facingLeft) {
			fx.transform.Rotate (new Vector3 (0f, 180f,0f));
		}

		ParticleSystem [] partsys = fx.GetComponentsInChildren<ParticleSystem> ();
		foreach (ParticleSystem p in partsys) {
			ParticleSystem.MainModule mainP = p.main;
			mainP.startLifetime = lifeTime; 
		}*/
	}
	public bool isAttacking() {
		return (CurrentAttackName == "none");
	}

	public void registerStun(float st, bool defaultStun,Hitbox hb) {
		if (defaultStun) {
			startHitState (st);
		}
		if (m_currentAttack != null) {
			m_currentAttack.onInterrupt (stunTime,defaultStun,hb);
		}
	}
	void startHitState(float st) {
		//Debug.Log ("Starting Hit State with Stun: "+ st);
		endAttack ();
		if (stunTime > 0.0f) {
			hitCombo = hitCombo + 1;
		} else {
			hitCombo = 1;
		}
		stunTime = st;
		m_physics.canMove = false;
	}

	public void registerHit(GameObject otherObj) {
		if (m_currentAttack != null) {
			m_currentAttack.onHitConfirm (otherObj);
			GetComponent<BasicMovement> ().onHitConfirm(otherObj);
		}
	}

	public void endStun() {
		if (m_attackable.alive) {
			m_physics.canMove = true;
			m_hitboxMaker.clearAttrs ();
			stunTime = 0.0f;
			hitCombo = 0;
		}
	}
	public void endAttack() {
		if (m_currentAttack != null) {
			m_currentAttack.onConclude ();
			m_currentAttack.timeSinceStart = 0.0f;
		}
		if (m_startingNewAttack)
			return;
		CurrentAttackName = "none";
		startUpTime = 0.0f;
		recoveryTime = 0.0f;
		m_anim.SetSpeed (1.0f);
		m_hitboxCreated = false;
		m_currentAttack = null;
		m_physics.canMove = true;
	}
	public bool tryAttack(string[] attackList) {
		foreach (string s in attackList) {
			if (attacks.ContainsKey (s)) {
				tryAttack (s);
				return true;
			}
		}
		return false;
	}

	public bool tryAttack(string attackName) {
		if (CurrentAttackName == "none" && attacks.ContainsKey(attackName) && stunTime <= 0.0f) {
			m_hitboxCreated = false;
			CurrentAttackName = attackName;
			m_currentAttack = attacks[CurrentAttackName];
			startUpTime = (m_currentAttack.startUpTime) - (Time.deltaTime * 2);
			recoveryTime = m_currentAttack.recoveryTime;
			m_anim.Play (m_currentAttack.StartUpAnimation,true);
			m_anim.SetSpeed(m_currentAttack.animSpeed * m_animationSpeed);
			m_physics.canMove = false;
			m_currentAttack.onStartUp ();
			m_currentAttack.timeSinceStart = 0.0f;
			m_startingNewAttack = true;
			if (m_currentAttack.startupSoundFX != null) {AudioSource.PlayClipAtPoint (m_currentAttack.startupSoundFX, transform.position);}
			return true;
		}
		return false;
	}
}
