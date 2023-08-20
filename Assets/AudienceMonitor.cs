using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudienceMonitor : MonoBehaviour
{
    public List<Character> Audience
    {
        get => m_Audience;
    }
    private List<Character> m_Audience;
    [SerializeField] private float m_Radius = 10.0f;
    private SphereCollider m_Collider;

    private void OnEnable()
    {
        m_Collider = this.GetComponent<SphereCollider>();
        m_Collider.radius = m_Radius;
        m_Audience = new List<Character>();
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, m_Radius);
        foreach (Collider collider in colliders)
        {
            Character otherPersona = collider.GetComponent<Character>();
            if (otherPersona && !m_Audience.Contains(otherPersona))
                m_Audience.Add(otherPersona);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Character otherPersona = other.GetComponent<Character>();
        if(otherPersona && !m_Audience.Contains(otherPersona))
            m_Audience.Add(otherPersona);
    }

    private void OnTriggerExit(Collider other)
    {
        Character otherPersona = other.GetComponent<Character>();
        if (otherPersona && !m_Audience.Contains(otherPersona))
            m_Audience.Remove(otherPersona);
    }
}
