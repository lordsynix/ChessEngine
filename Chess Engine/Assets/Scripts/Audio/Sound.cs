using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Die Klasse <c>Sound</c> dient als Skript fuer alle Audioquellen.
/// </summary>
[System.Serializable]
public class Sound
{

    public string name;
    
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    [Range(0.3f, 3f)]
    public float pitch;

    public bool loop;

    [HideInInspector] 
    public AudioSource source;

}
