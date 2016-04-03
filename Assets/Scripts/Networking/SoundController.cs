using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SoundController : NetworkBehaviour
{
    public AudioClip[] clips;

    private AudioSource[] audioSources;

    // Use this for initialization
    void Start()
    {
        audioSources = new AudioSource[clips.Length];
        for (int i=0; i<clips.Length; i++)
        {
            audioSources[i] = transform.gameObject.AddComponent<AudioSource>();
            audioSources[i].spatialBlend = 1;
            audioSources[i].clip = clips[i];
        }
    }

    [ClientRpc]
    private void RpcPlayClip(string clipName)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].name.Equals(clipName))
            {
                //Debug.Log("Playing Clip: " + clipName);
                audioSources[i].Play();
                return;
            }
        }
    }

    [Command]
    public void CmdPlayClip(string clipName)
    {
        RpcPlayClip(clipName);
    }

    public void PlayClip(AudioClip clip)
    {
        CmdPlayClip(clip.name);
    }
}
