  using UnityEngine;

  public class ChangeColorOnClick : MonoBehaviour
  {
      // Define an array of colors to choose from
      public Color[] colors;

      // Reference to the Renderer component
      private Renderer ballRenderer;

      // Index to track current color
      private int currentColorIndex = 0;

      void Start()
      {
          // Get the Renderer component of the GameObject
          ballRenderer = GetComponent<Renderer>();

          // Initialize the colors array if it's empty
          if (colors == null || colors.Length == 0)
          {
              colors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow };
          }

          // Set initial color
          ballRenderer.sharedMaterial.color = colors[currentColorIndex];
      }

      public void OnMouseDown()
      {
          // Change to the next color in the array
          currentColorIndex = (currentColorIndex + 1) % colors.Length;

          // Update the color of the material
          ballRenderer.sharedMaterial.color = colors[currentColorIndex];
      }
  }