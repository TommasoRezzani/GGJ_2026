using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMaskHandler : MonoBehaviour
{
    [SerializeField] public List<GameObject> mask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DeActivateAnyMask(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            DeActivateAnyMask(0);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            DeActivateAnyMask(1);
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            DeActivateAnyMask(2);
        }
    }

    private void DeActivateAnyMask(int m)
    {
        //deactivatemask but for any mask excluse the variabile

        for (int n = 0; n < mask.Count; n++)
        {
            //check if index of masks list is equal to 0
            if (mask.IndexOf(mask[n]) == m)
            {
                mask[n].SetActive(true);
            }
            else
            {
                mask[n].SetActive(false);
            }
        }
    }
}