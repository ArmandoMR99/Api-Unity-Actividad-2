using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class ApiManager : MonoBehaviour
{
    [Header("API URL")]
    public string apiUrl = "https://my-json-server.typicode.com/ArmandoMR99/Api-Unity/users";

    [Header("UI References")]
    public GameObject cardPrefab;
    public Transform contentParent;
    public TextMeshProUGUI userNameText;
    private User[] allUsers;
    private int currentUserIndex = 0;

    void Start()
    {
        StartCoroutine(GetUsers());
    }

    // ==========================
    // OBTENER USUARIOS
    // ==========================
    IEnumerator GetUsers()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            string json = request.downloadHandler.text;
            allUsers = JsonHelper.FromJson<User>(json);

            if (allUsers.Length > 0)
            {
                LoadUser(currentUserIndex);
            }

            if (allUsers.Length > 0)
            {
                userNameText.text = allUsers[0].name;
                StartCoroutine(LoadUserCards(allUsers[0].cards));
            }
        }
    }

    // ==========================
    // CARGAR CARTAS
    // ==========================
    IEnumerator LoadUserCards(int[] cardIds)
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (int id in cardIds)
        {
            yield return StartCoroutine(GetCardFromAPI((card) =>
            {
                CreateCardUI(card);
            }));
        }
    }

    // ==========================
    // API EXTERNA
    // ==========================
    IEnumerator GetCardFromAPI(System.Action<Card> callback)
    {
        string url = "https://deckofcardsapi.com/api/deck/new/draw/?count=1";

        UnityWebRequest request = UnityWebRequest.Get(url); // 👈 GET normal, no Texture
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error API externa: " + request.error);
        }
        else
        {
            string json = request.downloadHandler.text;

            CardResponse response = JsonUtility.FromJson<CardResponse>(json);

            if (response.cards != null && response.cards.Length > 0)
            {
                callback(response.cards[0]);
            }
        }
    }

    // ==========================
    // CREAR UI
    // ==========================
    void CreateCardUI(Card card)
    {
        GameObject newCard = Instantiate(cardPrefab, contentParent);
        CardUI cardUI = newCard.GetComponent<CardUI>();

        StartCoroutine(LoadCardImage(card.image, cardUI.cardImage));
    }

    IEnumerator LoadCardImage(string imageUrl, Image imageComponent)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error cargando imagen: " + request.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            imageComponent.sprite = sprite;
        }
    }

    public void LoadUser(int index)
    {
        if (index < 0 || index >= allUsers.Length) return;

        currentUserIndex = index;

        userNameText.text = allUsers[index].name;

        StartCoroutine(LoadUserCards(allUsers[index].cards));
    }

    public void NextUser()
    {
        currentUserIndex++;

        if (currentUserIndex >= allUsers.Length)
            currentUserIndex = 0;

        LoadUser(currentUserIndex);
    }
}