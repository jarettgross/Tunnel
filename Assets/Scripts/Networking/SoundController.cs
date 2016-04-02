using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class SoundController : NetworkBehaviour
{
    public AudioClip[] clips;
    private AudioSource m_AudioSource;

    private int hasPlayed;

    [SyncVar] private int toPlay;
    [SyncVar] private int clipIndex;

    // Use this for initialization
    void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
        toPlay = 1;
        clipIndex = -1;
        hasPlayed = toPlay;
    }

    [Command]
    public void CmdPlayClip(AudioClip clip)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].Equals(clip))
            {
                clipIndex = i;
                toPlay++;
                return;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("ToPlay: " + toPlay);
        if (toPlay != hasPlayed)
        {
            AudioClip clip = clips[clipIndex];
            Debug.Log("Playing Sound: " + clip.name);
            m_AudioSource.clip = clip;
            m_AudioSource.Play();
            hasPlayed = toPlay;
        }
    }
}
