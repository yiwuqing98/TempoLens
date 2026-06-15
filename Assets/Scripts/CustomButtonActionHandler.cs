using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class CustomButtonActionHandler : MonoBehaviour
{
    public InputActionReference buttonAction; // Drag your InputAction here in the Inspector
    public GameObject sphere; // Drag your sphere GameObject here in the Inspector
    public Color targetColor = Color.red; // Color to change to when button is pressed

    private Renderer sphereRenderer;
    private Color originalColor;

    private void Awake()
    {
        if (sphere != null)
        {
            sphereRenderer = sphere.GetComponent<Renderer>();
            originalColor = sphereRenderer.material.color;
        }
    }

    private void OnEnable()
    {
        buttonAction.action.performed += OnButtonPressed;
        buttonAction.action.Enable();
    }

    private void OnDisable()
    {
        buttonAction.action.performed -= OnButtonPressed;
        buttonAction.action.Disable();
    }

    private void OnButtonPressed(InputAction.CallbackContext context)
    {
        if (sphereRenderer != null)
        {
            sphereRenderer.material.color = targetColor;
        }
    }
}
