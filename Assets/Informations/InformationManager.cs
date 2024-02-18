using UnityEngine;
using TMPro;
using System.Text;

public class InformationManager : MonoBehaviour
{
    public static InformationManager Instance;
    public TextMeshProUGUI hoverText;
    public TextMeshProUGUI informationText;
    public GameObject informationGameObject;
    //public TextMeshProUGUI informationText;

    private void Start()
    {
        // Assurez-vous d'assigner le composant TextMeshProUGUI dans l'inspecteur.
        if (hoverText == null)
        {
            Debug.LogError("Le composant TextMeshProUGUI n'est pas assigné.");
        }

        // Appel de SetInfo pour mettre à jour le texte.
        SetHoverInfo("Ligne 1\nLigne 2\nLigne 3\nLigne 4\nLigne 5");
        SetHoverInfo("Ligne 6\nLigne 7\nLigne 8\nLigne 9\nLigne 10");
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
        informationGameObject.SetActive(true);
    }   

    public void EraseLiveInfo()
    {
        informationGameObject.SetActive(false);
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