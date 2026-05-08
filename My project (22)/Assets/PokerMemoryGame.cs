using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PokerMemoryGame : MonoBehaviour
{
    [Header("Card Image Assets")]
    public Sprite cardBack;
    public List<Sprite> cardFaces = new List<Sprite>();

    [Header("Rules")]
    [Min(1)] public int pairCount = 8;
    public float mismatchDelay = 0.8f;

    private readonly List<PokerCardView> cards = new List<PokerCardView>();
    private readonly List<Sprite> runtimeCardFaces = new List<Sprite>();
    private PokerCardView firstCard;
    private PokerCardView secondCard;
    private bool isChecking;
    private int attempts;
    private int matchedPairs;

    private Canvas canvas;
    private RectTransform boardRoot;
    private GridLayoutGroup cardGrid;
    private Text attemptsText;
    private Text matchedText;
    private Text resultText;

    private void Start()
    {
        BuildUi();
        StartGame();
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        TrySelectCardAt(Input.mousePosition);
    }

    public void StartGame()
    {
        StopAllCoroutines();
        ClearBoard();

        firstCard = null;
        secondCard = null;
        isChecking = false;
        attempts = 0;
        matchedPairs = 0;

        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        LoadPokerCardFaces();
        List<int> deck = CreateDeck();
        ConfigureGrid(deck.Count);

        for (int i = 0; i < deck.Count; i++)
        {
            cards.Add(CreateCard(i, deck[i]));
        }

        UpdateHud();
    }

    public void SelectCard(PokerCardView card)
    {
        if (isChecking || card == null || card.IsFront || card.IsMatched)
        {
            return;
        }

        card.Flip(true);

        if (firstCard == null)
        {
            firstCard = card;
            return;
        }

        if (firstCard == card)
        {
            return;
        }

        secondCard = card;
        attempts++;
        UpdateHud();
        StartCoroutine(CheckCards());
    }

    private void TrySelectCardAt(Vector2 screenPosition)
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            PokerCardView card = cards[i];
            if (card == null || card.IsFront || card.IsMatched)
            {
                continue;
            }

            RectTransform rect = card.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, null))
            {
                SelectCard(card);
                return;
            }
        }
    }

    private IEnumerator CheckCards()
    {
        isChecking = true;
        yield return new WaitForSeconds(mismatchDelay);

        if (firstCard.CardId == secondCard.CardId)
        {
            firstCard.SetMatched();
            secondCard.SetMatched();
            matchedPairs++;

            if (matchedPairs >= pairCount)
            {
                ShowClear();
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
        UpdateHud();
    }

    private void BuildUi()
    {
        EnsureCamera();

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
        else
        {
            EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
        }

        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        CreatePanel(canvasRect, "TableBackground", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color32(20, 96, 58, 255));
        attemptsText = CreateText(canvasRect, "AttemptsText", "", new Vector2(-360f, -44f), new Vector2(360f, 44f), 28, font, Color.white);
        matchedText = CreateText(canvasRect, "MatchedText", "", new Vector2(360f, -44f), new Vector2(360f, 44f), 28, font, Color.white);
        resultText = CreateText(canvasRect, "ResultText", "", new Vector2(0f, -92f), new Vector2(820f, 54f), 30, font, new Color32(255, 224, 110, 255));
        resultText.gameObject.SetActive(false);

        Button restartButton = CreateButton(canvasRect, "RestartButton", "다시 시작", new Vector2(0f, 52f), new Vector2(220f, 56f), font);
        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(StartGame);

        GameObject boardObject = new GameObject("CardBoard", typeof(RectTransform), typeof(GridLayoutGroup));
        boardObject.transform.SetParent(canvasRect, false);
        boardRoot = boardObject.GetComponent<RectTransform>();
        boardRoot.anchorMin = new Vector2(0.5f, 0.5f);
        boardRoot.anchorMax = new Vector2(0.5f, 0.5f);
        boardRoot.pivot = new Vector2(0.5f, 0.5f);
        boardRoot.anchoredPosition = new Vector2(0f, -80f);
        boardRoot.sizeDelta = new Vector2(1180f, 780f);

        cardGrid = boardObject.GetComponent<GridLayoutGroup>();
        cardGrid.cellSize = new Vector2(130f, 178f);
        cardGrid.spacing = new Vector2(20f, 20f);
        cardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        cardGrid.constraintCount = 4;
        cardGrid.childAlignment = TextAnchor.MiddleCenter;
    }

    private void EnsureCamera()
    {
        if (Camera.main != null || FindFirstObjectByType<Camera>() != null)
        {
            return;
        }

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(20, 96, 58, 255);
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }

    private PokerCardView CreateCard(int index, int cardId)
    {
        GameObject cardObject = new GameObject("PokerCard_" + (index + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(PokerCardView));
        cardObject.transform.SetParent(boardRoot, false);

        Sprite face = GetFaceSprite(cardId);
        PokerCardView card = cardObject.GetComponent<PokerCardView>();
        card.Setup(this, cardId, cardBack, face);
        return card;
    }

    private List<int> CreateDeck()
    {
        pairCount = Mathf.Max(1, pairCount);
        List<int> deck = new List<int>();

        for (int i = 0; i < pairCount; i++)
        {
            deck.Add(i);
            deck.Add(i);
        }

        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }

        return deck;
    }

    private Sprite GetFaceSprite(int cardId)
    {
        if (runtimeCardFaces.Count == 0)
        {
            return null;
        }

        return runtimeCardFaces[cardId % runtimeCardFaces.Count];
    }

    private void LoadPokerCardFaces()
    {
        runtimeCardFaces.Clear();
        Sprite[] resourceFaces = Resources.LoadAll<Sprite>("PokerCardImages");

        if (resourceFaces.Length > 0)
        {
            List<Sprite> sortedFaces = new List<Sprite>(resourceFaces);
            sortedFaces.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
            runtimeCardFaces.AddRange(sortedFaces);
            return;
        }

        Texture2D[] cardTextures = Resources.LoadAll<Texture2D>("PokerCardImages");
        if (cardTextures.Length > 0)
        {
            List<Texture2D> sortedTextures = new List<Texture2D>(cardTextures);
            sortedTextures.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

            for (int i = 0; i < sortedTextures.Count; i++)
            {
                Texture2D texture = sortedTextures[i];
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
                sprite.name = texture.name;
                runtimeCardFaces.Add(sprite);
            }

            return;
        }

        runtimeCardFaces.AddRange(cardFaces);
    }

    private void ConfigureGrid(int cardCount)
    {
        if (cardGrid == null || boardRoot == null)
        {
            return;
        }

        int columns = Mathf.CeilToInt(Mathf.Sqrt(cardCount * 1.2f));
        int rows = Mathf.CeilToInt((float)cardCount / columns);
        Vector2 boardSize = boardRoot.sizeDelta;
        Vector2 spacing = cardCount > 40 ? new Vector2(8f, 8f) : new Vector2(20f, 20f);

        float availableWidth = boardSize.x - spacing.x * (columns - 1);
        float availableHeight = boardSize.y - spacing.y * (rows - 1);
        float width = availableWidth / columns;
        float height = availableHeight / rows;
        float cardHeight = Mathf.Min(height, width * 1.38f);
        float cardWidth = cardHeight / 1.38f;

        cardGrid.constraintCount = columns;
        cardGrid.spacing = spacing;
        cardGrid.cellSize = new Vector2(cardWidth, cardHeight);
    }

    private void ClearBoard()
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            if (cards[i] != null)
            {
                Destroy(cards[i].gameObject);
            }
        }

        cards.Clear();
    }

    private void UpdateHud()
    {
        if (attemptsText != null)
        {
            attemptsText.text = "시도 횟수: " + attempts;
        }

        if (matchedText != null)
        {
            matchedText.text = "맞춘 카드: " + matchedPairs + " / " + pairCount;
        }
    }

    private void ShowClear()
    {
        if (resultText != null)
        {
            resultText.text = "클리어! 총 " + attempts + "번 만에 모든 짝을 맞췄습니다.";
            resultText.gameObject.SetActive(true);
        }
    }

    private RectTransform CreatePanel(RectTransform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Color color)
    {
        GameObject panelObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(parent, false);

        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        if (anchorMin == Vector2.zero && anchorMax == Vector2.one)
        {
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        Image image = panelObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private Text CreateText(RectTransform parent, string objectName, string content, Vector2 position, Vector2 size, int fontSize, Font font, Color color)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private Button CreateButton(RectTransform parent, string objectName, string label, Vector2 position, Vector2 size, Font font)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color32(235, 210, 135, 255);

        Button button = buttonObject.GetComponent<Button>();
        Text text = CreateText(rect, "Text", label, Vector2.zero, size, 24, font, new Color32(30, 32, 38, 255));
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        return button;
    }
}
