using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUiBarEffect : MonoBehaviour
{
    public ParticleSystem effectParticle;
    public GameObject object1;
    float sizex;
    // Start is called before the first frame update
    void Start()
    {
        effectParticle = GetComponent<ParticleSystem>();
        sizex = object1.transform.localPosition.x; ;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void MoveBar(float size)
    {
        float x = size * sizex;
        transform.localPosition = new Vector3(x, 0, 0);
        effectParticle.Play();
    }

}
