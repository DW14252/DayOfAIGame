using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections.Generic;

[Serializable] public class Claim { public string id, text, topic, truth_label; public int difficulty; }
[Serializable] public class Root  { public Claim[] claims; }

public class claimGenerator : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject GeminiManager;
    public spawnCards cardSpawner;
    public TMP_Text detectiveNotesText;
    public int score;
    public TMP_Text detectiveScoreText;

    [SerializeField] RectTransform card, canvasRoot, truePile, hallPile;
    [SerializeField] float snapRadius = 500f, snapBackTime = 0.15f;
    public TMP_Text claimText;

    public GameObject screenFlash;

    Vector2 startPos;
    Coroutine typeCR, snapCR;
    string currentFullText = "";
    bool isTyping = false;
    bool isTrue = false;

    int current_difficulty;
    int streak = 0;



    // Make a dictionary of max difficulty based on current score
    // e.g. 0-10 = 1, 11-25 = 2, 26-50 = 3, 51+ = 4
    // This can be adjusted for difficulty tuning
    Dictionary<int, int> difficultyMap = new Dictionary<int, int>()
    {
        { 10, 1 },
        { 25, 2 },
        { 50, 3 },
        { 70, 4 },
        { int.MaxValue, 5 }
    };


    int max_difficulty = 1; // 1 = easy, 2 = medium, 3 = hard

    // add (optional) cached canvas camera if you prefer
    Canvas canvas;
    void Awake() {
        if (!card) card = transform as RectTransform;
        if (!canvasRoot) canvasRoot = card.root as RectTransform;
        canvas = canvasRoot.GetComponentInParent<Canvas>();
        startPos = card.anchoredPosition;
    }

    void Start() => newClaim(max_difficulty);

    public void newClaim(int max_difficulty)
    {
        cardSpawner.SpawnCard();
        cardSpawner.SpawnCard();

        detectiveNotesText.text = "";
        string json = System.IO.File.ReadAllText(Application.dataPath + "/Resources/claims.json");
        var root = JsonUtility.FromJson<Root>(json);
        var filtered = Array.FindAll(root.claims, c => c.difficulty <= max_difficulty);
        if (filtered.Length == 0) return;

        var clm = filtered[UnityEngine.Random.Range(0, filtered.Length)];
        isTrue = clm.truth_label == "true";
        current_difficulty = clm.difficulty;

        GeminiManager.GetComponent<cardAI>().claim = clm.text;

        if (typeCR != null) StopCoroutine(typeCR);
        currentFullText = clm.text;
        typeCR = StartCoroutine(TypewriterEffect(currentFullText));
    }

    System.Collections.IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        claimText.text = "";
        foreach (char ch in text)
        {
            claimText.text += ch;
            yield return new WaitForSeconds(0.05f);
        }
        isTyping = false;
        typeCR = null;
    }

    // --- Drag handlers (UI / Screen Space - Overlay) ---
    public void OnBeginDrag(PointerEventData e)
    {
        // OPTIONAL CHANGE: reveal full text immediately if still typing
        if (isTyping && typeCR != null)
        {
            StopCoroutine(typeCR);
            claimText.text = currentFullText;
            isTyping = false;
            typeCR = null;
        }
    }

    public void OnDrag(PointerEventData e)
    {
        // Screen Space - Camera or World Space
        var parent = (RectTransform)card.parent;
        var cam = e.pressEventCamera ?? canvas?.worldCamera; // no nulls in camera-space
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, e.position, cam, out local);
        card.anchoredPosition = local;
    }


    public void OnEndDrag(PointerEventData e)
    {
        if (CloseTo(truePile, snapRadius))
        {
            //card.anchoredPosition = truePile.anchoredPosition;
            StartCoroutine(SnapBack(startPos, snapBackTime));
            Debug.Log(isTrue ? "Correct!" : "Wrong!");
            if (isTrue)
            {
                score += current_difficulty + streak;
                detectiveScoreText.text = "Score: " + score;
                //make the text flash green
                StartCoroutine(FlashText(detectiveScoreText, Color.green));
                // flash the screen green color
                screenFlash.SetActive(true);
                screenFlash.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 1, 0, 0.1f);
                Invoke("DisableFlash", 0.1f);
                if (streak < 3) streak++;
            }
            else
            {
                streak = 0;
                // flash the screen red color
                screenFlash.SetActive(true);
                screenFlash.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 0, 0, 0.1f);
                Invoke("DisableFlash", 0.1f);
            }
            // Update max difficulty based on current score
            foreach (var kvp in difficultyMap)
            {
                if (score <= kvp.Key)
                {
                    max_difficulty = kvp.Value;
                    break;
                }
            }
            newClaim(max_difficulty);
            return;
        }
        if (CloseTo(hallPile, snapRadius))
        {
            //card.anchoredPosition = hallPile.anchoredPosition;
            StartCoroutine(SnapBack(startPos, snapBackTime));
            Debug.Log(!isTrue ? "Correct!" : "Wrong!");
            if (!isTrue)
            {
                score += current_difficulty + streak;
                detectiveScoreText.text = "Score: " + score;
                //make the text flash green
                StartCoroutine(FlashText(detectiveScoreText, Color.green));
                // flash the screen green color
                screenFlash.SetActive(true);
                screenFlash.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 1, 0, 0.1f);
                Invoke("DisableFlash", 0.1f);

                if (streak < 3) streak++;
            }
            else
            {
                streak = 0;
                // flash the screen red color
                screenFlash.SetActive(true);
                screenFlash.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 0, 0, 0.1f);
                Invoke("DisableFlash", 0.1f);

            }
            // Update max difficulty based on current score
            foreach (var kvp in difficultyMap)
            {
                if (score <= kvp.Key)
                {
                    max_difficulty = kvp.Value;
                    break;
                }
            }
            newClaim(max_difficulty);
            return;
        }

        if (snapCR != null) StopCoroutine(snapCR);
        snapCR = StartCoroutine(SnapBack(startPos, snapBackTime));
    }
    bool CloseTo(RectTransform t, float r)
    {
        Vector2 cardPos = (Vector2)canvasRoot.InverseTransformPoint(card.position);
        Vector2 targetPos = (Vector2)canvasRoot.InverseTransformPoint(t.TransformPoint(t.rect.center));
        return Vector2.Distance(cardPos, targetPos) <= r;
    }
    System.Collections.IEnumerator SnapBack(Vector2 to, float t)
    {
        Vector2 from = card.anchoredPosition; float a = 0f;
        while (a < 1f)
        {
            a += Time.unscaledDeltaTime / t;
            card.anchoredPosition = Vector2.Lerp(from, to, Mathf.SmoothStep(0, 1, a));
            yield return null;
        }
        card.anchoredPosition = to; snapCR = null;
    }
    System.Collections.IEnumerator FlashText(TMP_Text textElement, Color flashColor)
    {
        Color originalColor = textElement.color;
        textElement.color = flashColor;
        yield return new WaitForSeconds(0.3f);
        textElement.color = originalColor;
    }

    void DisableFlash()
    {
        screenFlash.SetActive(false);
    }
}
