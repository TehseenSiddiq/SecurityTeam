﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

public class FieldOfView : MonoBehaviour
{
	public float radius;
	[Range(0, 180)]
	public float viewAngle;

	public LayerMask targetMask;
	public LayerMask obstacleMask;
	public GameObject playerRef;

	[Range(0, 3)]
	public float transitionSpeed = 1.25f;
	public Image fieldOfViewImage;
	public Color[] colors;
	public AudioSource alertSound;

	public float energy;
	public bool isWorking = false;
	

	private float clearness;
	public bool CanSeelPlayer { get; private set; }

	void Start()
	{
		//playerRef = GameObject.FindGameObjectWithTag("Bot");
		alertSound = GetComponent<AudioSource>();
		StartCoroutine("FindTargetsWithDelay", .2f);
		InvokeRepeating("Visual", 0.1f, 1);
	}
	public void Setter(float _radius,float _viewAngle,float _energy = 8,float _clearness = 2)
    {
		radius = _radius;
		viewAngle = _viewAngle;
		clearness = _clearness;
		energy = _energy;
    }
	void Visual()
    {
		fieldOfViewImage.fillAmount = viewAngle / 180;
		fieldOfViewImage.transform.eulerAngles = new Vector3(0, 0,  transform.eulerAngles.z+ viewAngle);
		fieldOfViewImage.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, radius * 2);
		fieldOfViewImage.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, radius * 2);
    }
	IEnumerator FindTargetsWithDelay(float delay)
	{
		while (true)
		{
			yield return new WaitForSeconds(delay);
			FindVisibleTargets();
		}
	}

	void LateUpdate()
	{
        //DrawFieldOfView();
        if (CanSeelPlayer)
        {
			//fieldOfViewImage.DOFade(0, transitionSpeed / 2);
			fieldOfViewImage.DOColor(colors[0], transitionSpeed);
			alertSound.Play();
        }
        else
        {
			alertSound.Stop();
			//fieldOfViewImage.DOFade(0, transitionSpeed / 2);
			fieldOfViewImage.DOColor(colors[1], transitionSpeed);
		}
	}
	[SerializeField] Collider2D[] targetsInViewRadius;
	void FindVisibleTargets()
	{
        if (isWorking)
        {
			targetsInViewRadius = Physics2D.OverlapCircleAll(transform.position, radius, targetMask);

			if (targetsInViewRadius.Length > 0)
			{
				Transform target = targetsInViewRadius[0].transform;
				Vector2 directionToTarget = (target.position - transform.position).normalized;
				if (Vector2.Angle(transform.up, directionToTarget) < viewAngle)
				{
					float distanceToTarget = Vector2.Distance(transform.position, target.position);

					if (!Physics2D.Raycast(transform.position, target.position, distanceToTarget, obstacleMask))
					{
						CanSeelPlayer = true;
						target.GetComponent<LootController>().StressManage(clearness);
					}

					else
						CanSeelPlayer = false;
				}
				else if (CanSeelPlayer)
					CanSeelPlayer = false;
				playerRef = targetsInViewRadius[0].gameObject;
			}
			else
				CanSeelPlayer = false;
		}
		
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, radius);
	//	UnityEditor.Handles.DrawWireDisc(transform.position,Vector3.forward,radius);

		Vector2 fovLine1 = Quaternion.AngleAxis(viewAngle, transform.forward) * transform.up * radius;
		Vector2 fovLine2 = Quaternion.AngleAxis(-viewAngle, transform.forward) * transform.up * radius;
	//	Vector3 fovLine1 = DirectionFromAngle

		Gizmos.color = Color.blue;
		Gizmos.DrawRay(transform.position, fovLine1);
		Gizmos.DrawRay(transform.position, fovLine2);

		if (playerRef == null)
		{
			// Handle the case when playerRef is destroyed or null.
			return;
		}

		// Gizmos.color = Color.red;
		//Gizmos.DrawRay(transform.position, (target.position - transform.position).normalized * maxRadius);
		if (CanSeelPlayer)
        {
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, playerRef.transform.position);
        }

		Gizmos.color = Color.black;
		Gizmos.DrawRay(transform.position, transform.up * radius);
	}
}