using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailRendererScroller : MonoBehaviour
{
    [SerializeField]
    protected Rigidbody m_Rigidbody;
    public Rigidbody Rigidbody
    {
        get { return m_Rigidbody; }
        set { m_Rigidbody = value; }
    }

    [SerializeField]
    protected TrailRenderer trailRenderer;

    [SerializeField]
    protected string textureKey = "_MainTex";

    [SerializeField]
    protected float scrollSpeedX = -5;

    [SerializeField]
    protected float tiling = 0.005f;


    protected void Reset()
    {
        m_Rigidbody = transform.parent.root.GetComponentInChildren<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float scrollSpeed = scrollSpeedX;
        float nextOffset = m_Rigidbody.velocity.magnitude * trailRenderer.material.GetTextureScale(textureKey).x;
        trailRenderer.material.SetTextureOffset(textureKey, new Vector2((-Time.time * nextOffset) % 1.0f, 0f));   
    }
}
