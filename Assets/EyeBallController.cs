using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeBallController : MonoBehaviour
{
    public GameObject Focus { get => m_Focus; set => m_Focus = value; }
    private GameObject m_Focus;

    // Update is called once per frame
    private void Update()
    {
        LookAt();
    }

    private void LookAt()
    {
        Vector3 dir;
        if(m_Focus != null)
        {
            dir = (m_Focus.transform.position - this.transform.position).normalized;
            this.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }
}
