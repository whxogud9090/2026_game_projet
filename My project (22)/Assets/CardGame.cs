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
    public Color backColor = new Color(0.91f, 0.44f, 0.32f, 1f);
    public bool useSymbols;
    public string[] symbols =
    {
        "Circle", "Triangle", "Square", "Diamond", "Plus",
        "X", "Ring", "Pentagon", "Hexagon", "Star"
    };
    public Card cardPrefab;
    public RectTransform cardRoot;

    [Header("Layout")]
    public Vector2 boardSize = new Vector2(720f, 360f);
    public Vector2 cellSize = new Vector2(100f, 100f);
    public Vector2 spacing = new Vector2(18f, 18f);

    [Header("Runtime")]
    public List<Card> cards = new List<Card>();

    [Header("UI")]
    public Text attemptText;
    public Text clearMessageText;
    public Button restartButton;

    private Card firstCard;
    private Card secondCard;
    private bool isChecking;
    private int attemptCount;
    private int matchedPairCount;
    private readonly Dictionary<int, Sprite> symbolSprites = new Dictionary<int, Sprite>();

    private void Start()
    {
        SetupBoard();
    }

    private void OnValidate()
    {
        pairCount = Mathf.Max(1, pairCount);

        if (!Application.isPlaying)
        {
            PreviewCardsInEditor();
        }
    }

    public void RestartGame()
    {
        StopAllCoroutines();
        SetupBoard();
    }

    [ContextMenu("Setup Board")]
    public void SetupBoard()
    {
        pairCount = Mathf.Max(1, pairCount);

        PrepareReferences();
        RemoveOldCards();
        CreateCards();
        GiveCardsNumbers();
        UpdateStatusText();
    }

    private void GiveCardsNumbers()
    {
        firstCard = null;
        secondCard = null;
        isChecking = false;
        attemptCount = 0;
        matchedPairCount = 0;

        if (clearMessageText != null)
        {
            clearMessageText.gameObject.SetActive(false);
        }

        List<int> pairNumbers = GeneratePairNumbers(pairCount * 2);
        for (int i = 0; i < cards.Count; i++)
        {
            int number = pairNumbers[i];
            Sprite faceSprite = GetFaceSprite(number);
            cards[i].Setup(this, number, GetDisplayText(number), faceSprite, backSprite, backColor);
        }
    }

    private Sprite GetFaceSprite(int number)
    {
        if (useSymbols)
        {
            return GetSymbolSprite(number);
        }

        return number < sprites.Count ? sprites[number] : null;
    }

    private void PreviewCardsInEditor()
    {
        if (cards == null)
        {
            return;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            if (card == null)
            {
                continue;
            }

            int number = pairCount > 0 ? i % pairCount : i;
            card.PreviewFront(number, GetDisplayText(number), GetFaceSprite(number));
        }
    }

    private string GetDisplayText(int number)
    {
        if (useSymbols && symbols != null && number >= 0 && number < symbols.Length && !string.IsNullOrEmpty(symbols[number]))
        {
            return symbols[number];
        }

        return (number + 1).ToString();
    }

    private Sprite GetSymbolSprite(int number)
    {
        if (symbolSprites.TryGetValue(number, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        Sprite sprite = CreateSymbolSprite(number);
        symbolSprites[number] = sprite;
        return sprite;
    }

    private Sprite CreateSymbolSprite(int number)
    {
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color draw = Color.white;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = ((x + 0.5f) / size * 2f) - 1f;
                float ny = ((y + 0.5f) / size * 2f) - 1f;
                texture.SetPixel(x, y, IsInsideSymbol(number, nx, ny) ? draw : clear);
            }
        }

        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private bool IsInsideSymbol(int number, float x, float y)
    {
        float ax = Mathf.Abs(x);
        float ay = Mathf.Abs(y);
        float radius = Mathf.Sqrt((x * x) + (y * y));

        switch (number % 10)
        {
            case 0:
                return radius < 0.5f;
            case 1:
                return IsInsidePolygon(x, y, 3, 0.62f, -90f);
            case 2:
                return ax < 0.46f && ay < 0.46f;
            case 3:
                return ax + ay < 0.64f;
            case 4:
                return (ax < 0.14f && ay < 0.52f) || (ay < 0.14f && ax < 0.52f);
            case 5:
                return (Mathf.Abs(x - y) < 0.12f || Mathf.Abs(x + y) < 0.12f) && ax < 0.48f && ay < 0.48f;
            case 6:
                return radius > 0.28f && radius < 0.52f;
            case 7:
                return IsInsidePolygon(x, y, 5, 0.58f, -90f);
            case 8:
                return IsInsidePolygon(x, y, 6, 0.58f, 30f);
            default:
                return IsInsideStar(x, y);
        }
    }

    private bool IsInsidePolygon(float x, float y, int sides, float outerRadius, float rotationDegrees)
    {
        float rotation = rotationDegrees * Mathf.Deg2Rad;
        bool inside = false;

        for (int i = 0, j = sides - 1; i < sides; j = i++)
        {
            float angleI = rotation + (Mathf.PI * 2f * i / sides);
            float angleJ = rotation + (Mathf.PI * 2f * j / sides);
            float xi = Mathf.Cos(angleI) * outerRadius;
            float yi = Mathf.Sin(angleI) * outerRadius;
            float xj = Mathf.Cos(angleJ) * outerRadius;
            float yj = Mathf.Sin(angleJ) * outerRadius;

            bool crosses = (yi > y) != (yj > y);
            if (crosses && x < ((xj - xi) * (y - yi) / (yj - yi)) + xi)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private bool IsInsideStar(float x, float y)
    {
        const int points = 10;
        float rotation = -90f * Mathf.Deg2Rad;
        bool inside = false;

        for (int i = 0, j = points - 1; i < points; j = i++)
        {
            float radiusI = i % 2 == 0 ? 0.58f : 0.26f;
            float radiusJ = j % 2 == 0 ? 0.58f : 0.26f;
            float angleI = rotation + (Mathf.PI * 2f * i / points);
            float angleJ = rotation + (Mathf.PI * 2f * j / points);
            float xi = Mathf.Cos(angleI) * radiusI;
            float yi = Mathf.Sin(angleI) * radiusI;
            float xj = Mathf.Cos(angleJ) * radiusJ;
            float yj = Mathf.Sin(angleJ) * radiusJ;

            bool crosses = (yi > y) != (yj > y);
            if (crosses && x < ((xj - xi) * (y - yi) / (yj - yi)) + xi)
            {
                inside = !inside;
            }
        }

        return inside;
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
        attemptCount++;
        UpdateStatusText();
        StartCoroutine(CheckCardRoutine());
    }

    private IEnumerator CheckCardRoutine()
    {
        isChecking = true;
        yield return new WaitForSeconds(0.5f);

        if (firstCard.number == secondCard.number)
        {
            firstCard.SetMatched();
            secondCard.SetMatched();
            matchedPairCount++;

            if (matchedPairCount >= pairCount)
            {
                ShowClearMessage();
            }
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
            RectTransform parentRect = cardPrefab.transform.parent as RectTransform;
            if (parentRect != null && parentRect.GetComponent<Canvas>() == null)
            {
                cardRoot = parentRect;
            }
        }

        if (cardRoot == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("CardGame needs a Canvas in the scene before it can create the card board.", this);
                return;
            }

            Transform existingBoard = canvas.transform.Find("CardBoard");
            GameObject boardObject = existingBoard != null
                ? existingBoard.gameObject
                : new GameObject("CardBoard", typeof(RectTransform), typeof(GridLayoutGroup));

            boardObject.transform.SetParent(canvas.transform, false);
            cardRoot = boardObject.GetComponent<RectTransform>();
            cardRoot.anchorMin = new Vector2(0.5f, 0.5f);
            cardRoot.anchorMax = new Vector2(0.5f, 0.5f);
            cardRoot.pivot = new Vector2(0.5f, 0.5f);
            cardRoot.anchoredPosition = new Vector2(0f, -70f);
            cardRoot.sizeDelta = boardSize;
        }

        PrepareUiTexts();
    }

    private void PrepareUiTexts()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (attemptText == null)
        {
            attemptText = CreateText(canvas.transform, "AttemptText", new Vector2(0f, 260f), 32, TextAnchor.MiddleCenter);
            attemptText.font = font;
            attemptText.color = Color.white;
        }

        if (clearMessageText == null)
        {
            clearMessageText = CreateText(canvas.transform, "ClearMessageText", new Vector2(0f, 205f), 42, TextAnchor.MiddleCenter);
            clearMessageText.font = font;
            clearMessageText.color = new Color(1f, 0.88f, 0.18f, 1f);
            clearMessageText.gameObject.SetActive(false);
        }

        if (restartButton == null)
        {
            restartButton = CreateRestartButton(canvas.transform, font);
        }

        restartButton.onClick.RemoveListener(RestartGame);
        restartButton.onClick.AddListener(RestartGame);
    }

    private Text CreateText(Transform parent, string objectName, Vector2 position, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(520f, 70f);

        Text text = textObject.GetComponent<Text>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.raycastTarget = false;

        return text;
    }

    private Button CreateRestartButton(Transform parent, Font font)
    {
        GameObject buttonObject = new GameObject("RestartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(330f, 260f);
        rectTransform.sizeDelta = new Vector2(180f, 48f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.18f, 0.46f, 0.62f, 1f);

        Button button = buttonObject.GetComponent<Button>();

        Text label = CreateText(buttonObject.transform, "Text", Vector2.zero, 24, TextAnchor.MiddleCenter);
        label.font = font;
        label.color = Color.white;
        label.text = "Restart";

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;
        labelRect.anchoredPosition = Vector2.zero;

        return button;
    }

    private void UpdateStatusText()
    {
        if (attemptText != null)
        {
            attemptText.text = $"\uC2DC\uB3C4 \uD69F\uC218: {attemptCount}";
        }
    }

    private void ShowClearMessage()
    {
        if (clearMessageText == null)
        {
            return;
        }

        clearMessageText.text = $"\uAC8C\uC784 \uD074\uB9AC\uC5B4! \uCD1D {attemptCount}\uBC88 \uC2DC\uB3C4";
        clearMessageText.gameObject.SetActive(true);
    }

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
