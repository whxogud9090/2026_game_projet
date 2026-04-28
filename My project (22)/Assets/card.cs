using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Card : MonoBehaviour, IPointerClickHandler
{
    public float rotateY = 5f;
    public Component text;
    public int number;
    public string displayText;
    public CardGame cardGame;
    public bool isFront;
    public bool isMatched;
    public float flipDuration = 0.25f;

    [SerializeField] private Image image;
    [SerializeField] private Image symbolImage;
    [SerializeField] private Button button;
    [SerializeField] private Color frontColor = new Color(1f, 0.88f, 0.18f, 1f);
    [SerializeField] private Color matchedColor = new Color(0.18f, 0.78f, 0.36f, 1f);

    private CardGame game;
    private Sprite faceSprite;
    private Sprite backSprite;
    private Color backColor;
    private Coroutine flipRoutine;

    private void Awake()
    {
        CacheReferences();
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }
    }

    private void OnValidate()
    {
        CacheReferences();
    }

    public void Setup(CardGame owner, int cardNumber, Sprite frontSprite, Sprite cardBackSprite, Color cardBackColor)
    {
        Setup(owner, cardNumber, (cardNumber + 1).ToString(), frontSprite, cardBackSprite, cardBackColor);
    }

    public void Setup(CardGame owner, int cardNumber, string frontText, Sprite frontSprite, Sprite cardBackSprite, Color cardBackColor)
    {
        game = owner;
        cardGame = owner;
        number = cardNumber;
        displayText = string.IsNullOrEmpty(frontText) ? (cardNumber + 1).ToString() : frontText;
        faceSprite = frontSprite;
        backSprite = cardBackSprite;
        backColor = cardBackColor;
        PrepareFrontImage();
        ResetState();
    }

    public void ResetState()
    {
        isFront = false;
        isMatched = false;
        CacheReferences();

        if (button != null)
        {
            button.interactable = true;
        }

        if (flipRoutine != null)
        {
            StopCoroutine(flipRoutine);
            flipRoutine = null;
        }

        transform.localRotation = Quaternion.identity;
        ShowBack();
    }

    public void PreviewFront(int cardNumber, string frontText, Sprite frontSprite)
    {
        number = cardNumber;
        displayText = string.IsNullOrEmpty(frontText) ? (cardNumber + 1).ToString() : frontText;
        faceSprite = frontSprite;
        PrepareFrontImage();
        transform.localRotation = Quaternion.identity;
        ShowFront();
    }

    public void ClickCard()
    {
        HandleClick();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        HandleClick();
    }

    public void Flip(bool showFront)
    {
        if (isMatched)
        {
            return;
        }

        if (flipRoutine != null)
        {
            StopCoroutine(flipRoutine);
        }

        flipRoutine = StartCoroutine(FlipRoutine(showFront));
    }

    public void SetMatched()
    {
        isMatched = true;
        isFront = true;

        if (button != null)
        {
            button.interactable = false;
        }

        if (flipRoutine != null)
        {
            StopCoroutine(flipRoutine);
            flipRoutine = null;
        }

        transform.localRotation = Quaternion.identity;
        ShowFront();

        if (image != null && faceSprite == null)
        {
            image.color = matchedColor;
        }
    }

    private void HandleClick()
    {
        CardGame owner = game != null ? game : cardGame;
        if (owner != null)
        {
            owner.OnClickCard(this);
        }
    }

    private System.Collections.IEnumerator FlipRoutine(bool showFront)
    {
        float halfDuration = Mathf.Max(0.01f, flipDuration * 0.5f);

        yield return RotateCard(0f, 90f, halfDuration);

        isFront = showFront;
        if (isFront)
        {
            ShowFront();
        }
        else
        {
            ShowBack();
        }

        yield return RotateCard(90f, 0f, halfDuration);

        transform.localRotation = Quaternion.identity;
        flipRoutine = null;
    }

    private System.Collections.IEnumerator RotateCard(float startY, float endY, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            float y = Mathf.Lerp(startY, endY, eased);
            transform.localRotation = Quaternion.Euler(0f, y, 0f);
            yield return null;
        }
    }

    private void ShowFront()
    {
        CacheReferences();

        if (image == null)
        {
            return;
        }

        image.color = frontColor;

        if (faceSprite != null)
        {
            image.sprite = null;
            SetText(string.Empty);
            SetTextVisible(false);
            ShowSymbol(faceSprite);
            return;
        }

        HideSymbol();
        SetTextVisible(true);
        SetText(string.IsNullOrEmpty(displayText) ? (number + 1).ToString() : displayText);
    }

    private void ShowBack()
    {
        CacheReferences();
        SetText(string.Empty);
        SetTextVisible(false);

        if (image == null)
        {
            return;
        }

        image.sprite = backSprite;
        image.color = backSprite != null ? Color.white : backColor;
        HideSymbol();
    }

    private void CacheReferences()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (text == null)
        {
            text = FindTextComponent();
        }

        if (symbolImage == null)
        {
            symbolImage = FindSymbolImage();
        }
    }

    private Image FindSymbolImage()
    {
        Transform child = transform.Find("SymbolImage");
        if (child != null)
        {
            return child.GetComponent<Image>();
        }

        return null;
    }

    private Image GetOrCreateSymbolImage()
    {
        if (symbolImage != null)
        {
            return symbolImage;
        }

        GameObject symbolObject = new GameObject("SymbolImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        symbolObject.transform.SetParent(transform, false);

        RectTransform rectTransform = symbolObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(78f, 78f);

        symbolImage = symbolObject.GetComponent<Image>();
        symbolImage.raycastTarget = false;
        symbolImage.preserveAspect = true;
        return symbolImage;
    }

    private void PrepareFrontImage()
    {
        CacheReferences();
        if (faceSprite == null)
        {
            return;
        }

        Image target = GetOrCreateSymbolImage();
        target.sprite = faceSprite;
        target.color = Color.white;
        target.gameObject.SetActive(false);
    }

    private void ShowSymbol(Sprite sprite)
    {
        Image target = GetOrCreateSymbolImage();
        target.sprite = sprite;
        target.color = Color.white;
        target.gameObject.SetActive(true);
    }

    private void HideSymbol()
    {
        if (symbolImage != null)
        {
            symbolImage.gameObject.SetActive(false);
        }
    }

    private Component FindTextComponent()
    {
        Component[] components = GetComponentsInChildren<Component>(true);
        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];
            if (component == null)
            {
                continue;
            }

            string typeName = component.GetType().Name;
            if (typeName == "Text" || typeName == "TextMeshProUGUI" || typeName == "TMP_Text")
            {
                return component;
            }
        }

        return null;
    }

    private void SetText(string value)
    {
        if (text == null)
        {
            text = FindTextComponent();
        }

        if (text == null)
        {
            return;
        }

        System.Reflection.PropertyInfo property = text.GetType().GetProperty("text");
        if (property != null && property.CanWrite)
        {
            property.SetValue(text, value);
        }
    }

    private void SetTextVisible(bool visible)
    {
        Component[] components = GetComponentsInChildren<Component>(true);
        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];
            if (component == null)
            {
                continue;
            }

            string typeName = component.GetType().Name;
            if (typeName == "Text" || typeName == "TextMeshProUGUI" || typeName == "TMP_Text")
            {
                component.gameObject.SetActive(visible);
            }
        }
    }
}
