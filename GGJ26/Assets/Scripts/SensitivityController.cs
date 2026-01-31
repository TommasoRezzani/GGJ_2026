using UnityEngine;
using UnityEngine.UI;

public class MouseSensitivityController : MonoBehaviour
{
    // MODIFICA: Usiamo private + SerializeField. 
    // È più sicuro perché altri script non possono "rompere" il riferimento allo slider,
    // ma tu puoi comunque assegnarlo dall'Inspector.
    [SerializeField] 
    private Slider sensitivitySlider;

    // Questa rimane public e static perché DEVE essere letta dallo script della Camera.
    // Nota: Le variabili statiche NON si vedono nell'Inspector di base.
    public static float fMouseSensitivity = 2.0f;

    void Start()
    {
        // Recupera il valore salvato (default 2.0)
        fMouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2.0f);
        
        // Controllo di sicurezza: se ti sei dimenticato di trascinare lo slider, non crasha
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = fMouseSensitivity;
            sensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);
        }
        else
        {
            Debug.LogError("Attenzione: Non hai assegnato lo Slider nell'Inspector!");
        }
    }

    // Anche qui, se il metodo è usato solo dallo slider via codice (AddListener), 
    // potresti farlo privato, ma spesso si lascia public per debug o eventi UI diretti.
    public void UpdateSensitivity(float newValue)
    {
        fMouseSensitivity = newValue;
        PlayerPrefs.SetFloat("MouseSensitivity", fMouseSensitivity);
    }
}