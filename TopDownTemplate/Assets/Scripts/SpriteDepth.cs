using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (SpriteRenderer))]
public class SpriteDepth : MonoBehaviour {

	public int Y_SortOffset = 0;
	SpriteRenderer m_spr;
	// Use this for initialization
	void Start () {
		m_spr = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		m_spr.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1 + Y_SortOffset;
	}
}
