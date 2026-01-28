using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        m_selfTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        m_selfTransform.Rotate(new Vector3(0.0f, Time.deltaTime * 30.0f, 0.0f));
    }
    private Transform m_selfTransform = null;
}
