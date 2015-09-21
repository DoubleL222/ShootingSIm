using UnityEngine;
using System.Collections;

public class Hydra : MonoBehaviour {
	public SixenseHands	m_hand;
	public SixenseInput.Controller m_controller = null;
	
	Animator 	m_animator;
	float 		m_fLastTriggerVal;
	Vector3		m_initialPosition;
	Quaternion 	m_initialRotation;

	public GameObject BulletHole;
	// Use this for initialization
	void Start () {
		m_initialRotation = transform.localRotation;
		m_initialPosition = transform.localPosition;
	}
	
	// Update is called once per frame
	protected  void Update()
	{
		if (m_controller == null) {
			m_controller = SixenseInput.GetController (m_hand);
		} else {

		}
	}

	public Quaternion InitialRotation
	{
		get { return m_initialRotation; }
	}
	
	public Vector3 InitialPosition
	{
		get { return m_initialPosition; }
	}
}
