using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Event
{
    public int id { get; set; }
    public string title { get; set; }
    public string eventType { get; set; } //Bonus, Neutre, Malus
    public string eventText { get; set; }
    public int eventTrigger { get; set; } //1 = Fin mois, 

    public Event (int idInput, string titleInput, string eventTypeInput, string eventTextInput, int eventTriggerInput)
    {
        id = idInput;
        title = titleInput;
        eventType = eventTypeInput;
        eventText = eventTextInput;
        eventTrigger = eventTriggerInput;
    }
}