using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class cardAI : MonoBehaviour
{
    public string storyContent;
    public GeminiIntegration geminiIntegration;
    public string claim;
    public TMP_Text detectiveNotesText;

    public GameObject loadingUI;

    const float REPLY_TIMEOUT = 15f; // seconds

    // optional legacy store
    public List<string> AIresponses = new List<string>();
    void returnResponse(string r) { AIresponses.Add(r); }

    void Start()
    {
        // Load story content from text file
        if (string.IsNullOrEmpty(storyContent))
        {
            TextAsset textAsset = Resources.Load<TextAsset>("storyContent");
            if (textAsset != null)
            {
                storyContent = textAsset.text;
            }
            else
            {
                Debug.LogError("Failed to load storyContent.txt from Resources.");
            }
        }
    }

    public void AskAICard() => StartCoroutine(AskAICardEnum());

    // Small: builds prompt, waits for helper, interprets, updates UI
    public System.Collections.IEnumerator AskAICardEnum()
    {
        if (!geminiIntegration) { Debug.LogError("GeminiIntegration missing"); yield break; }

        string prompt = $"Case story context: \n{storyContent}\n\nIs the following claim true or false? Claim: {claim} Answer only with 'true' or 'false'. Be somewhat unpredictable.";
        string reply = null;
        loadingUI.SetActive(true);
        // queue + await this specific response
        yield return StartCoroutine(AIQueue.Enqueue(geminiIntegration, prompt, REPLY_TIMEOUT, r => reply = r));
        loadingUI.SetActive(false);
        Debug.Log("The AI said: " + reply);
        string l = (reply ?? "").ToLowerInvariant();
        if      (l.Contains("true"))  detectiveNotesText.text += "\nAsking AI: True";
        else if (l.Contains("false")) detectiveNotesText.text += "\nAsking AI: False";
        else                          detectiveNotesText.text += "\nAsking AI: No response / unclear";
    }

    // -------- Helper: shared queue + per-request registry --------
    static class AIQueue
    {
        static int nextTicket = 1, nowServing = 1;
        static int nextReqId = 1;
        static readonly Dictionary<int,string> registry = new Dictionary<int,string>();
        static readonly object regLock = new object();

        public static System.Collections.IEnumerator Enqueue(
            GeminiIntegration gi, string prompt, float timeoutSeconds, System.Action<string> onResult)
        {
            // ticketed queue
            int myTicket = nextTicket++;
            yield return new WaitUntil(() => nowServing == myTicket);

            try
            {
                int reqId = nextReqId++;

                // fire request; callback writes into registry
                gi.StartCoroutine(gi.SendPromptToGemini(prompt, r => {
                    lock (regLock) registry[reqId] = r;
                }));

                // await this exact reqId with timeout
                string resp = null;
                float t0 = Time.unscaledTime;
                while (true)
                {
                    bool got = false;
                    lock (regLock)
                    {
                        if (registry.TryGetValue(reqId, out resp)) { registry.Remove(reqId); got = true; }
                    }
                    if (got) break;
                    if (Time.unscaledTime - t0 > timeoutSeconds) { resp = null; break; }
                    yield return null;
                }

                onResult?.Invoke(resp);
            }
            finally
            {
                nowServing++;
            }
        }
    }
}
