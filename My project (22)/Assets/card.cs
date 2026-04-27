using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public TextMeshProUGUI text;
    public bool isFront = true;
    public int number;
    public CardGame cardGame;
    public bool isMatched;

    private Image cardImage;
    private Button cardButton;
    private Sprite frontSprite;
    private Sprite hiddenSprite;
    private Color hiddenColor = Color.blue;

    private void Awake()
    {
        CacheComponents();
    }

    public void ClickCard()
    {
        if (!isMatched && cardGame != null)
        {
            cardGame.OnClickCard(this);
        }
    }

    public void Setup(CardGame owner, int newNumber, Sprite newFrontSprite, Sprite newBackSprite, Color newBackColor)
    {
        CacheComponents();

        cardGame = owner;
        number = newNumber;
        frontSprite = newFrontSprite;
        hiddenSprite = newBackSprite;
        hiddenColor = newBackColor;
        isMatched = false;

        if (cardButton != null)
        {
            cardButton.interactable = true;
        }

        Flip(false);
    }

    public void Flip(bool showFront)
    {
        isFront = showFront;
        RefreshVisual();
    }

    public void SetMatched()
    {
        isMatched = true;
        isFront = true;

        if (cardButton != null)
        {
            cardButton.interactable = false;
        }

        RefreshVisual();
    }

    // 새로 생성된 카드가 시작할 때 기본 상태로 돌아가게 한다.
    public void ResetState()
    {
        CacheComponents();
        isMatched = false;
        isFront = false;
        number = 0;
        if (cardButton != null)
        {
            cardButton.interactable = true;
        }
        RefreshVisual();
    }

    private void CacheComponents()
    {
        if (text == null)
        {
            text = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (cardImage == null)
        {
            cardImage = GetComponent<Image>();
        }

        if (cardButton == null)
        {
            cardButton = GetComponent<Button>();
        }
    }

    private void RefreshVisual()
    {
        CacheComponents();

        if (cardImage == null)
        {
            return;
        }

        if (isFront)
        {
            cardImage.sprite = frontSprite;
            cardImage.color = Color.white;

            if (text != null)
            {
                text.text = frontSprite == null ? (number + 1).ToString() : string.Empty;
            }
        }
        else
        {
            cardImage.sprite = hiddenSprite;
            cardImage.color = hiddenSprite == null ? hiddenColor : Color.white;

            if (text != null)
            {
                text.text = string.Empty;
            }
        }
    }
}
