using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Utilities;

public class AmbientSoundManager : MonoBehaviour
{
    #region Audio Clips
    [Header("Audio Clips")]
    public AudioClip[] DayClipsShort;
    public AudioClip[] DayClipsLong;
    public AudioClip[] NightClipsLong;
    public AudioClip[] EveningClipsLong;
    public AudioClip[] MorningClipsLong;
    #endregion

    #region Script References
    [Header("Script References")]
    // public AzureTimeController TimeController;
    [Range(0, 24)]
    public float time = 12.0f;
    #endregion

    #region Hidden Variables
    [HideInInspector]
    public float DayTimeStart;
    [HideInInspector]
    public float DayTimeEnd;
    [HideInInspector]
    public float NightTimeStart1;
    [HideInInspector]
    public float NightTimeEnd1;
    [HideInInspector]
    public float NightTimeStart2;
    [HideInInspector]
    public float NightTimeEnd2;
    [HideInInspector]
    public float EveningTimeStart;
    [HideInInspector]
    public float EveningTimeEnd;
    [HideInInspector]
    public float MorningTimeStart;
    [HideInInspector]
    public float MorningTimeEnd;

    //Note: If the volumeChangesPerSecond value is higher than the fps, the duration of the fading will be extended!
    private int volumeChangesPerSecond = 15;
    private IEnumerator[] fader = new IEnumerator[2];
    private int ActivePlayer = 0;

    private AudioSource[] audioSourceLong;
    private AudioSource[] audioSourceShort;
    #endregion

    #region Other Stuff
    [Header("Other Stuff")]
    public bool[] isPlaying;
    public AudioMixerGroup AmbienceSoundGroup;
    public string label;
    public float fadeDuration = 1.0f;
    [Range(0.0f, 1.0f)]
    public float volume = 1.0f;
    [Tooltip("Should be a child of the player")]
    public GameObject AudioSourceParent;
    #endregion

    private void Awake()
    {
        //Generate the two AudioSources
        audioSourceLong = new AudioSource[2]{
            AudioSourceParent.AddComponent<AudioSource>(),
            AudioSourceParent.AddComponent<AudioSource>()
        };
        audioSourceShort = new AudioSource[2]{
            AudioSourceParent.AddComponent<AudioSource>(),
            AudioSourceParent.AddComponent<AudioSource>()
        };

        //Set default values
        foreach (AudioSource s in audioSourceLong)
        {
            s.outputAudioMixerGroup = AmbienceSoundGroup;
            s.loop = true;
            s.playOnAwake = false;
            s.volume = 0.0f;
        }

        //Set default values
        foreach (AudioSource s in audioSourceShort)
        {
            s.outputAudioMixerGroup = AmbienceSoundGroup;
            s.loop = false;
            s.playOnAwake = false;
            s.volume = 0.0f;
        }

        print("Successfully initialized the sound subsystem");
    }

    // private float time;
    private int i = 0;
    void Update()
    {
        if (i == 50)
        {
            // time = TimeController.GetTimeline();

            if ((time > DayTimeStart && time <= DayTimeEnd))
            {
                label = "Day";
                if (Random.Range(0, 10) == 5 && !isPlaying[0])
                {
                    Play(DayClipsLong[Random.Range(0, DayClipsLong.Length)], audioSourceLong, 0);
                    isPlaying[0] = true;
                }
                else if (Random.Range(0, 500) == 50)
                {
                    Play(DayClipsShort[Random.Range(0, DayClipsShort.Length)], audioSourceShort, 0);
                    isPlaying[0] = true;
                }
            }
            if (((time > NightTimeStart1 && time <= NightTimeEnd1) || (time > NightTimeStart2 && time <= NightTimeEnd2)))
            {
                label = "Night";
                if (Random.Range(0, 10) == 5 && !isPlaying[1])
                {
                    Play(NightClipsLong[Random.Range(0, NightClipsLong.Length)], audioSourceLong, 1);
                    isPlaying[1] = true;
                }
            }
            if ((time > EveningTimeStart && time <= EveningTimeEnd))
            {
                label = "Evening";
                if (Random.Range(0, 10) == 5 && !isPlaying[2])
                {
                    Play(EveningClipsLong[Random.Range(0, EveningClipsLong.Length)], audioSourceLong, 2);
                    isPlaying[2] = true;
                }
            }
            if ((time > MorningTimeStart && time <= MorningTimeEnd))
            {
                label = "Morning";
                if (Random.Range(0, 10) == 5 && !isPlaying[3])
                {
                    Play(MorningClipsLong[Random.Range(0, MorningClipsLong.Length)], audioSourceLong, 3);
                    isPlaying[3] = true;
                }
            }
            // print(label);
            i = 0;
        }
        i++;
    }

    /// <summary>
    /// Starts the fading of the provided AudioClip and the running clip
    /// </summary>
    /// <param name="clip">AudioClip to fade-in</param>

    public void Play(AudioClip clip, AudioSource[] _player, int e)
    {
        Debug.Log("Started playing a new audio clip");
        //Prevent fading the same clip on both players 
        if (clip == _player[ActivePlayer].clip)
        {
            return;
        }
        //Kill all playing
        foreach (IEnumerator i in fader)
        {
            if (i != null)
            {
                StopCoroutine(i);
            }
        }

        //Fade-out the active play, if it is not silent (eg: first start)
        if (_player[ActivePlayer].volume > 0)
        {
            fader[0] = FadeAudioSource(_player[ActivePlayer], fadeDuration, 0.0f, () =>
            {
                fader[0] = null;
                for (int o = 0; o < isPlaying.Length; o++)
                {
                    isPlaying[o] = false;
                }
                isPlaying[e] = true;
            }, e, false);
            StartCoroutine(fader[0]);
        }

        //Fade-in the new clip
        int NextPlayer = (ActivePlayer + 1) % _player.Length;
        _player[NextPlayer].clip = clip;
        _player[NextPlayer].Play();
        fader[1] = FadeAudioSource(_player[NextPlayer], fadeDuration, volume, () => { fader[1] = null; }, e, true);
        StartCoroutine(fader[1]);

        //Register new active player
        ActivePlayer = NextPlayer;
    }

    /// <summary>
    /// Fades an AudioSource(player) during a given amount of time(duration) to a specific volume(targetVolume)
    /// </summary>
    /// <param name="player">AudioSource to be modified</param>
    /// <param name="duration">Duration of the fading</param>
    /// <param name="targetVolume">Target volume, the player is faded to</param>
    /// <param name="finishedCallback">Called when finshed</param>
    /// <returns></returns>
    IEnumerator FadeAudioSource(AudioSource player, float duration, float targetVolume, System.Action finishedCallback, int e, bool falseOnClipDone)
    {
        //Calculate the steps
        int Steps = (int)(volumeChangesPerSecond * duration);
        float StepTime = duration / Steps;
        float StepSize = (targetVolume - player.volume) / Steps;

        //Fade now
        for (int i = 1; i < Steps; i++)
        {
            player.volume += StepSize;
            yield return Utils.GetWait(StepTime);
        }
        //Make sure the targetVolume is set
        player.volume = targetVolume;

        //Callback
        if (finishedCallback != null)
            finishedCallback();

        if (falseOnClipDone)
            StartCoroutine(WaitUntilFinished(player.clip, e));
    }

    IEnumerator WaitUntilFinished(AudioClip clip, int i)
    {
        yield return Utils.GetWait(clip.length);
        isPlaying[i] = false;
    }
}