using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtkDash : AttackInfo {

	public Vector2 startUpDash = new Vector2 (0.0f, 0f);
	public float startUpDuration = 0.0f;
	public Vector2 attackDash = new Vector2 (0.0f, 0f);
	public float attackDashDuration = 0.0f;
	public Vector2 conclusionDash = new Vector2 (0.0f, 0f);
	public float conclusionDuration = 0.0f;

	public override void onStartUp() {
		m_physics = GetComponent<PhysicsTD> ();
		m_physics.addSelfForce (m_physics.OrientVectorToDirection (startUpDash), startUpDuration);
	}
	public override void onAttack() {
		m_physics.addSelfForce (m_physics.OrientVectorToDirection (attackDash), attackDashDuration);
	}
	public override void onConclude() {
		m_physics.addSelfForce (m_physics.OrientVectorToDirection (conclusionDash), conclusionDuration);
	}
}
