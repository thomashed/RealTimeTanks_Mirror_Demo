using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    private static InputController inputControllerInstance = null;
    
    void Awake()
    {
        if (inputControllerInstance is null)
        {
            DontDestroyOnLoad(gameObject);
            inputControllerInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        //if (!Mouse.current.rightButton.wasPressedThisFrame) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
    }

}
