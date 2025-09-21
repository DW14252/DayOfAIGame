using UnityEngine;

public class spawnCards : MonoBehaviour
{
    public Transform[] spawnPoints;
    public Transform deckPosition;
    public int ocupiedSpots = 0;

    [System.Serializable]
    public class Cards
    {
        public GameObject prefab;
        public int unlockScore;
        public bool isUnlocked;
        public GameObject unlockScreen;
    }

    [SerializeField] private Cards[] cardPrefabs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RemoveSpecificCard(int index)
    {
        for (int i = index; i < ocupiedSpots - 1; i++)
        {
            Transform currentCard = spawnPoints[i].GetChild(0);
            Transform nextCard = spawnPoints[i + 1].GetChild(0);
            currentCard.position = nextCard.position;
            currentCard.SetParent(spawnPoints[i]);
            currentCard.GetComponent<cardManager>().positionInHand = i;
            StartCoroutine(MoveCard(currentCard, spawnPoints[i].position, 0.2f));
        }
        Destroy(spawnPoints[ocupiedSpots - 1].GetChild(0).gameObject);

        ocupiedSpots--;
    }

    public void SpawnCard()
    {
        if (ocupiedSpots >= spawnPoints.Length) return;

        int score = Object.FindFirstObjectByType<claimGenerator>().score;

        // Unlock all new cards based on score
        foreach (var CurrentCard in cardPrefabs)
        {
            if (!CurrentCard.isUnlocked && score >= CurrentCard.unlockScore)
            {
                CurrentCard.isUnlocked = true;
                Debug.Log($"CurrentCard unlocked: {CurrentCard.prefab.name}");

                CurrentCard.unlockScreen.SetActive(true);
            }
        }
        
        var availableCards = System.Array.FindAll(cardPrefabs, c => c.unlockScore <= score);
        if (availableCards.Length == 0) return;

        var cardToSpawn = availableCards[Random.Range(0, availableCards.Length)].prefab;

        Transform spawnPoint;
        spawnPoint = spawnPoints[ocupiedSpots];

        // Spawn at deck position then move to hand position

        GameObject card = Instantiate(cardToSpawn, deckPosition.position, Quaternion.identity, spawnPoint);
        card.transform.SetParent(spawnPoint);
        // lerp movement to spawn point
        StartCoroutine(MoveCard(card.transform, spawnPoint.position, 0.5f));

        // set zero position relative to parent
        card.transform.localPosition = Vector3.zero;

        card.GetComponent<cardManager>().positionInHand = ocupiedSpots;
        ocupiedSpots++;
    }
    System.Collections.IEnumerator MoveCard(Transform card, Vector3 to, float duration)
    {
        Vector3 from = card.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            card.position = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        card.position = to;
    }
}
