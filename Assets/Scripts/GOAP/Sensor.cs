using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Sensor : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 5f;
    //[SerializeField] private float timerIntervial = 1f;

    private SphereCollider detectionCollider;
    private GameObject _target;
    private Vector3 lastKnownPosition;

    public event Action OnTargetChanged = delegate { };

    public Vector3 TargetPosition => _target ? _target.transform.position : Vector3.zero;
    public bool IsTargetInRange => TargetPosition != Vector3.zero;

    private void Awake()
    {
        detectionCollider = GetComponent<SphereCollider>();
        detectionCollider.isTrigger = true;
        detectionCollider.radius = detectionRadius;
    }

    private void UpdateTargetPosition(GameObject target = null)
    {
        this._target = target;
        if(IsTargetInRange && (lastKnownPosition != TargetPosition || lastKnownPosition != Vector3.zero))
        {
            lastKnownPosition = TargetPosition;
            OnTargetChanged.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        UpdateTargetPosition(other.gameObject);
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        UpdateTargetPosition();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsTargetInRange ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
