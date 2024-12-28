using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;

public class SkillsManager : MonoBehaviour
{
    public List<Skill> skills { get; set; }

    /****************************************************/
    /********** INITIALISATION DE LA TRIBU **************/
    /****************************************************/
    public static SkillsManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            skills = new List<Skill>();
            InitializeSkills();
        }
        else
        {
            Debug.LogWarning("Il ne peut y avoir qu'une seule instance de SkillsManager.");
            Destroy(gameObject);
        }
    }

    public bool CanAddSkill(string skillName, List<string> skillList)
    {
        foreach (Skill skill in skills)
        {
            if (skill.name.Equals(skillName))
            {
                if (skillList.Contains(skill.parentname) || skill.parentname == "")
                {
                    return true;
                }
                else 
                {
                    return false;
                }
            }
        }
        return false;
    }

    public List<string> AvailableTrainings(List<string> skillList)
    {
        List<string> availableTrainings = new List<string>();
        foreach (Skill skill in skills)
        {
            if (!skillList.Contains(skill.name))
            {
                if (skillList.Contains(skill.parentname) || skill.parentname == "")
                {
                    availableTrainings.Add(skill.name);  
                }
            }
        }
        return availableTrainings;
    }

    public void InitializeSkills()
    {
        skills.Add (new Skill("Paysanerie", "", "Permet de planter des céréales et de réaliser des plantations complexes, avec des conditions spécifiques pour les parcelles de céréales et des bonus pour les légumes."));
        skills.Add (new Skill("Élevage", "Paysanerie", "Permet de mettre des animaux (mouton, lapin, canard) dans un enclos et de s’en occuper, avec une exception pour les poules (bonus simple) et excluant les abeilles."));
        skills.Add (new Skill("Horticulture", "Paysanerie", "Permet de planter tous les arbres et arbustes du verger, avec des bonus appliqués aux arbres et une création facile de graines et boutures."));
        skills.Add (new Skill("Apiculture", "Paysanerie", "Permet d’installer des essaims dans des ruches et de s’occuper des abeilles, apportant des bonus aux fleurs et certaines cultures."));
        skills.Add (new Skill("Artisanat", "", "Permet les actions de fabrication de base comme le cidre, la confiture, le pain de base, la couture de base et des créations simples."));
        skills.Add (new Skill("Fabrication", "Artisanat", "Permet de créer des objets d’artisanat avancé tels que savon, savon parfumé, toile cirée, bougie, avec des bonus de fabrication sur certains objets."));
        skills.Add (new Skill("Brassage", "Artisanat", "Permet de créer de la bière et de la bière d’abbaye, avec un bonus de +1 pour la création du cidre."));
        skills.Add (new Skill("Vinification", "Artisanat", "Permet de créer des barriques de vin, du vin ou du vin de garde."));
        skills.Add (new Skill("Couture", "Artisanat", "Permet de créer des habits, tenues du dimanche, senteurs, sacs de conservation et couvertures."));
        skills.Add (new Skill("Céramique", "Artisanat", "Permet de créer de la vaisselle, des pots et des sculptures, ainsi que des outils comme beurrier et vinaigrier."));
        skills.Add (new Skill("Boulangerie", "Artisanat", "Permet de créer des pains et des tartes évolués."));
        skills.Add (new Skill("Cuisine", "Artisanat", "Permet de préparer des plats plus avancés ou de transformer le lait et d’autres produits (hors cidre, jus, lait, huile)."));
        skills.Add (new Skill("Naturalisme", "", "Permet de partir à la cueillette en forêt (racines, baies), de chasser et de respecter les attentes de la terre."));
        skills.Add (new Skill("Herboristerie", "Naturalisme", "Permet de fabriquer des potions, des pommades et des médicaments."));
        skills.Add (new Skill("Champignon", "Naturalisme", "Permet de créer des cultures de champignons dans une grange, avec un bonus à la cueillette sur la quantité de champignons et aucun risque de contamination."));
        /*skills.Add (new Skill("Chasse", "Naturalisme", "Permet d’obtenir un bonus sur la quantité de viande perçue en chasse, avec une meilleure connaissance de l’écosystème et la préservation de l’équilibre en forêt."));*/
        skills.Add (new Skill("Bricolage", "", "Permet de construire et déplacer tous les aménagements de type structure, ainsi que de créer des objets à partir de bois et de quincaillerie."));
        skills.Add (new Skill("Maçonnerie", "Bricolage", "Permet de construire des gros bâtiments comme maison, hangar, gros atelier, réserve de bois."));
        skills.Add (new Skill("Ingénierie", "Bricolage", "Permet de construire les bâtiments d’électricité et les systèmes d’eau."));
        skills.Add (new Skill("Ébénisterie", "Bricolage", "Permet de construire des meubles et des tonneaux."));
        /*skills.Add (new Skill("Forge", "Bricolage", "Permet de fabriquer des couteaux, outils de menuiserie et autres outils à partir du métal récupéré."));*/
    }
}

