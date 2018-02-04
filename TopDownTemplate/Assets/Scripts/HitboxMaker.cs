using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxMaker : MonoBehaviour {

	public GameObject hitboxClass;
	public List<string> mAttrs;

	PhysicsTD m_physics;
	void Start () {
		m_physics = GetComponent<PhysicsTD> ();
	}
	void Update() {}
	public Hitbox createHitbox(Vector2 hitboxScale, Vector2 offset,float damage, float stun, float hitboxDuration, Vector2 knockback,bool fixedKnockback,string faction, bool followObj) {
		Vector2 cOff = m_physics.OrientVectorToDirection (offset);
		Vector3 newPos = transform.position + new Vector3(cOff.x,cOff.y,0f);
		GameObject go = Instantiate(hitboxClass,newPos,Quaternion.identity) as GameObject; 
		Hitbox newBox = go.GetComponent<Hitbox> ();
		newBox.setScale (m_physics.OrientScaleToDirection(hitboxScale));
		newBox.setDamage (damage);
		newBox.setHitboxDuration (hitboxDuration);
		newBox.setKnockback (m_physics.OrientVectorToDirection(knockback));
		newBox.setFixedKnockback (fixedKnockback);
		newBox.setFaction (faction);
		newBox.stun = stun;
		newBox.creator = gameObject;
		newBox.mAttr = mAttrs;
		if (followObj) {
			newBox.setFollow (gameObject,offset);
		}
		return newBox;
	}
	public void registerHit(GameObject otherObj) {
		if (gameObject.GetComponent<Fighter> ()) {
			gameObject.GetComponent<Fighter> ().registerHit (otherObj);
		}
	}
	public void addAttrs(string attr) {
		mAttrs.Add (attr);
	}

	public void clearAttrs() {
		mAttrs = new List<string> ();
	}
}
