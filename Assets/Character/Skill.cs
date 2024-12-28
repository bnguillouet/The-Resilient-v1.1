using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Skill
{
    public string name { get; set; }
    public string parentname { get; set; }
    public string description { get; set; }
    public Skill (string nameinput, string parentnameinput, string descriptioninput)
    {
        name = nameinput;
        parentname = parentnameinput;
        description = descriptioninput;
    }
}