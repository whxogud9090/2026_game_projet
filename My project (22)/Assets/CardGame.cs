using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

public class CardGame : MonoBehaviour
{

    public List<Sprite> sprites = new List<Sprite>();
    public List<Card> cards = new List<Card>();
    private Card firstCard = null;
    private Card SecondCard = null;
    private bool isChecking = false;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startGame();
    }

    //카드 섞고 다 뒤집기
    void startGame()
    {
        List<int> pairNumbers = GeneratePairNumbers(cards.Count);
        for (int i = 0; i < pairNumbers.Count; ++i)
        {
            cards[i].SetCardNumber(pairNumbers[i]);

            cards[i].SetImage(sprites[pairNumbers[i]]);
        }

        //뒤집기

        for (int i = 0; i < pairNumbers.Count; ++i)
        {
            cards[i].isFront = false;
        }
    }

    //카드 맞는지 체크
    void CheckCard()
    {
        isChecking = true;

        if(firstCard.number == SecondCard.number)
        {
            //정답.클릭 안되게;
            //카드 맞음
            firstCard. ChangeColor(Color.red);
            SecondCard.ChangeColor(Color.red);

            firstCard.isMatched = true;
            SecondCard.isMatched =true;

            firstCard = null;
            SecondCard = null;

            isChecking = false;
        }
        else
        {
            Invoke("HideCard", 1.0f);
            //카드 뒤집기
            
        }
    }

    public void OnClickCard(Card card)
    {
        if (isChecking)
        {
            return;
        }

        if (firstCard == null)
        {
            firstCard = card;
            firstCard.Flip(true);
        }
        else
        {
            SecondCard = card;
            SecondCard.Flip(true);
        }
        if (firstCard != null && SecondCard != null)
        {
            CheckCard();
        }
    }

    //다시 뒤집기
    void HideCard()
    {
        firstCard.isFront = false;
        SecondCard.isFront = false;

        firstCard.Flip(false);
        SecondCard.Flip(false);

        firstCard = null;
        SecondCard = null;

        isChecking = false;
    }

    //페어 넘버의 알고리즘
    List<int> GeneratePairNumbers(int cardCount)
    {
        //8
        int pairCount = cardCount / 2;
        List<int> newCardNumbers = new List<int>();


        for (int i = 0; i < pairCount; ++i)
        {
            newCardNumbers.Add(i);
            newCardNumbers.Add(i);
        }

        //셔플
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
