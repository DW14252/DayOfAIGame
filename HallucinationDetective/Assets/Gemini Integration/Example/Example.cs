using UnityEngine;

public class Example : MonoBehaviour
{
    public GeminiIntegration geminiIntegration;

    void Start()
    {
        if (geminiIntegration == null)
        {
            Debug.LogError("GeminiDirectIntegration script not assigned!");
            return;
        }

        string prompt = "Tell me a one line joke.";
        StartCoroutine(geminiIntegration.SendPromptToGemini(prompt, HandleGeminiResponse));
    }

    void HandleGeminiResponse(string response)
    {
        if (string.IsNullOrEmpty(response))
        {
            Debug.LogError("Gemini API call failed or returned no response.");
        }
        else
        {
            Debug.Log("Gemini Response: " + response);
        }
    }
}