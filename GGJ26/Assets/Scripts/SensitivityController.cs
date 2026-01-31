using UnityEngine;
using UnityEngine.UI;

public class MouseSensitivityController : MonoBehaviour
{
    // Riferimento allo Slider nella UI
    public Slider sensitivitySlider;

    // Variabile statica per accedervi facilmente da altri script (es. MouseLook)
    public static float mouseSensitivity = 2.0f;

    void Start()
    {
        // Se abbiamo salvato la sensibilità in precedenza, caricala
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2.0f);
        
        // Imposta lo slider al valore corrente
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = mouseSensitivity;
            // Aggiunge un listener che chiama il metodo quando lo slider si muove
            sensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);
        }
    }

    public void UpdateSensitivity(float newValue)
    {
        mouseSensitivity = newValue;
        // Opzionale: salva il valore per la prossima sessione di gioco
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
    }
}