/*using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventPile : MonoBehaviour
{
    public GameObject originalEventObject; // Le prefab pour chaque événement dans la pile
    public static EventPile Instance;
    public int maxEvents = 200; // Limite du nombre maximum d'événements à stocker

    // Structure pour représenter un événement
    public class Event
    {
        public string text;        // Le texte de l'événement
        public string category;    // La catégorie pour l'icône (ex. "info", "warning", etc.)
        public int importance;     // 0 = normal, 1 = warning, 2 = important (définit la couleur)

        public Event(string text, string category, int importance)
        {
            this.text = text;
            this.category = category;
            this.importance = importance;
        }
    }

    // Liste des événements
    public List<Event> events = new List<Event>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Méthode pour ajouter un nouvel événement à la pile
    public void AddEvent(string text, string category, int importance)
    {
        // Ajouter le nouvel événement en haut de la liste
        events.Insert(0, new Event(text, category, importance));

        // Vérifier la limite et supprimer les anciens événements si nécessaire
        if (events.Count > maxEvents)
        {
            events.RemoveAt(events.Count - 1); // Supprimer le plus ancien événement (index max)
        }

        // Rafraîchir l'affichage après l'ajout
        Refresh();
    }

    // Méthode pour rafraîchir l'affichage de la pile d'événements
    public void Refresh()
    {
        // Nettoyage de la pile d'événements
        GameObject panelEvent = transform.Find("Scroll View").Find("Viewport").Find("Content").gameObject;
        foreach (UnityEngine.Transform child in panelEvent.transform)
        {
            GameObject.Destroy(child.gameObject); // Supprimer tous les icônes du panel
        }

        float currentY = -10f; // Point de départ pour positionner les événements
        foreach (Event ev in events)
        {
            GameObject eventObject = Instantiate(originalEventObject);
            eventObject.transform.SetParent(panelEvent.transform);
            eventObject.SetActive(true);
            eventObject.name = "Event_" + ev.text;

            // Trouver et définir le texte de l'événement
            TextMeshProUGUI textMeshPro = eventObject.transform.Find("Event_Text").GetComponent<TextMeshProUGUI>();
            if (textMeshPro != null)
            {
                textMeshPro.text = ev.text;
                // Changer la couleur selon l'importance
                switch (ev.importance)
                {
                    case 0:
                        textMeshPro.color = Color.white; // Normal
                        break;
                    case 1:
                        textMeshPro.color = Color.yellow; // Warning
                        break;
                    case 2:
                        textMeshPro.color = Color.red; // Important
                        break;
                }
            }

            // Trouver et définir l'icône selon la catégorie
            RawImage rawImage = eventObject.transform.Find("Event_Icon").GetComponent<RawImage>();
            if (rawImage != null)
            {
                // Charger l'icône basée sur la catégorie (assurez-vous d'avoir les bons fichiers dans Resources/Icons/Status)
                Texture2D texture = Resources.Load<Texture2D>("Icons/Status/" + ev.category);
                rawImage.texture = texture;
            }

            // Positionner l'événement dans la pile
            RectTransform rectItem = eventObject.GetComponent<RectTransform>();
            rectItem.anchoredPosition = new Vector3(200, currentY, 0); // Aligné en haut
            currentY -= 20; // Espacement vertical entre les événements
        }
    }
}
*/


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventPile : MonoBehaviour
{
    public GameObject originalEventObject; // Le prefab pour chaque événement dans la pile
    public static EventPile Instance;
    public int maxEvents = 200; // Limite du nombre maximum d'événements à stocker

    // Structure pour représenter un événement
    public class Event
    {
        public string text;        // Le texte de l'événement
        public string category;    // La catégorie pour l'icône (ex. "info", "warning", etc.)
        public int importance;     // 0 = normal, 1 = warning, 2 = important (définit la couleur)
        public Vector2Int position; // Position vers laquelle la caméra doit se déplacer

        public Event(string text, string category, int importance, Vector2Int position)
        {
            this.text = text;
            this.category = category;
            this.importance = importance;
            this.position = position;
        }
    }

    // Liste des événements
    public List<Event> events = new List<Event>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Méthode pour ajouter un nouvel événement à la pile
    public void AddEvent(string text, string category, int importance, Vector2Int position)
    {
        // Ajouter le nouvel événement en haut de la liste
        events.Insert(0, new Event(text, category, importance, position));

        // Vérifier la limite et supprimer les anciens événements si nécessaire
        if (events.Count > maxEvents)
        {
            events.RemoveAt(events.Count - 1); // Supprimer le plus ancien événement (index max)
        }

        // Rafraîchir l'affichage après l'ajout
        Refresh();
    }

    // Méthode pour rafraîchir l'affichage de la pile d'événements
    public void Refresh()
    {
        // Nettoyage de la pile d'événements
        GameObject panelEvent = transform.Find("Scroll View").Find("Viewport").Find("Content").gameObject;
        foreach (UnityEngine.Transform child in panelEvent.transform)
        {
            GameObject.Destroy(child.gameObject); // Supprimer tous les icônes du panel
        }
        Debug.Log("Refresh du panel event");
        float currentY = -10f; // Point de départ pour positionner les événements
        foreach (Event ev in events)
        {
            GameObject eventObject = Instantiate(originalEventObject);
            eventObject.transform.SetParent(panelEvent.transform);
            eventObject.SetActive(true);
            eventObject.name = "Event_" + ev.text;

            // Trouver et définir le texte de l'événement
            TextMeshProUGUI textMeshPro = eventObject.transform.Find("Event_Text").GetComponent<TextMeshProUGUI>();
            if (textMeshPro != null)
            {
                textMeshPro.text = ev.text;
                // Changer la couleur selon l'importance
                switch (ev.importance)
                {
                    case 0:
                        textMeshPro.color = Color.white; // Normal
                        break;
                    case 1:
                        textMeshPro.color = Color.yellow; // Warning
                        break;
                    case 2:
                        textMeshPro.color = Color.red; // Important
                        break;
                }
            }
            if (ev.category == "None")
            {
                Destroy(eventObject.transform.Find("Event_Icon").gameObject);
            }
            else
            {
                // Trouver et définir l'icône selon la catégorie
                RawImage rawImage = eventObject.transform.Find("Event_Icon").GetComponent<RawImage>();
                if (rawImage != null)
                {
                    // Charger l'icône basée sur la catégorie (assurez-vous d'avoir les bons fichiers dans Resources/Icons/Status)
                    Texture2D texture = Resources.Load<Texture2D>("Icons/Status/" + ev.category);
                    rawImage.texture = texture;
                }
            }
            if (ev.position == new Vector2Int(-1, -1))
            {
                Destroy(eventObject.transform.Find("Event_Button").gameObject);
            }
            else
            {
                // Ajouter un listener au bouton pour déplacer la caméra vers la position de l'événement
                Button eventButton = eventObject.transform.Find("Event_Button").GetComponent<Button>();
                if (eventButton != null)
                {
                    // Capturer la position de l'événement
                    Vector2Int eventPosition = ev.position;
                    eventButton.onClick.AddListener(() => MoveCameraToPosition(eventPosition));
                }
            }

            // Positionner l'événement dans la pile
            RectTransform rectItem = eventObject.GetComponent<RectTransform>();
            rectItem.anchoredPosition = new Vector3(200, currentY, 0); // Aligné en haut
            currentY -= 20; // Espacement vertical entre les événements
        }
    }

    public void InitScrollBar()
    {
        Scrollbar verticalScrollbar = transform.Find("Scroll View").gameObject.transform.Find("Scrollbar Vertical").GetComponent<Scrollbar>();
        verticalScrollbar.value = 1;
    }

    // Fonction pour déplacer la caméra vers une position donnée
    public void MoveCameraToPosition(Vector2Int position)
    {
        Debug.Log("Déplacement de la caméra vers la position: " + position);
        // Appel de la fonction qui déplace réellement la caméra
        // CameraController.Instance.MoveTo(position);  // Par exemple
    }
}