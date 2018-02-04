using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Animator))]
[RequireComponent (typeof (SpriteDepth))]
public class AnimatorSprite : MonoBehaviour {
	Animator m_anim;
	List<string> m_states;
	string m_currentAnim = "";

	void Start () {
		m_states = new List<string> ();
		m_anim = GetComponent<Animator> ();
	}

	public void Play(string[] stateNames) {
		foreach (string s in stateNames) {
			if (Play (s)) 
				break;
		}
	}

	public bool Play(string stateName, bool autoAlign = false) {
		if (m_currentAnim == stateName || m_currentAnim == "none") {
			return true;
		}
		if (autoAlign) {
			Direction d = GetComponent<PhysicsTD> ().Dir;
			if (d == Direction.DOWN) {
				stateName += "_down";
			} else if (d == Direction.UP) {
				stateName += "_up";
			} else {
				stateName += "_side";
			}
		}
		if (m_states.Contains(stateName)) {
			m_anim.Play (stateName);
			m_currentAnim = stateName;
			return true;
		} else if (m_anim.HasState (0, Animator.StringToHash (stateName))) {
			m_anim.Play (stateName);
			m_currentAnim = stateName;
			m_states.Add (stateName);
			return true;
		}
		return false;
	}

	public void SetSpeed(float speed) {
		if (m_anim) {
			m_anim.speed = speed;
		}
	}
}
