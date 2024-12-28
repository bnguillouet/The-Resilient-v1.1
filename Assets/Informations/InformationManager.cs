using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.UI;
using System.Collections;  // Ajoute cette ligne


public class InformationManager : MonoBehaviour
{
    public static InformationManager Instance;
    public TextMeshProUGUI hoverText;
    public TextMeshProUGUI informationText;
    public GameObject informationGameObject;
    public bool forceClose;
    //public TextMeshProUGUI informationText;

    private void Start()
    {
        // Assurez-vous d'assigner le composant TextMeshProUGUI dans l'inspecteur.
        if (hoverText == null)
        {
            Debug.LogError("Le composant TextMeshProUGUI n'est pas assigné.");
        }

        forceClose = false;
    }

    // Méthode pour mettre à jour le texte avec retour à la ligne

    public void SetHoverInfo(string newText)
    {
        hoverText.text = newText;
    }
    public void AddHoverInfo(string newText)
    {
        hoverText.text = hoverText.text + "<br>--------------------------------<br>" + newText;
    }

    public void SetLiveInfo(string newText)
    {
        informationText.text = newText;
        forceClose = false;
        //informationGameObject.SetActive(false);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(informationText.GetComponent<RectTransform>());
        //LayoutRebuilder.ForceRebuildLayoutImmediate(informationGameObject.GetComponent<RectTransform>());
        //informationGameObject.SetActive(true);
        StartCoroutine(ShowAfterDelay(0.3f));
    }   
        

    private IEnumerator ShowAfterDelay(float delay)
    {
        if (!forceClose)
        {
            yield return new WaitForSeconds(delay);
            informationGameObject.SetActive(false);  // Désactiver le GameObject
            LayoutRebuilder.ForceRebuildLayoutImmediate(informationText.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(informationGameObject.GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();  // Force Unity à recalculer les Canvases immédiatement
            informationGameObject.SetActive(true);
        }

    }

    public void EraseLiveInfo()
    {
        informationGameObject.SetActive(false);
        forceClose = true;
    }   

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        targetPosition.z = 0;
        mousePosition.y += 30;
        informationGameObject.transform.position = mousePosition;
    }

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
}