using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGame : MonoBehaviour
{
    [Header("Board Setup")]
    [Min(1)] public int pairCount = 4;
    public List<Sprite> sprites = new List<Sprite>();
    public Sprite backSprite;
    public Color backColor = new Color(0.18f, 0.61f, 0.82f, 1f);
    public Card cardPrefab;
    public RectTransform cardRoot;

    [Header("Layout")]
    public Vector2 boardSize = new Vector2(720f, 360f);
    public Vector2 cellSize = new Vector2(100f, 100f);
    public Vector2 spacing = new Vector2(18f, 18f);

    [Header("Runtime")]
    public List<Card> cards = new List<Card>();

    private Card firstCard;
    private Card secondCard;
    private bool isChecking;

    private void Start()
    {
        SetupBoard();
    }

    private void OnValidate()
    {
        pairCount = Mathf.Max(1, pairCount);
    }

    [ContextMenu("Setup Board")]
    public void SetupBoard()
    {
        pairCount = Mathf.Max(1, pairCount);

        PrepareReferences();
        RemoveOldCards();
        CreateCards();
        GiveCardsNumbers();
    }

    // 1. Pair count만큼 숫자를 2개씩 만들고 섞은 뒤 카드에 넣는다.
    private void GiveCardsNumbers()
    {
        firstCard = null;
        secondCard = null;
        isChecking = false;

        List<int> pairNumbers = GeneratePairNumbers(pairCount * 2);
        for (int i = 0; i < cards.Count; i++)
        {
            int number = pairNumbers[i];
            Sprite faceSprite = number < sprites.Count ? sprites[number] : null;
            cards[i].Setup(this, number, faceSprite, backSprite, backColor);
        }
    }

    public void OnClickCard(Card card)
    {
        if (isChecking || card == null || card.isMatched || card.isFront)
        {
            return;
        }

        if (firstCard == null)
        {
            firstCard = card;
            firstCard.Flip(true);
            return;
        }

        if (firstCard == card)
        {
            return;
        }

        secondCard = card;
        secondCard.Flip(true);
        StartCoroutine(CheckCardRoutine());
    }

    // 2. 두 장을 열었으면 같은 카드인지 확인한다.
    private IEnumerator CheckCardRoutine()
    {
        isChecking = true;
        yield return new WaitForSeconds(0.5f);

        if (firstCard.number == secondCard.number)
        {
            firstCard.SetMatched();
            secondCard.SetMatched();
        }
        else
        {
            firstCard.Flip(false);
            secondCard.Flip(false);
        }

        firstCard = null;
        secondCard = null;
        isChecking = false;
    }

    private void PrepareReferences()
    {
        if (cardPrefab == null && cards.Count > 0)
        {
            cardPrefab = cards[0];
        }

        if (cardRoot == null && cardPrefab != null)
        {
            cardRoot = cardPrefab.transform.parent as RectTransform;
        }

        if (cardRoot == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            GameObject boardObject = new GameObject("CardBoard", typeof(RectTransform), typeof(GridLayoutGroup));
            boardObject.transform.SetParent(canvas.transform, false);
            cardRoot = boardObject.GetComponent<RectTransform>();
            cardRoot.anchorMin = new Vector2(0.5f, 0.5f);
            cardRoot.anchorMax = new Vector2(0.5f, 0.5f);
            cardRoot.pivot = new Vector2(0.5f, 0.5f);
            cardRoot.anchoredPosition = new Vector2(0f, -40f);
            cardRoot.sizeDelta = boardSize;
        }
    }

    // 3. 이전에 있던 카드들은 템플릿 1장을 제외하고 정리한다.
    private void RemoveOldCards()
    {
        if (cardPrefab == null)
        {
            return;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == null || cards[i] == cardPrefab)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(cards[i].gameObject);
            }
            else
            {
                DestroyImmediate(cards[i].gameObject);
            }
        }

        cardPrefab.gameObject.SetActive(false);
        cards.Clear();
    }

    // 4. pairCount에 맞게 카드들을 새로 만든다.
    private void CreateCards()
    {
        if (cardPrefab == null || cardRoot == null)
        {
            return;
        }

        GridLayoutGroup grid = cardRoot.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = cardRoot.gameObject.AddComponent<GridLayoutGroup>();
        }

        int totalCards = pairCount * 2;
        int columns = Mathf.CeilToInt(Mathf.Sqrt(totalCards));
        int rows = Mathf.CeilToInt(totalCards / (float)columns);

        float width = (columns * cellSize.x) + ((columns - 1) * spacing.x) + 40f;
        float height = (rows * cellSize.y) + ((rows - 1) * spacing.y) + 40f;
        cardRoot.sizeDelta = new Vector2(Mathf.Max(boardSize.x, width), Mathf.Max(boardSize.y, height));

        grid.cellSize = cellSize;
        grid.spacing = spacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.padding = new RectOffset(20, 20, 20, 20);

        for (int i = 0; i < totalCards; i++)
        {
            Card newCard = Instantiate(cardPrefab, cardRoot);
            newCard.gameObject.name = $"Card_{i + 1}";
            newCard.gameObject.SetActive(true);
            newCard.transform.localScale = Vector3.one;
            newCard.ResetState();
            cards.Add(newCard);
        }
    }

    private List<int> GeneratePairNumbers(int cardCount)
    {
        int localPairCount = cardCount / 2;
        List<int> newCardNumbers = new List<int>(cardCount);

        for (int i = 0; i < localPairCount; i++)
        {
            newCardNumbers.Add(i);
            newCardNumbers.Add(i);
        }

        for (int i = newCardNumbers.Count - 1; i > 0; i--)
        {
            int temp = newCardNumbers[i];
            int rnd = Random.Range(0, i + 1);
            newCardNumbers[i] = newCardNumbers[rnd];
            newCardNumbers[rnd] = temp;
        }

        return newCardNumbers;
    }
}
