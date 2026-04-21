using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{

    public float rotateY;
    public TextMeshProUGUI text;
    public bool isFront = true;
    private Quaternion flipRotation = Quaternion.Euler(0, 180f, 0);
    private Quaternion originRotation = Quaternion.Euler(0, 0, 0);
    public int number;
    public CardGame cardGame;
    public bool isMatched = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    //&& = and || = or
    // 0 => 180 => -180 => 0

    //x<180
    //
    void Update()
    {
        
        float currentY = transform.eulerAngles.y;

        if (isFront)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, originRotation, rotateY * Time.deltaTime);
        }

        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, flipRotation, rotateY * Time.deltaTime);

        }
            /*if (currentY < 180 && currentY >= 0)
            {
                transform.Rotate(0, rotateY, 0);
            }
            else if (isClick)
            {
                transform.Rotate(0, rotateY, 0);
            }*/
        }
    public void ClickCard()
    {
        if (isMatched)
        {

        }
        else
        {
            cardGame.OnClickCard(this);
        }
    }


    public void Flip(bool isFront)
    {
        this.isFront = isFront;
    }

    public void SetCardNumber(int newNumber)
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        number = newNumber;
        text.text = number.ToString();
    }

    public void ChangeColor(Color newColor)
    {
        GetComponent<Image>().color = newColor;
    }

    public void SetImage(Sprite sprite)
    {
        GetComponent<Image>().sprite = sprite;
    }


}
