﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attackable : MonoBehaviour {

	public float bottomOfTheWorld = -30.0f;
	public float health = 100.0f;
	public float max_health = 100.0f;
	public float energy = 0.0f;
	public float max_energy = 100.0f;
	public bool alive = true;
	public bool immortal = false;
	public string faction = "noFaction";
	public string groupID = "";
	public GameObject HitEffect;
	public GameObject HealEffect;
	public GameObject DeathEffect;
	public float EnergyRegenRate = 10.0f;
	public string mHitSound = "None";
	public float deathTime = 0.0f;
	public Color deathColor = new Color(0.0f,0.0f,0.0f);
	float currDeathTime;
	SpriteRenderer SR;

	public AudioClip Hit;


	public Dictionary<string,float> resistences = new Dictionary<string,float>();

	PhysicsSS movementController;
	// Use this for initialization
	void Start () {
		movementController = gameObject.GetComponent<PhysicsSS> ();
		health = Mathf.Min (health, max_health);
		currDeathTime = deathTime;
		SR = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		alive = transform.position.y >= bottomOfTheWorld && health > 0;
		if (!alive && !immortal) {
			if (currDeathTime > 0.0) {
				currDeathTime -= Time.deltaTime;
			} else {
				Destroy (gameObject);
			}
		}
		List<string> keys = new List<string> (resistences.Keys);

		foreach (string k in keys) {
			float time = resistences [k] - Time.deltaTime;
			resistences [k] = time;
			if (resistences [k] <= 0.0f) {
				resistences.Remove (k);
			}
		}
	}

	public void addResistence(string attribute, float time) {
		resistences [attribute] = time;
	}

	public string takeHit(Hitbox hb) {
		
		if (hb.mAttr != null) {
			foreach (string k in resistences.Keys) {
				if (hb.mAttr.Contains(k)) {
					/*if (GetComponent<Fighter> ()) {
						GetComponent<Fighter> ().registerStun( hb.stun,false,hb);
					}*/
					return "block";
				}
			}
		}
		damageObj (hb.damage);
		if (gameObject.GetComponent<PhysicsSS> ()) {
			if (hb.fixedKnockback) {
				addToVelocity (hb.knockback);
			} else {
				Vector3 otherPos = hb.gameObject.transform.position;
				float angle = Mathf.Atan2 (transform.position.y - otherPos.y, transform.position.x - otherPos.x); //*180.0f / Mathf.PI;
				float magnitude = hb.knockback.magnitude;
				float forceX = Mathf.Cos (angle) * magnitude;
				float forceY = Mathf.Sin (angle) * magnitude;
				Vector2 force = new Vector2 (forceX, forceY);
				float counterF = (gameObject.GetComponent<PhysicsSS> ().velocity.y * (1 / Time.deltaTime));
				if (counterF < 0) {
					force.y = force.y - counterF;
				}
				addToVelocity (force);
			}
		}
		/*if (hb.stun > 0 && GetComponent<Fighter> ()) {
			GetComponent<Fighter> ().registerStun( hb.stun,true,hb);
		}*/
		return "hit";
	}

	public void damageObj(float damage) {
		health = Mathf.Max(Mathf.Min(max_health, health - damage),0);
		alive = (health > 0);
	}

	public void modifyEnergy(float energyDiff) {		
		energy = Mathf.Max(Mathf.Min(max_energy, energy + energyDiff),0);
		if (energyDiff > 10) {
			GameObject.Instantiate (HealEffect, transform.position, Quaternion.identity);
		}
	}

	public void resetHealth() {
		damageObj (-1000f);
	}

	public void addToVelocity(Vector2 veloc )
	{
		if (movementController) {
			movementController.addToVelocity(veloc);
		} 
	}

	public void AddConstantVel(Vector2 veloc, float time)
	{
		if (movementController) {
			movementController.addSelfForce (veloc, time);
		}
	}
}
