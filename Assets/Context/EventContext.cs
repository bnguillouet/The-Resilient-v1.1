using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;
using TMPro;
public class EventContext : MonoBehaviour
{
    public static EventContext Instance; // Singleton pour accéder facilement aux paramètres du jeu
    /*Attributs de l'evenement */
    public  TextMeshProUGUI eventText, inputfieldText;
    /*public  InputField inputField;*/
    public GameObject inputField;
    public Button okButton;
    public GameObject eventGameObject;
    public bool liveEvent;

    public List<Event> events { get; set; }

    private void Awake()
    {
        // Assurez-vous qu'il n'y ait qu'une seule instance de GameSettings
        if (Instance == null)
        {
            Instance = this;
            events = new List<Event>();
            InitializeEvents();
            if (okButton == null){okButton = GetComponent<Button>();}
            if (okButton != null){okButton.onClick.AddListener(OkButtonClick);}
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OkButtonClick()
    {
        eventGameObject.SetActive(false);
        liveEvent = false;
        inputField.SetActive(false);
    }

    public void LaunchEndMonthEvent()
    {
        int random = UnityEngine.Random.Range(0, 100);
        Debug.Log("Event random : " + random);
        /*Event currentEvent = events[random];*/
        Event currentEvent = events.Find(e => e.id == random);
        if (((GameSettings.Instance.DifficultyLevel == 0 || GameSettings.Instance.DifficultyLevel == 99) && currentEvent.eventType == "Malus") || (GameSettings.Instance.DifficultyLevel == 2 && currentEvent.eventType == "Bonus"))
        {
            random = 49;
        }
        LaunchEvent(random, null); 
    } 
    public void LaunchEvent(int idEvent, Human human)
    {
        /*Event currentEvent = events[idEvent];*/
        Event currentEvent = events.Find(e => e.id == idEvent);
        eventGameObject.SetActive(true);
        liveEvent = true;
        eventText.text = "Nous sommes maintenant au mois de <br><size=22>"+TurnContext.Instance.ActuelMonth()+"<br><br>"+ currentEvent.title + "</size><br><br>" + currentEvent.eventText;
        if (idEvent == 113)
        {
            inputField.SetActive(true);
        }
        ExecuteEvent(idEvent);
    }    

    public void ExecuteEvent(int idEvent)
    {
        Debug.LogError("L'evenement est exécuté :" + idEvent);
    }

    public void InitializeEvents()
    {
        events.Add (new Event(1,"Conditions idéales","Bonus","Les conditions sont idéales ce mois-ci. Bonus de 30 secondes de temps pour ce tour.",1));
        events.Add (new Event(2,"La nuit des étoiles filantes","Bonus","Toute la communauté s'est retrouvée allongée dans l'herbe a admirer les étoiles filantes. Un simple moment de bonheur. + 5 Bonheur pour tout le monde.",1));
        events.Add (new Event(3,"De drôles d'histoires","Bonus","<Personnage> nous a raconté ses aventures dans son ancienne vie. Il nous a tous bien fait marré. + 2 Bonheur pour tout le monde",1));
        events.Add (new Event(4,"Soirée musicale","Bonus","<Personnage> a organisé une soirée musicale. On a sortie les guitares et les tam-tam. Quel beau moment ! + 2 Bonheur pour tout le monde",1));
        events.Add (new Event(5,"Mariage dans la communauté","Bonus","<Personnage> s'est marié aujourd'hui. On a célébré cela comme il se devait. + 5 Bonheur pour <Personnage>",1));
        events.Add (new Event(6,"Visite surprise","Bonus","<Personnage> a eu la suprise d'être visité par un vieil ami. + 2 Bonheur pour <Personnage>",1));
        events.Add (new Event(7,"Excédent sur les produits","Bonus","L'année a été particulièrement productive pour l'agriculture et l'industrie régionale. Les produits sont disponibles en grand nombre ce mois-ci.",1));
        events.Add (new Event(8,"Don à la communauté","Bonus","Un généreux donateur admire la manière dont votre communauté respecte la nature et montre comment vivre autrement est possible ",1));
        events.Add (new Event(9,"Cadeau d'une pelle","Bonus","Vous nourrissez un aventurier de passage. Pour vous remercier, il vous offre une pelle.",1));
        events.Add (new Event(10,"Cadeau de batéries lactiques","Bonus","Vous dépannez la communauté voisine en oeuf. Pour vous remercier, il vous offre des bactéries lactiques.",1));
        events.Add (new Event(11,"Cadeau d'une baratte","Bonus","Vous offrez quelques fruits à un pélerin qui passait non loin. Pour vous remercier, il vous offre une baratte pour fabriquer du beurre",1));
        events.Add (new Event(12,"Cadeau d'une serpe","Bonus","",1));
        events.Add (new Event(13,"Un petit stock d'argile","Bonus","Vous avez trouvé un gisement d'argile lors d'une ballade en sous-bois. Vous avez 5 unités d'argile ajouté à votre inventaire",1));
        events.Add (new Event(14,"Vide grenier","Bonus","En préparant le vide grenier annuel de <Nom ville>, on a trouvé un objet qu'on avait oublié : <Objet>",1));
        events.Add (new Event(15,"Don généreux","Bonus","Un marchand nous a offert quelques chose : <Objet>",1));
        events.Add (new Event(16,"La bonne étoile","Bonus","<Personnage> va beaucoup mieux. C'est comme un miracle.",1));
        events.Add (new Event(17,"Un druide à la maison","Bonus","Biderwan la druide est passé nous rendre visite. Il a pu soigner <Personnage> qui est remis sur pied.",1));
        events.Add (new Event(18,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(19,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(20,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(21,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(22,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(23,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(24,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(25,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(26,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(27,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(28,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(29,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(30,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(31,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(32,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(33,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(34,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(35,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(36,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(37,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(38,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(39,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(40,"Bonus à venir","Bonus","Bonus à venir",1));
        events.Add (new Event(41,"Nouveau rapport du GIEC","Neutre","Bonne nouvelle : le nouveau rapport du GIEC montre que les efforts des gouvernements et de chacun va dans la bonne direction. En espérant que ca ne soit pas trop tard",1));
        events.Add (new Event(42,"Encore l'océan qui monte","Neutre","Lors des derniers relevés, le niveau de la mer a encore progressé de 7 cm depuis l'annnée dernière.",1));
        events.Add (new Event(43,"Je ne croise plus de Bruant des roseaux","Neutre","C'est très étrange. Cela fait plusieurs mois que je ne vois plus de bruant des roseaux lors de mes ballades du soir.",1));
        events.Add (new Event(44,"Les villes se vident","Neutre","Une nouvelle étude révèle que les grandes villes ont perdues 40% de leur population depuis 10 ans. Sans doute, les habitants ont renouer avec leurs racines à la campagne. ",1));
        events.Add (new Event(45,"Un nomade en woofing","Neutre","<prenom> est un ébéniste nomade qui traverse les contrées afin de donner un coup de main dans les communautés résilientes ",1));
        events.Add (new Event(46,"Fête de la nature","Neutre","On a organisé la fête de la nature. Bon, on va moins travailler ce mois-ci mais c'est important de s'arrêter et de profiter de la vie aussi.",1));
        events.Add (new Event(47,"Nouveau président d'association","Neutre","<Personnage> a été choisi pour nous représenté comme président de notre association à but non lucratif. Toute décision de la communauté est validée de manière démocratique",1));
        events.Add (new Event(48,"Réunion exceptionnelle","Neutre","L'ensemble des membres de la communauté s'est réuni pour discuter de nos futurs projet. Comme toujours, on a plein d'idée et encore beaucoup de travail pour être autonome.",1));
        events.Add (new Event(49,"Rien de particulier","Neutre","Il n'y rien de particulier ce mois-ci. ",1));
        events.Add (new Event(50,"Business as usual","Neutre","La bourse a encore pris 3,4 % depuis le début du mois. Le monde continue de regarder ailleurs alors que notre maison brûle.",1));
        events.Add (new Event(51,"Event neutre à venir","Neutre","Event neutre à venir",1));
        events.Add (new Event(52,"Event neutre à venir","Neutre","Event neutre à venir",1));
        events.Add (new Event(53,"Event neutre à venir","Neutre","Event neutre à venir",1));
        events.Add (new Event(54,"Event neutre à venir","Neutre","Event neutre à venir",1));
        events.Add (new Event(55,"Event neutre à venir","Neutre","Event neutre à venir",1));
        events.Add (new Event(56,"Event neutre à venir","Neutre","Event neutre à venir",1));
        events.Add (new Event(57,"Event neutre à venir","Neutre","Event neutre à venir",1));
        events.Add (new Event(58,"Event neutre à venir","Neutre","Event neutre à venir",1));
        events.Add (new Event(59,"Event neutre à venir","Neutre","Event neutre à venir",1));
        events.Add (new Event(60,"Event neutre à venir","Neutre","Event neutre à venir",1));
        events.Add (new Event(61,"C'est la tempète","Malus","Une tempète est annoncée ce mois-ci. Attention aux dommages que cela pourrait causer.",1));
        events.Add (new Event(62,"Un été caniculaire","Malus","Encore les conséquences du changement climatique. Le thermomètre atteint 43° à l'ombre, c'est étouffant.",1));
        events.Add (new Event(63,"Un été incendiaire","Malus","A cause du changement climatique (ou peut être d'origine humaine) un incendie s'est déclaré dans la forêt voisine. Vous ne pouvez pas y aller pour le tour. En espérant que ce sera sous contrôle rapidement.",1));
        events.Add (new Event(64,"Un printemps sec","Malus","Le printemps est particulièrement sec cette année. Aucune précipitation prévu ce mois-ci.",1));
        events.Add (new Event(65,"Chicane : Départ d'un membre","Malus","<Personnage> a eu un conflit avec un autre membre de la communauté. <Personnage> a préférer quitter le groupe pour s'installer ailleurs",1));
        events.Add (new Event(66,"Confinement : Epidémie","Malus","Une nouvelle épidémie mondiale vient d'éclore. Un confinement a été décrété. Pour ce tour, il n'est pas possible de sortir de votre terrain.",1));
        events.Add (new Event(67,"Pénurie au marché","Malus","Une pénurie est en cours sur certains produit. Les magasins ont été pris d'assault et il ne reste plus grand chose.",1));
        events.Add (new Event(68,"Epidémie de grippe aviaire","Malus","Une nouvelle épidémie de grippe aviaire (H5N1) est en cours dans la région. Toutes nos poules ont du être abattues",1));
        events.Add (new Event(69,"C'est la semaine des remises","Malus","Il y'a des bons prix en ce moment dans les marchés. Peut être le moment de faire des réserves.",1));
        events.Add (new Event(70,"Vol dans nos réserves","Malus","On nous a volé des ressources. La communauté n'a rien vu et on nous a volé une trentaine de ressources.",1));
        events.Add (new Event(71,"Cambriolage ","Malus","On nous a cambriolé. Des outils et de l'argent ont été subtilisés.",1));
        events.Add (new Event(72,"Outil brisé","Malus","Un outil, <Outil>, a été brisé suite à une vérification. Il faudra en retrouver un.",1));
        events.Add (new Event(73,"Des conserves ont été contaminées","Malus","Mince, lors de la dernière fabrication, des pots étaient mals stérilisés. On a perdu 5 Conserves.",1));
        events.Add (new Event(74,"Infesté de charançons","Malus","On a découvert des charançons dans nos réserves de graines. On a perdu 10 unités de graines.",1));
        events.Add (new Event(75,"Feu de paille","Malus","Un début d'incendie à eu lieu dans la remise de paille. Heureusement, on est intervenu à temps. On a quand même perdu 20 unités de paille.",1));
        events.Add (new Event(76,"Champignon vénéreux","Malus","<Personnage> est tombé malade en consommant un champignon vénéreux. Son statut change pour empoissonné. Il lui faudrait rapidement une potion.",1));
        events.Add (new Event(77,"Maladie dans la communauté","Malus","<Personnage> est tombé malade. Sans doute à cause du changement de saison. <Personnage> va rester au chaud à la maison ce mois-ci, il lui faudrait des médicaments pour s'assurer s'aller mieux le mois prochain.",1));
        events.Add (new Event(78,"C'est pas son jour !","Malus","<Personnage> est un peu patraque. Gueule de bois ou coup de vieux ? On ne sait pas, en tout cas, <Personnage> va être plus lent sur ce tour.",1));
        events.Add (new Event(79,"Pénurie sur l'alimentaire","Malus","Pénurie sur l'alimentaire. Tout le monde fait des reserves et il ne reste pas grand chose dans les magasins.",1));
        events.Add (new Event(80,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(81,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(82,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(83,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(84,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(85,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(86,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(87,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(88,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(89,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(90,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(91,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(92,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(93,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(94,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(95,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(96,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(97,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(98,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(99,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(100,"Malus à venir","Malus","Malus à venir",1));
        events.Add (new Event(101,"Communauté endeuillée !","Malus","<Personnage> est mort de vieillesse à l'age de <age du personnage> . Paix à son âme.",2));
        events.Add (new Event(102,"Le grand festin !","Bonus","Un joyeux festin a eu lieu dans votre <nom complet du lieu> . Vous avez pu démontrer à l'ensemble du village que vous pouviez vivre de manière résiliente et en harmonie.",3));
        events.Add (new Event(103,"Bactérie dans le lait cru","Malus","Une bactérie s'est développée dans le lait cru. On a perdu tout notre stock de Lait cru.",4));
        events.Add (new Event(104,"Nouvelle compétence","Bonus","<Personnage> a passé avec succès sa formation de <Compétence>. Il peut maintenant profiter des nouvelles actions associées à cela.",5));
        events.Add (new Event(105,"Blessé en taillant du bois","Malus","<Personnage>  s'est blessé en taillant du bois. Il reste à la maison pour ce tour, et pourra être soigné plus rapidement avec des baumes réparateurs.",6));
        events.Add (new Event(106,"Objet trouvé en coupant l'herbe","Bonus","<Personnage> a trouvé un objet : <Objet> en coupant l'herbe",7));
        events.Add (new Event(107,"Objet trouvé lors de la cueillette","Bonus","<Personnage> a trouvé un objet : <Objet> en allant a la cueillette",8));
        events.Add (new Event(108,"Blessé en coupant un arbre","Malus","<Personnage>  s'est blessé en coupant un arbre. Il reste à la maison pour ce tour, et pourra être soigné plus rapidement avec des baumes réparateurs.",9));
        events.Add (new Event(109,"Mort à la chasse","Malus","<Personnage> a rencontré une bête sauvage qui l'a pris en chasse et s'est occupé de lui. <Personnage> est décédé.",10));
        events.Add (new Event(110,"Blessé à la chasse","Malus","<Personnage>  s'est blessé en allant à la chasse. Il reste à la maison pour ce tour, et pourra être soigné plus rapidement avec des baumes réparateurs.",11));
        events.Add (new Event(111,"Blessé en buchant","Malus","<Personnage>  s'est blessé en buchant du bois. Il reste à la maison pour ce tour, et pourra être soigné plus rapidement avec des baumes réparateurs.",12));
        events.Add (new Event(112,"On est maintenant 10 !","Neutre","Nous accueillons aujourd'hui notre dixième membre. Je me souviens des débuts de l'aventure où nous n'étions que deux...",13));
        events.Add (new Event(113,"Niveau 2 - Colonie Survivaliste","Neutre","Le petit refuge est maintenant devenu une petite colonie survivaliste. L'ensemble des besoins de base pour la survie ont été satisfaits. Il y'a maintenant de nouveaux objectifs de Sociabilisation (6 membres), de Respect du vivant, d'Hygiène, de Protéine, d'Habillement et de Médication, afin de devenir un éco-lieu organisé.",14));
        events.Add (new Event(114,"Niveau 3 - Eco-Lieu organisé","Neutre","La petite colonie est maintenant devenue un éco-lieu organisé. L'ensemble des besoins pour l'organisation ont été satisfaits. Il y'a maintenant de nouveaux objectifs de Santé, d'Ingénierie (Electricité), de Variété de produit, d'Argent, de Connaissance et de non-Pollution (retraitement des eaux), afin de devenir un éco-hameau résilient.",15));
        events.Add (new Event(115,"Niveau 4 - Eco-Hameau résilient","Neutre","L'éco-lieu est maintenant devenu un éco-hameau résilient. L'ensemble des besoins pour la résilience ont été satisfaits. Il y'a maintenant de nouveaux objectifs de Bonheur, de Produit avancés, d'Influence, de Culture, de Communauté (15 membres) et d'Autonomie (plus d'achat), afin de devenir un éco-village épanoui.",16));
        events.Add (new Event(116,"Niveau 5 - Eco-Village épanoui !","Neutre","Félicitation ! L'Eco-hameau est devenu un éco-village épanoui. Vous avez atteint le niveau maximum... Vous pouvez continuer votre partie ou bien adapter ce que vous avez appris dans la vie réelle ;)",17));
    }
}