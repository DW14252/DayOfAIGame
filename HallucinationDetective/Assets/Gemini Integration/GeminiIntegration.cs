using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Integrates with the Google Gemini API to send prompts and receive generated text.
/// </summary>
public class GeminiIntegration : MonoBehaviour
{
    /// <summary>
    /// The API key for accessing the Gemini API.
    /// </summary>
    [SerializeField]
    private string apiKey;

    /// <summary>
    /// The URL for the Gemini API's generateContent endpoint.
    /// </summary>
    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1/models/gemini-2.0-flash:generateContent";

    /// <summary>
    /// Represents a part of the content in the Gemini API request/response.
    /// </summary>
    [System.Serializable]
    private class Part
    {
        /// <summary>
        /// The text content of the part.
        /// </summary>
        public string text;
    }

    /// <summary>
    /// Represents the content in the Gemini API request/response.
    /// </summary>
    [System.Serializable]
    private class Content
    {
        /// <summary>
        /// The list of parts that make up the content.
        /// </summary>
        public List<Part> parts;

        /// <summary>
        /// The role of the content (e.g., "user", "model").
        /// </summary>
        public string role;
    }

    /// <summary>
    /// Represents the request body sent to the Gemini API.
    /// </summary>
    [System.Serializable]
    private class RequestBody
    {
        /// <summary>
        /// The list of content objects in the request.
        /// </summary>
        public List<Content> contents;
    }

    /// <summary>
    /// Represents a candidate response from the Gemini API.
    /// </summary>
    [System.Serializable]
    private class Candidate
    {
        /// <summary>
        /// The content of the candidate response.
        /// </summary>
        public Content content;

        /// <summary>
        /// The reason the candidate was finished.
        /// </summary>
        public string finishReason;

        /// <summary>
        /// The average log probabilities of the candidate.
        /// </summary>
        public float avgLogprobs;
    }

    /// <summary>
    /// Represents the response body received from the Gemini API.
    /// </summary>
    [System.Serializable]
    private class ResponseBody
    {
        /// <summary>
        /// The list of candidate responses.
        /// </summary>
        public List<Candidate> candidates;
    }

    /// <summary>
    /// Stores the conversation history for the Gemini API interaction.
    /// </summary>
    private List<Content> conversationHistory = new List<Content>();

    /// <summary>
    /// The default personality prompt used to initialize the conversation.
    /// </summary>
    [SerializeField]
    private string personalityPrompt = "You are a helpful assistant.";

    /// <summary>
    /// Sends a prompt to the Gemini API and invokes a callback with the generated text.
    /// </summary>
    /// <param name="prompt">The prompt to send to the Gemini API.</param>
    /// <param name="callback">The callback to invoke with the generated text.</param>
    /// <returns>An IEnumerator for use with StartCoroutine.</returns>
    public IEnumerator SendPromptToGemini(string prompt, System.Action<string> callback)
    {
        return SendPromptToGemini(prompt, callback, personalityPrompt);
    }

    /// <summary>
    /// Sends a prompt to the Gemini API with a custom personality prompt and invokes a callback with the generated text.
    /// </summary>
    /// <param name="prompt">The prompt to send to the Gemini API.</param>
    /// <param name="callback">The callback to invoke with the generated text.</param>
    /// <param name="customPersonalityPrompt">The custom personality prompt to use.</param>
    /// <returns>An IEnumerator for use with StartCoroutine.</returns>
    public IEnumerator SendPromptToGemini(string prompt, System.Action<string> callback, string customPersonalityPrompt)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("Gemini API Key is not set in the Inspector!");
            callback?.Invoke(null);
            yield break;
        }

        if (conversationHistory.Count == 0)
        {
            conversationHistory.Add(new Content { parts = new List<Part> { new Part { text = customPersonalityPrompt } }, role = "model" });
        }

        conversationHistory.Add(new Content { parts = new List<Part> { new Part { text = prompt } }, role = "user" });

        RequestBody requestBody = new RequestBody
        {
            contents = conversationHistory
        };

        string jsonRequestBody = JsonUtility.ToJson(requestBody);
        string apiUrlWithKey = $"{GeminiApiUrl}?key={apiKey}";

        using (UnityWebRequest request = new UnityWebRequest(apiUrlWithKey, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error}");
                Debug.LogError($"Response Code: {request.responseCode}");
                Debug.LogError($"Response Body: {request.downloadHandler?.text}");
                callback?.Invoke(null);
            }
            else
            {
                ResponseBody response = JsonUtility.FromJson<ResponseBody>(request.downloadHandler.text);

                if (response != null && response.candidates != null && response.candidates.Count > 0 &&
                    response.candidates[0].content != null && response.candidates[0].content.parts != null && response.candidates[0].content.parts.Count > 0)
                {
                    string generatedText = response.candidates[0].content.parts[0].text;

                    conversationHistory.Add(new Content { parts = new List<Part> { new Part { text = generatedText } }, role = "model" });

                    callback?.Invoke(generatedText);
                }
                else
                {
                    Debug.LogWarning("No valid response received from Gemini API or response format is incorrect.");
                    callback?.Invoke(null);
                }
            }
        }
    }

    /// <summary>
    /// Clears the conversation history.
    /// </summary>
    public void ClearConversationHistory()
    {
        conversationHistory.Clear();
    }

    /// <summary>
    /// Sets a new personality prompt for the conversation.
    /// </summary>
    /// <param name="newPersonalityPrompt">The new personality prompt.</param>
    public void SetPersonalityPrompt(string newPersonalityPrompt)
    {
        if (conversationHistory.Count > 0)
        {
            conversationHistory[0].parts[0].text = newPersonalityPrompt;
        }
        else
        {
            personalityPrompt = newPersonalityPrompt;
        }
    }
}