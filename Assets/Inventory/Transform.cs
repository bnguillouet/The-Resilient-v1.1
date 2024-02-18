using System.Collections.Generic;
using UnityEngine;
public class Transform
{
    public string action { get; set; }
    public string subAction { get; set; }
    public string category { get; set; }
    public int specific { get; set; }
    public List<(List<string>, int)> ingredients { get; set; }
    public List<(string, int, int)> production { get; set; }
    public string placeType { get; set; }
    public string placeSubType { get; set; }
    //public List<string> source { get; set; }
    public string skill { get; set; }
    public string condition {get; set;} // custom condition
    public List<(string, int)> tools { get; set; }


    public Transform(string actioninput, string subActioninput, string categoryinput, int specificinput, List<(List<string>, int)> ingredientsInput,List<(string, int, int)> productioninput, string placeTypeinput, string placeSubtypeinput, string skillinput, string conditioninput, List<(string, int)> toolsinput)
    {
        action = actioninput;
        subAction = subActioninput;
        category = categoryinput;
        specific = specificinput;
        ingredients = ingredientsInput;
        production = productioninput;
        placeType = placeTypeinput;
        placeSubType = placeSubtypeinput;
        //source = new List<string>();
        skill = skillinput;
        condition = conditioninput;
        tools = toolsinput;
    }
}