using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
public class cardManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] RectTransform card, canvasRoot, claimLocation;
    [SerializeField] float snapRadius = 500f, snapBackTime = 0.15f;
    public int cardNumber;
    public int positionInHand;

    Canvas canvas;
    Vector2 startPos;
    Coroutine snapCR;
    cardAI cardAI;

    public GameObject tutorialManager;

    void Awake()
    {
        if (!card) card = (RectTransform)transform;
        if (!canvasRoot) canvasRoot = card.root as RectTransform;
        canvas = canvasRoot.GetComponent<Canvas>();

        if (!claimLocation)
            claimLocation = GameObject.Find("Ai CLAIM").GetComponent<RectTransform>();

        // if in mainMenu, find the Ai CLAIM object
        if (SceneManager.GetActiveScene().name == "MainGame")
        {
            cardAI = GameObject.Find("GeminiManager").GetComponent<cardAI>();
        }
    }

    void Start() {
        // after initial layout
        startPos = card.anchoredPosition;
    }

    // called automatically by Unity when parent changes
    void OnTransformParentChanged() {
        // parent changed due to left-shift; reset local start position
        startPos = card.anchoredPosition;
    }

    // optional: lets external code force a refresh after moves
    public void OnRepositioned() {
        startPos = card.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData e) {
        // refresh in case layout just changed
        startPos = card.anchoredPosition;
    }

    public void OnDrag(PointerEventData e) {
        var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 pos;
        // use the CURRENT parent, never a cached one
        RectTransform parentRT = (RectTransform)card.parent;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, e.position, cam, out pos);
        card.anchoredPosition = pos;
    }

    public void OnEndDrag(PointerEventData e) {
        // if not in mainMenu, check if close to claimLocation
        if (SceneManager.GetActiveScene().name != "MainGame")
        {
            if (CloseTo(claimLocation, snapRadius))
            {
                // Snap to claimLocation
                tutorialManager.GetComponent<TutorialManager>().draggedDummyCard = true;
                if (snapCR != null) StopCoroutine(snapCR);
                snapCR = StartCoroutine(SnapBack(startPos, snapBackTime));
                return;
            }
        }

        if (CloseTo(claimLocation, snapRadius))
        {
            if (cardNumber == 1)
            {
                cardAI.AskAICard();
                GetComponentInParent<spawnCards>().RemoveSpecificCard(positionInHand);
            }
            else if (cardNumber == 2)
            {
                cardAI.CheckSources();
                GetComponentInParent<spawnCards>().RemoveSpecificCard(positionInHand);
            }
            else if (cardNumber == 3)
            {
                cardAI.CounterEvidence();
                GetComponentInParent<spawnCards>().RemoveSpecificCard(positionInHand);
            }
            else if (cardNumber == 4)
            {
                cardAI.TimeCheck();
                GetComponentInParent<spawnCards>().RemoveSpecificCard(positionInHand);
            }
            else if (cardNumber == 5)
            {
                cardAI.RoleCheck();
                GetComponentInParent<spawnCards>().RemoveSpecificCard(positionInHand);
            }
            else if (cardNumber == 6)
            {
                cardAI.LocationCheck();
                GetComponentInParent<spawnCards>().RemoveSpecificCard(positionInHand);
            }
            return;
        }
        if (snapCR != null) StopCoroutine(snapCR);
        snapCR = StartCoroutine(SnapBack(startPos, snapBackTime));
    }

    bool CloseTo(RectTransform t, float r) {
        Vector2 cardPos   = (Vector2)canvasRoot.InverseTransformPoint(card.position);
        Vector2 targetPos = (Vector2)canvasRoot.InverseTransformPoint(t.TransformPoint(t.rect.center));
        return Vector2.Distance(cardPos, targetPos) <= r;
    }

    System.Collections.IEnumerator SnapBack(Vector2 to, float t) {
        Vector2 from = card.anchoredPosition; float a = 0f;
        while (a < 1f) {
            a += Time.unscaledDeltaTime / t;
            card.anchoredPosition = Vector2.Lerp(from, to, Mathf.SmoothStep(0,1,a));
            yield return null;
        }
        card.anchoredPosition = to; snapCR = null;
    }
}
