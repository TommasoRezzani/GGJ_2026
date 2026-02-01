using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Trascina qui l'oggetto padre 'Player'")]
    [SerializeField] private PlayerCharacterController player;

    [Header("Audio Sources")]
    [Tooltip("Sorgente per la musica (Spunta Loop e PlayOnAwake)")]
    [SerializeField] private AudioSource musicSource; 
    
    [Tooltip("Sorgente per i salti (Togli Loop)")]
    [SerializeField] private AudioSource sfxSource;   
    
    [Tooltip("Sorgente per i passi/corsa (Spunta Loop, Togli PlayOnAwake)")]
    [SerializeField] private AudioSource stepSource;  

    [System.Serializable]
    public struct MaskAudioProfile
    {
        public string name;             // Solo per ordine nell'inspector (es. "Yuki Profile")
        public MaskType maskType;       // Il tipo di maschera a cui corrisponde
        public AudioClip backgroundMusic;
        public AudioClip jumpSfx;       // Salto normale
        public AudioClip doubleJumpSfx; // Doppio salto (speciale per Yuki)
        public AudioClip walkStepLoop;  // Camminata normale (loop)
        public AudioClip runLoop;       // Corsa veloce Oni (loop)
    }

    [Header("Profiles Setup")]
    [SerializeField] private MaskAudioProfile[] maskProfiles;

    // Stato interno
    private MaskType currentMaskType;
    private bool wasPaused = false;

    // --- ISCRIZIONE AGLI EVENTI DEL PLAYER ---
    private void OnEnable()
    {
        if (player != null)
        {
            player.OnJump += PlayJump;             // Ascolta quando il player salta
            player.OnMaskChanged += UpdateAudioProfile; // Ascolta quando cambia maschera
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.OnJump -= PlayJump;
            player.OnMaskChanged -= UpdateAudioProfile;
        }
    }

    private void Start()
    {
        // Inizializza l'audio con la maschera che il player ha all'inizio
        if (player != null)
        {
            UpdateAudioProfile(player.CurrentMask);
        }
    }

    private void Update()
    {
        if (player == null) return;

        // 1. GESTIONE PAUSA (Se il TimeScale è fermo, mettiamo in pausa l'audio)
        bool isPaused = Time.timeScale < 0.01f;
        
        if (isPaused)
        {
            if (!wasPaused)
            {
                // Mettiamo in pausa le fonti che loopano (musica e passi)
                if (musicSource.isPlaying) musicSource.Pause();
                if (stepSource.isPlaying) stepSource.Pause();
                wasPaused = true;
            }
            return; // Esci dall'Update, non fare altro finché è in pausa
        }
        else if (wasPaused) 
        {
            // Riprendiamo l'audio
            musicSource.UnPause();
            stepSource.UnPause();
            wasPaused = false;
        }

        // 2. GESTIONE PASSI / CORSA (Solo se non siamo in pausa)
        HandleMovementSound();
    }

    // --- LOGICA MOVIMENTO (PASSI) ---
    private void HandleMovementSound()
    {
        // Se il player è in aria OPPURE è fermo (velocità quasi zero) -> Stop passi
        if (!player.IsGrounded || player.CurrentSpeed < 0.1f)
        {
            if (stepSource.isPlaying) stepSource.Stop();
            return;
        }

        MaskAudioProfile profile = GetProfile(currentMaskType);

        // Controllo specifico per la corsa dell'Oni
        if (currentMaskType == MaskType.Oni && player.IsOniSprinting)
        {
            // Stiamo correndo: usa il suono di corsa
            if (stepSource.clip != profile.runLoop)
            {
                stepSource.clip = profile.runLoop;
                stepSource.pitch = 1.0f; 
                stepSource.Play();
            }
        }
        else
        {
            // Camminata normale (Yuki o Oni base)
            if (stepSource.clip != profile.walkStepLoop)
            {
                stepSource.clip = profile.walkStepLoop;
                stepSource.pitch = 1.0f; // Puoi variare questo valore leggermente per randomizzare
                stepSource.Play();
            }
        }

        // Sicurezza: se dovrebbe suonare ma si è fermato, fallo ripartire
        if (!stepSource.isPlaying && stepSource.clip != null) 
        {
            stepSource.Play();
        }
    }

    // --- LOGICA SALTO (CHIAMATA DALL'EVENTO) ---
    private void PlayJump(bool isDoubleJump)
    {
        MaskAudioProfile profile = GetProfile(currentMaskType);

        if (isDoubleJump)
        {
            // È un doppio salto (Yuki)
            if (profile.doubleJumpSfx != null)
            {
                // Variamo leggermente il pitch per renderlo dinamico
                sfxSource.pitch = Random.Range(0.95f, 1.05f);
                sfxSource.PlayOneShot(profile.doubleJumpSfx);
                sfxSource.pitch = 1.0f; // Reset
            }
            else if (profile.jumpSfx != null)
            {
                // Fallback: se manca il suono doppio, usa quello normale
                sfxSource.PlayOneShot(profile.jumpSfx);
            }
        }
        else
        {
            // Salto normale
            if (profile.jumpSfx != null)
            {
                sfxSource.pitch = Random.Range(0.9f, 1.1f);
                sfxSource.PlayOneShot(profile.jumpSfx);
                sfxSource.pitch = 1.0f;
            }
        }
    }

    // --- CAMBIO PROFILO (QUANDO CAMBI MASCHERA) ---
    private void UpdateAudioProfile(MaskType newMask)
    {
        currentMaskType = newMask;
        MaskAudioProfile profile = GetProfile(newMask);

        // Cambia la musica di sottofondo solo se è diversa da quella attuale
        if (musicSource.clip != profile.backgroundMusic)
        {
            musicSource.Stop();
            musicSource.clip = profile.backgroundMusic;
            musicSource.Play();
        }

        // Ferma i passi attuali per evitare di avere il suono vecchio incastrato
        stepSource.Stop();
        stepSource.clip = null; 
    }

    // Funzione helper per trovare il profilo giusto nell'array
    private MaskAudioProfile GetProfile(MaskType type)
    {
        foreach (var p in maskProfiles)
        {
            if (p.maskType == type) return p;
        }
        // Se non trova nulla, ritorna il primo elemento (Default) per evitare errori
        return maskProfiles.Length > 0 ? maskProfiles[0] : new MaskAudioProfile();
    }
}