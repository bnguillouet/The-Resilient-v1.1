using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ActionPile : MonoBehaviour
{
    //public List<ActionExecution> actionsExecution = new List<ActionExecution>();
    public CharacterToken selectedToken { get; set; }
    //public GameObject inventoryMenu; 
    public GameObject originalActionObject;
    public static ActionPile Instance;

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


    public void Refresh()
    {
        //Nettoyage
        selectedToken = Tribe.activeMember.characterToken;
        GameObject panelAction = transform.Find("Scroll View").gameObject.transform.Find("Viewport").gameObject.transform.Find("Content").gameObject;
        foreach (UnityEngine.Transform child in panelAction.transform){GameObject.Destroy(child.gameObject);} // Supprimer tous les icones du Panel Inventory

        float currentY = -13f; //+ 26 a chaque ajout 
        foreach (Action actionExecution in selectedToken.actionsExecution)
        {

            GameObject actionObject = Instantiate(originalActionObject);
            actionObject.transform.SetParent(panelAction.transform);
            actionObject.SetActive(true);
            actionObject.name = "Action_"+name;
            Button cancelButton = actionObject.transform.Find("Button_Cancel").GetComponent<Button>();
            cancelButton.onClick.AddListener(() => RemoveAction(actionExecution));

            TextMeshProUGUI textMeshPro = actionObject.transform.Find("Action_Text").GetComponent<TextMeshProUGUI>();
            if (textMeshPro != null)
            {
                textMeshPro.text = "["+ actionExecution.actionPosition.x + "/"+ actionExecution.actionPosition.y +"] "+ actionExecution.actionName; //TO DO : peut etre mettre la subaction si existe
            }
            /*
            RawImage rawImage = actionObject.transform.Find("Action_Image").GetComponent<RawImage>();
            if (rawImage != null)
            {
                Texture2D texture = Resources.Load<Texture2D>("Icons/Status/");
                rawImage.texture = texture; 
            }*/
            RectTransform rectItem = actionObject.GetComponent<RectTransform>();
            rectItem.anchoredPosition = new Vector3(150, currentY , 0);
            currentY -= 26;
        }        
    }

    public void RemoveAction(Action actionExecution)
    {
        selectedToken.actionsExecution.Remove(actionExecution);
        Refresh();
    }
}

