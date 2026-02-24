[System.Serializable]
public class CardResponse
{
    public Card[] cards;
}

[System.Serializable]
public class Card
{
    public string value;
    public string suit;
    public string image;
}