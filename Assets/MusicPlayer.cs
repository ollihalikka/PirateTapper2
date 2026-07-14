using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{

    public AudioSource music1;
    public AudioSource music2;
    int last = 0;
    // Start is called before the first frame update
    void Start()
    {
        music1.Play();
        last = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (music1.isPlaying)
        {
            return;
        }
        else if (!music1.isPlaying && !music2.isPlaying && last == 0)
        {
            music2.Play();
            last = 1;
        }
        else if (last == 1 && !music1.isPlaying && !music2.isPlaying)
        {
            music1.Play();
            last = 0;
        }
    }
}
