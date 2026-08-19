using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("SoundManager");
                _instance = obj.AddComponent<SoundManager>();
            }
            return _instance;
        }
    }

    private AudioSource source;
    private AudioSource uiSource;
    private AudioSource musicSource;
    private AudioClip selectClip, moveClip, diceClip, hitClip, blockClip, victoryClip, defeatClip;
    private int currentTrackIndex = 0;
    private float crossfadeDuration = 1.5f;
    private string[] gameplayTracks = { "medieval_horizons", "deuslower-fantasy-medieval-ambient-237371" };
    private string[] menuTracks = { "After_the_Last_Round", "Three_Fingers_of_Ale", "Hearthside_at_Twilight" };
    private Coroutine menuQueueCoroutine;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 0f;
        source.volume = 0.5f;

        uiSource = gameObject.AddComponent<AudioSource>();
        uiSource.spatialBlend = 0f;
        uiSource.volume = 0.7f;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.spatialBlend = 0f;
        musicSource.volume = 0.3f;
        musicSource.loop = true;

        selectClip = GenerateTone(800, 0.08f, 0.5f);
        moveClip = GenerateNoise(0.1f, 0.3f);
        diceClip = GenerateNoise(0.18f, 0.6f);
        hitClip = GenerateDescendingTone(300, 100, 0.22f, 0.5f);
        blockClip = GenerateTone(700, 0.12f, 0.4f);
        victoryClip = GenerateChord(new float[] { 440, 554, 659 }, 0.5f, 0.4f);
        defeatClip = GenerateDescendingTone(400, 200, 0.45f, 0.4f);
    }

    public void PlaySelect() { source.PlayOneShot(selectClip); }
    public void PlayMove() { source.PlayOneShot(moveClip); }
    public void PlayDice() { source.PlayOneShot(diceClip); }
    public void PlayHit() { source.PlayOneShot(hitClip); }
    public void PlayBlock() { source.PlayOneShot(blockClip); }
    public void PlayVictory() { source.PlayOneShot(victoryClip); StartCoroutine(PlayFanfareDelayed(0.2f)); }
    public void PlayDefeat() { source.PlayOneShot(defeatClip); source.PlayOneShot(GenerateDescendingTone(200, 50, 0.6f, 0.3f)); }
    public void PlayCoin() { uiSource.PlayOneShot(GenerateTone(1800, 0.12f, 0.5f)); uiSource.PlayOneShot(GenerateTone(2400, 0.08f, 0.3f)); }
    public void PlayButton() { uiSource.PlayOneShot(GenerateTone(660, 0.1f, 0.7f)); uiSource.PlayOneShot(GenerateTone(880, 0.08f, 0.5f)); }

    public void PlayHammer() { source.PlayOneShot(GenerateDescendingTone(600, 80, 0.35f, 0.7f)); source.PlayOneShot(GenerateNoise(0.08f, 0.5f)); }
    public void PlayCupShake() { source.PlayOneShot(GenerateNoise(0.12f, 0.4f)); StartCoroutine(PlayCupRattle()); }
    IEnumerator PlayCupRattle()
    {
        for (int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(Random.Range(0.12f, 0.22f));
            source.PlayOneShot(GenerateTone(Random.Range(400, 800), 0.03f, 0.15f));
        }
    }
    public void PlayCupPour() { source.PlayOneShot(GenerateDescendingTone(500, 200, 0.18f, 0.35f)); source.PlayOneShot(GenerateNoise(0.06f, 0.3f)); }

    public void PlaySelectSound(string species)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Sounds/{species}/yes");
        if (clip == null)
            clip = Resources.Load<AudioClip>($"Sprites/{species}/Sound/yes");
        if (clip == null) return;
        source.Stop();
        source.clip = clip;
        source.time = species == "Human" ? 1f : 0f;
        source.Play();
        if (species != "Human")
            StartCoroutine(StopAfterDelay(1.25f));
    }

    public void PlayDiceRoll()
    {
        source.Stop();
        source.volume = 1f;
        StartCoroutine(PlayDiceSequence());
    }

    IEnumerator PlayDiceSequence()
    {
        AudioClip clip1 = Resources.Load<AudioClip>("Sounds/Efectos/dice1");
        AudioClip clip2 = Resources.Load<AudioClip>("Sounds/Efectos/dice2");

        if (clip1 != null)
        {
            source.clip = clip1;
            source.time = 0f;
            source.Play();
            yield return new WaitForSeconds(clip1.length);
        }

        if (clip2 != null)
        {
            source.clip = clip2;
            source.time = 0f;
            source.Play();
            yield return new WaitForSeconds(clip2.length);
        }

        source.volume = 0.5f;
    }

    private IEnumerator PlayFanfareDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        source.PlayOneShot(GenerateChord(new float[] { 523, 659, 784 }, 0.6f, 0.35f));
        yield return new WaitForSeconds(0.25f);
        source.PlayOneShot(GenerateChord(new float[] { 659, 784, 1047 }, 0.8f, 0.4f));
    }

    public void PlaySwordClash()
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Efectos/Choque de espadas");
        if (clip == null) return;
        source.Stop();
        source.clip = clip;
        source.time = 1f;
        source.Play();
    }

    public void PlayRockBreak()
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Efectos/rock-break");
        if (clip == null) return;
        source.Stop();
        source.clip = clip;
        source.time = clip.length > 6f ? 6f : 0f;
        source.Play();
        StartCoroutine(StopAfterDelay(clip.length > 6f ? 4f : clip.length));
    }

    public void PlayOrcAppear()
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Efectos/orcs");
        if (clip == null) return;
        source.Stop();
        source.clip = clip;
        source.time = clip.length > 6f ? 6f : 0f;
        source.Play();
        StartCoroutine(StopAfterDelay(clip.length > 6f ? 4f : clip.length));
    }

    public void PlayLightning()
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Efectos/rayos");
        if (clip == null) return;
        source.PlayOneShot(clip);
    }

    public void PlayFireball()
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Efectos/fire");
        if (clip == null) return;
        source.PlayOneShot(clip);
        StartCoroutine(StopAfterDelay(1f));
    }

    public void PlayFireRayo()
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Efectos/fire-rayo");
        if (clip == null) return;
        source.Stop();
        source.clip = clip;
        source.time = 0f;
        source.volume = 0.15f;
        source.pitch = 0.75f;
        source.Play();
        StartCoroutine(RestoreSourceAfterDelay(clip.length / 0.75f));
    }

    private IEnumerator RestoreSourceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        source.volume = 0.5f;
        source.pitch = 1f;
    }

    public void PlayTemblor()
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Efectos/temblor");
        if (clip == null) return;
        source.PlayOneShot(clip);
        StartCoroutine(StopAfterDelay(4f));
    }

    public void PlayExplosion()
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Efectos/explosion");
        if (clip == null) return;
        source.PlayOneShot(clip);
    }

    public void PlayTrumpet()
    {
        StartCoroutine(PlayTrumpetSequence());
    }

    public void PlaySlime()
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Efectos/slime");
        if (clip != null)
            uiSource.PlayOneShot(clip);
    }

    public void PlayHolyBeam()
    {
        source.PlayOneShot(GenerateTone(880, 0.15f, 0.4f));
        StartCoroutine(PlayHolyBeamDelayed(0.05f));
    }

    public void PlayChoir()
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Efectos/choir");
        if (clip != null)
            source.PlayOneShot(clip, 0.7f);
    }

    public void PlaySalto2()
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Efectos/salto2");
        if (clip != null)
            source.PlayOneShot(clip, 0.8f);
    }

    public void PlayNinja()
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Efectos/ninja");
        if (clip != null)
            source.PlayOneShot(clip, 0.7f);
    }

    IEnumerator PlayHolyBeamDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        source.PlayOneShot(GenerateTone(1320, 0.25f, 0.35f));
        yield return new WaitForSeconds(0.08f);
        source.PlayOneShot(GenerateTone(1760, 0.2f, 0.25f));
    }

    IEnumerator PlayTrumpetSequence()
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Efectos/Trumpet");
        if (clip != null)
            source.PlayOneShot(clip, 0.9f);
        yield break;
    }

    private IEnumerator StopAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (source.isPlaying)
            source.Stop();
    }

    public void PlayGameplayMusic()
    {
        string chosen = gameplayTracks[Random.Range(0, gameplayTracks.Length)];
        AudioClip clip = Resources.Load<AudioClip>($"Sounds/Fondo/{chosen}");
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.volume = chosen == "deuslower-fantasy-medieval-ambient-237371" ? 0.5f : 0.3f;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayMenuMusic()
    {
        currentTrackIndex = 0;
        musicSource.loop = false;
        PlayNextMenuTrack();
    }

    private void PlayNextMenuTrack()
    {
        string chosen = menuTracks[currentTrackIndex];
        currentTrackIndex = (currentTrackIndex + 1) % menuTracks.Length;
        AudioClip clip = Resources.Load<AudioClip>($"Sounds/Fondo/{chosen}");
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.volume = 0.3f;
            musicSource.Play();
            menuQueueCoroutine = StartCoroutine(QueueNextMenuTrack(clip.length));
        }
    }

    private IEnumerator QueueNextMenuTrack(float delay)
    {
        yield return new WaitForSeconds(delay - crossfadeDuration);

        float elapsed = 0f;
        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0.3f, 0f, elapsed / crossfadeDuration);
            yield return null;
        }
        musicSource.volume = 0f;

        PlayNextMenuTrack();
    }

    public void PlayWinLoseMusic()
    {
        if (menuQueueCoroutine != null)
        {
            StopCoroutine(menuQueueCoroutine);
            menuQueueCoroutine = null;
        }
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Fondo/win-lose");
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.volume = 0.3f;
            musicSource.loop = false;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (menuQueueCoroutine != null)
        {
            StopCoroutine(menuQueueCoroutine);
            menuQueueCoroutine = null;
        }
        musicSource.Stop();
    }

    private const float pitchRange = 0.1f;

    static AudioClip GenerateTone(float freq, float duration, float volume)
    {
        float pitch = 1f + Random.Range(-pitchRange, pitchRange);
        freq *= pitch;
        int sampleRate = 44100;
        int samples = Mathf.FloorToInt(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (float)i / samples;
            data[i] = volume * Mathf.Sin(2f * Mathf.PI * freq * t) * envelope;
        }
        AudioClip clip = AudioClip.Create("Tone", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip GenerateDescendingTone(float startFreq, float endFreq, float duration, float volume)
    {
        float pitch = 1f + Random.Range(-pitchRange, pitchRange);
        startFreq *= pitch;
        endFreq *= pitch;
        int sampleRate = 44100;
        int samples = Mathf.FloorToInt(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = Mathf.Lerp(startFreq, endFreq, t / duration);
            float envelope = 1f - (float)i / samples;
            data[i] = volume * Mathf.Sin(2f * Mathf.PI * freq * t) * envelope;
        }
        AudioClip clip = AudioClip.Create("Descending", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip GenerateNoise(float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = Mathf.FloorToInt(sampleRate * duration);
        float[] data = new float[samples];
        float vol = volume * (1f + Random.Range(-0.15f, 0.15f));
        for (int i = 0; i < samples; i++)
        {
            float envelope = 1f - (float)i / samples;
            data[i] = vol * Random.Range(-1f, 1f) * envelope;
        }
        AudioClip clip = AudioClip.Create("Noise", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip GenerateChord(float[] freqs, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = Mathf.FloorToInt(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (float)i / samples;
            float sum = 0f;
            foreach (float f in freqs)
            {
                float pitch = 1f + Random.Range(-0.05f, 0.05f);
                sum += Mathf.Sin(2f * Mathf.PI * f * pitch * t);
            }
            data[i] = volume * (sum / freqs.Length) * envelope;
        }
        AudioClip clip = AudioClip.Create("Chord", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static Coroutine hitStopCoroutine;
    public static void HitStop(float duration = 0.05f)
    {
        if (hitStopCoroutine != null) return;
        if (Instance != null)
            hitStopCoroutine = Instance.StartCoroutine(Instance.HitStopRoutine(duration));
    }

    IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        hitStopCoroutine = null;
    }
}
