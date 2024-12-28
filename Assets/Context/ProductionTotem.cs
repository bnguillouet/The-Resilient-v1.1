using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ProductionTotem : MonoBehaviour
{
    public GameObject productPrefab; // Prefab contenant les images 
    public float moveDistance = 300f; // Distance de déplacement en pixels
    public float duration = 2.5f; // Durée du déplacement et de la disparition
    public float delayBetweenTotems = 0.5f; // Délai entre les totems

    public static ProductionTotem Instance; 

    private Dictionary<Vector2Int, float> lastTotemTimes = new Dictionary<Vector2Int, float>();

    private void Awake()
    {
        // Assurez-vous qu'il n'y ait qu'une seule instance de GameSettings
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CreateProductTotem(string productName, Vector2Int position, int quantity, bool producted)
    {
        /*Debug.Log("coordonné iso" + position);
        Debug.Log("coordonné calc" + IsometricToUnityCoordinates(position));     */   
        // Créer un nouveau GameObject à partir du prefab
        productPrefab.SetActive(true);
        GameObject productTotem = Instantiate(productPrefab, IsometricToUnityCoordinates(position), Quaternion.identity);
        productPrefab.SetActive(false);
        productTotem.SetActive(true);
        productTotem.transform.SetParent(transform, false);

        // Assigner l'image correspondante au produit
        RawImage productImage = productTotem.GetComponent<RawImage>();
        productImage.texture = Resources.Load<Texture>($"Icons/Items/{productName}");
        if (!producted)
        {
            productImage.color = new Color(1.0f, 0.7f, 0.7f);
        }
       
        //Mettre à jour la quantité
        TextMeshProUGUI textMeshPro = productTotem.transform.Find("Quantity").GetComponent<TextMeshProUGUI>();
        if (textMeshPro != null)
        {
            if(quantity > 1)
            {
                textMeshPro.text = "x"+quantity;
            }
            else 
            {
                textMeshPro.text = "";
            }
        }

        // Lancer la coroutine pour déplacer et faire disparaître l'icône
        float delay = 0f;
        if (lastTotemTimes.ContainsKey(position))
        {
            delay = Mathf.Max(0, delayBetweenTotems - (Time.time - lastTotemTimes[position]));
        }
        lastTotemTimes[position] = Time.time + delay;
        StartCoroutine(MoveAndFade(productTotem, delay, producted));
    }

    private IEnumerator MoveAndFade(GameObject productTotem, float delay, bool producted)
    {
        yield return new WaitForSeconds(delay);

        RectTransform rectTransform = productTotem.GetComponent<RectTransform>();
        Vector3 startPosition = rectTransform.anchoredPosition;
        Vector3 endPosition = startPosition + new Vector3(0, producted ? moveDistance : -moveDistance, 0);
        RawImage productImage = productTotem.GetComponent<RawImage>();
        Color startColor = productImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Déplacer l'icône
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, endPosition, t);

            // Faire disparaître l'icône à partir de la moitié de la durée
            if (t >= 0.5f)
            {
                float fadeT = (t - 0.5f) / 0.5f; // Normaliser t pour la seconde moitié de la durée
                productImage.color = Color.Lerp(startColor, endColor, fadeT);
            }

            yield return null;
        }

        // Détruire le GameObject une fois le déplacement terminé
        Destroy(productTotem);
    }

    private Vector3 IsometricToUnityCoordinates(Vector2 isometricCoordinates)
    {
        float scale = 0.6f;
        float unityX = 50*(isometricCoordinates.x - isometricCoordinates.y);
        float unityY = 30+50*(isometricCoordinates.x + isometricCoordinates.y)* scale;
        return new Vector3(unityX, unityY, 0); // Retourner les coordonnées converties
    }
}







/*
    public void CreateProductTotem(string productName, Vector2Int position, int quantity)
    {
        //Debug.Log("coordonné iso" + position);
        //Debug.Log("coordonné calc" + IsometricToUnityCoordinates(position)); 
        // Créer un nouveau GameObject à partir du prefab
        GameObject productTotem = Instantiate(productPrefab, IsometricToUnityCoordinates(position), Quaternion.identity);
        productTotem.SetActive(true);
        productTotem.transform.SetParent(transform, false);

        // Assigner l'image correspondante au produit
        RawImage productImage = productTotem.GetComponent<RawImage>();
        productImage.texture = Resources.Load<Texture>($"Icons/Items/{productName}");

        //Mettre à jour la quantité
        TextMeshProUGUI textMeshPro = productTotem.transform.Find("Quantity").GetComponent<TextMeshProUGUI>();
        if (textMeshPro != null)
        {
            if(quantity > 1)
            {
                textMeshPro.text = "x"+quantity;
            }
            else 
            {
                textMeshPro.text = "";
            }
        }

        // Lancer la coroutine pour déplacer et faire disparaître l'icône
        float delay = 0f;
        if (lastTotemTimes.ContainsKey(position))
        {
            delay = Mathf.Max(0, delayBetweenTotems - (Time.time - lastTotemTimes[position]));
        }
        lastTotemTimes[position] = Time.time + delay;
        StartCoroutine(MoveAndFade(productTotem, delay));
    }

    private IEnumerator MoveAndFade(GameObject productTotem, float delay)
    {
        yield return new WaitForSeconds(delay);

        RectTransform rectTransform = productTotem.GetComponent<RectTransform>();
        Vector3 startPosition = rectTransform.anchoredPosition;
        Vector3 endPosition = startPosition + new Vector3(0, moveDistance, 0);
        RawImage productImage = productTotem.GetComponent<RawImage>();
        Color startColor = productImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Déplacer l'icône
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, endPosition, t);

            // Faire disparaître l'icône à partir de la moitié de la durée
            if (t >= 0.5f)
            {
                float fadeT = (t - 0.5f) / 0.5f; // Normaliser t pour la seconde moitié de la durée
                productImage.color = Color.Lerp(startColor, endColor, fadeT);
            }

            yield return null;
        }

        // Détruire le GameObject une fois le déplacement terminé
        Destroy(productTotem);
    }

    private Vector3 IsometricToUnityCoordinates(Vector2 isometricCoordinates)
    {
        float scale = 0.6f;
        float unityX = 50*(isometricCoordinates.x - isometricCoordinates.y);
        float unityY = 30+50*(isometricCoordinates.x + isometricCoordinates.y)* scale;
        return new Vector3(unityX, unityY, 0); // Retourner les coordonnées converties
    }
}*/


