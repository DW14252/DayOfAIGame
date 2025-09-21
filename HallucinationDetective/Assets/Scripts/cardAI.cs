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
    public void LocationCheck() => StartCoroutine(LocationCheckEnum());
    public System.Collections.IEnumerator LocationCheckEnum()
    {
        if (!geminiIntegration) { Debug.LogError("GeminiIntegration missing"); yield break; }

        List<string> locations = new List<string> {
            "Ashfield Municipal Archives",
            "Special Collections Vault 3C",
            "Corridor B1",
            "Control Room",
            "Air Handling Unit B",
            "Mail Room",
            "Store Room",
            "Vault 3 corridor",
            "Corridor (Camera 11 view)",
            "Freight lift L-B",
            "Roof hatch R-2",
            "Rooftop",
            "Victoria Street"
        };

        string prompt = $"Case story context: \n{storyContent}\n\nThe detective is considering the following claim: {claim}\nBased on the story content, where did this event most likely take place? Regardless of what the claim says, where did this event most likely take place? Answer with one of the locations only. e.g. respond exactly with one of these; {string.Join(", ", locations)}. If none of these locations are involved, say 'none of these locations'.";
        string reply = null;
        loadingUI.SetActive(true);
        // queue + await this specific response
        yield return StartCoroutine(AIQueue.Enqueue(geminiIntegration, prompt, REPLY_TIMEOUT, r => reply = r));
        loadingUI.SetActive(false);
        Debug.Log("The AI said: " + reply);

        // lower replycase for easier comparison
        reply = reply?.ToLower();

        // check if reply is in locations
        for (int i = 0; i < locations.Count; i++)
        {
            if (reply != null && reply.Contains(locations[i].ToLower()))
            {
                reply = locations[i];
                detectiveNotesText.text += $"\nLocation Check: Location: {reply}";
                yield break;
            }
        }
        detectiveNotesText.text += $"\nLocation Check: No valid location found.";
    }
    public void RoleCheck() => StartCoroutine(RoleCheckEnum());
    public System.Collections.IEnumerator RoleCheckEnum()
    {
        if (!geminiIntegration) { Debug.LogError("GeminiIntegration missing"); yield break; }

        List<string> characters = new List<string> {
            "Mia Okereke",
            "Julian B",
            "Sentinel-9",
            "Ledger-Box",
            "Kestrel-2",
            "NIGHTFALL",
            "IT lead",
            "Parks crew",
            "rooftop quadcopter"
        };

        string prompt = $"Case story context: \n{storyContent}\n\nThe detective is considering the following claim: {claim}\nBased on the story content, which character is involved in this event? Regardless of what the claim says, who was most involved in this event? Answer with one of the characters names only. e.g. respond exactly with one of these; {string.Join(", ", characters)}. If none of these characters are involved, say 'none of these characters'.";
        string reply = null;
        loadingUI.SetActive(true);
        // queue + await this specific response
        yield return StartCoroutine(AIQueue.Enqueue(geminiIntegration, prompt, REPLY_TIMEOUT, r => reply = r));
        loadingUI.SetActive(false);
        Debug.Log("The AI said: " + reply);
        // lower replycase for easier comparison
        reply = reply?.ToLower();

        // check if reply is in characters and if so find index in the characters list
        for (int i = 0; i < characters.Count; i++)
        {
            if (reply != null && reply.Contains(characters[i].ToLower()))
            {
                reply = characters[i];
                detectiveNotesText.text += $"\nRole Check: Character: {reply}";
                yield break;
            }
        }
        // if we get here, no valid character found
        detectiveNotesText.text += $"\nRole Check: No valid character found.";
    }
    public void TimeCheck() => StartCoroutine(TimeCheckEnum());
    public System.Collections.IEnumerator TimeCheckEnum()
    {
        if (!geminiIntegration) { Debug.LogError("GeminiIntegration missing"); yield break; }

        string prompt = $"Case story context: \n{storyContent}\n\nThe detective is considering the following claim: {claim}\nBased on the story content, do the times line up? Answer only with 'yes' or 'no'.";
        string reply = null;
        loadingUI.SetActive(true);
        // queue + await this specific response
        yield return StartCoroutine(AIQueue.Enqueue(geminiIntegration, prompt, REPLY_TIMEOUT, r => reply = r));
        loadingUI.SetActive(false);
        Debug.Log("The AI said: " + reply);
        // 1 in 4 chance to pick randomly
        if (Random.value < 0.25f)
        {
            reply = Random.value < 0.5f ? "yes" : "no";
        }
        string l = (reply ?? "").ToLowerInvariant();
        if      (l.Contains("yes")) detectiveNotesText.text += "\nTime Check: Times match.";
        else if (l.Contains("no"))  detectiveNotesText.text += "\nTime Check: Times do not match.";
        else                        detectiveNotesText.text += "\nTime Check: Timing is unclear.";
    }

    public void CounterEvidence() => StartCoroutine(CounterEvidenceEnum());

    public System.Collections.IEnumerator CounterEvidenceEnum()
    {
        if (!geminiIntegration) { Debug.LogError("GeminiIntegration missing"); yield break; }

        string prompt = $"Case story context: \n{storyContent}\n\nThe detective is considering the following claim: {claim}\nProvide one piece of counter-evidence that challenges this claim. Quote one piece of evidence from the story content that challenges the claim, in single quotes. e.g. 'exact word for word quote from story content'. If no counter-evidence exists, say 'No counter-evidence found'.";
        string reply = null;
        loadingUI.SetActive(true);
        // queue + await this specific response
        yield return StartCoroutine(AIQueue.Enqueue(geminiIntegration, prompt, REPLY_TIMEOUT, r => reply = r));
        loadingUI.SetActive(false);
        Debug.Log("The AI said: " + reply);
        // extract quote from source if present
        int quoteStart = (reply ?? "").IndexOf('\'');
        int quoteEnd = (reply ?? "").IndexOf('\'', quoteStart + 1);
        string quote = (quoteStart >= 0 && quoteEnd > quoteStart) ? (reply.Substring(quoteStart + 1, quoteEnd - quoteStart - 1)) : null;
        // check if quote is in story content
        if (quote != null && !storyContent.Contains(quote))
        {
            quote = null; // invalid quote
        }
        if (quote != null)
        {
            detectiveNotesText.text += $"\nCounter Evidence: Quote: '{quote}'";
        }
        else
        {
            detectiveNotesText.text += $"\nCounter Evidence: No valid quote found.";
        }
    }

    public void CheckSources() => StartCoroutine(CheckSourcesEnum());

    public System.Collections.IEnumerator CheckSourcesEnum()
    {
        if (!geminiIntegration) { Debug.LogError("GeminiIntegration missing"); yield break; }

        string prompt = $"Case story context: \n{storyContent}\n\nThe detective is considering the following claim: {claim}\nDo the sources in the story content support this claim? Quote one piece of evidence from the story content that supports the claim, in single quotes. e.g. 'exact word for word quote from story content'. If no sources suport the claim, say 'No supporting quote found'. ";
        string reply = null;
        loadingUI.SetActive(true);
        // queue + await this specific response
        yield return StartCoroutine(AIQueue.Enqueue(geminiIntegration, prompt, REPLY_TIMEOUT, r => reply = r));
        loadingUI.SetActive(false);
        Debug.Log("The AI said: " + reply);
        // 1 in 4 chance to have no evidence
        if (Random.value < 0.25f)
        {
            reply = "No supporting quote found.";
        }
        // extract quote from source if present
        int quoteStart = (reply ?? "").IndexOf('\'');
        int quoteEnd = (reply ?? "").IndexOf('\'', quoteStart + 1);
        string quote = (quoteStart >= 0 && quoteEnd > quoteStart) ? (reply.Substring(quoteStart + 1, quoteEnd - quoteStart - 1)) : null;
        // check if quote is in story content
        if (quote != null && !storyContent.Contains(quote))
        {
            quote = null; // invalid quote
        }
        if (quote != null)
        {
            detectiveNotesText.text += $"\nChecking Sources: Quote: '{quote}'";
        }
        else
        {
            detectiveNotesText.text += $"\nChecking Sources: No valid quote found.";
        }

        // string l = (reply ?? "").ToLowerInvariant();
        // if      (l.Contains("1")) detectiveNotesText.text += "\nChecking Sources: Sources agree.";
        // else if (l.Contains("2")) detectiveNotesText.text += "\nChecking Sources: Sources disagree.";
        // else                      detectiveNotesText.text += "\nChecking Sources: Sources unsure.";
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
        // 1 in 4 chance to pick randomly
        if (Random.value < 0.25f)
        {
            reply = Random.value < 0.5f ? "true" : "false";
        }
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
