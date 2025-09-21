using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class cardManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] RectTransform card, canvasRoot, claimLocation;
    [SerializeField] float snapRadius = 500f, snapBackTime = 0.15f;
    public int cardNumber;
    public int positionInHand;
    RectTransform parentRT;
    Canvas canvas;
    Vector2 startPos;
    Coroutine snapCR;

    cardAI cardAI;

    void Awake()
    {
        if (!card) card = (RectTransform)transform;
        parentRT = (RectTransform)card.parent;
        if (!canvasRoot) canvasRoot = card.root as RectTransform;
        canvas = canvasRoot.GetComponent<Canvas>();

        claimLocation = GameObject.Find("Ai CLAIM").GetComponent<RectTransform>();
        cardAI = GameObject.Find("GeminiManager").GetComponent<cardAI>();
    }
    void Start() {
        startPos = card.anchoredPosition; // after layout
    }

    public void OnBeginDrag(PointerEventData e) {}

    public void OnDrag(PointerEventData e)
    {
        var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, e.position, cam, out local);
        card.anchoredPosition = local;  // correct space = parent
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (CloseTo(claimLocation, snapRadius)) {
            StartCoroutine(SnapBack(startPos, snapBackTime));
            Debug.Log("Using Card!");
            if (cardNumber == 1)
            {
                Debug.Log("Card 1 used");
                cardAI.AskAICard();
                gameObject.GetComponentInParent<spawnCards>().RemoveSpecificCard(positionInHand);
            }

            return;
        }
        if (snapCR != null) StopCoroutine(snapCR);
        snapCR = StartCoroutine(SnapBack(startPos, snapBackTime));
    }

    bool CloseTo(RectTransform t, float r)
    {
        Vector2 cardPos   = (Vector2)canvasRoot.InverseTransformPoint(card.position);
        Vector2 targetPos = (Vector2)canvasRoot.InverseTransformPoint(t.TransformPoint(t.rect.center));
        return Vector2.Distance(cardPos, targetPos) <= r;
    }

    System.Collections.IEnumerator SnapBack(Vector2 to, float t)
    {
        Vector2 from = card.anchoredPosition; float a = 0f;
        while (a < 1f) {
            a += Time.unscaledDeltaTime / t;
            card.anchoredPosition = Vector2.Lerp(from, to, Mathf.SmoothStep(0,1,a));
            yield return null;
        }
        card.anchoredPosition = to; snapCR = null;
    }
}
