using UnityEngine;
using UnityEngine.UI;
using FYFY;
using TMPro;
using System.IO;
using System;
using System.Xml;
using System.Collections.Generic;

public class ParamCompetenceSystem : FSystem
{

	public static ParamCompetenceSystem instance;

	// Famille
	private Family competence_f = FamilyManager.getFamily(new AllOfComponents(typeof(Competence)));

	// Variable
	public GameObject panelInfoComp;

	private GameData gameData;

	public ParamCompetenceSystem()
	{
		instance = this;
	}

	protected override void onStart()
	{
		gameData = GameObject.Find("GameData").GetComponent<GameData>();

		try
        {
			// On charge les donn�es pour chaque comp�tence
			loadParamComp();

			// On d�sactive les comp�tences pas encore impl�ment�
			desactiveToogleComp();

			// Note pour chaque comp�tence les niveaux ou elle est pr�sente
			readXMLinfo();
		}
        catch
        {
			Debug.Log("Erreur chargement fichier de parametrage des comp�tences");
        }


	}

	public void startLevel()
    {
		// On cr�er une copie de la liste des niveaux disponible
		List<string> copyLevel = new List<string>(gameData.levelList["Campagne"]);
		int nbCompActive = 0; // Permet de noter si au moins une comp�tence � �t� selectionn�e

		// On parcours chaque comp�tence selectionner
		foreach (GameObject comp in competence_f)
		{
			// Si La comp�tence est activ�
			if (comp.GetComponent<Toggle>().isOn)
			{
				nbCompActive += 1;
				// On parcourt ce qui reste des niveaux possible et pour chaque niveau on regarde si il est pr�sent dans la comp�tence selectionn�e
				// Si se n'est pas le cas on le supprime de la liste
				for(int i = 0; i < copyLevel.Count;) {
					bool levelOk = false;
					foreach(string level in comp.GetComponent<Competence>().listLevel)
                    {
						if(level == copyLevel[i])
                        {
							levelOk = true;
						}
                    }

                    if (levelOk)
                    {
						i++;
                    }
                    else
                    {
						copyLevel.Remove(copyLevel[i]);
					}
                }
			}
		}

		// Si on a au moins une comp�tence activ� et un niveau en commun
		// On lance un niveau selectionn� al�atoirement parmis la liste des niveaux restant
		if (nbCompActive != 0 && copyLevel.Count != 0)
        {
			if(copyLevel.Count > 1)
            {
				var rand = new System.Random();
				gameData.levelToLoad = ("Campagne", gameData.levelList["Campagne"].IndexOf(copyLevel[rand.Next(0, copyLevel.Count)]));
			}
            else
            {
				gameData.levelToLoad = ("Campagne", gameData.levelList["Campagne"].IndexOf(copyLevel[0]));
			}
			// TROUVER MOYEN DE CHARGER LES BONNES INFOS DANS GAMEDATA
			GameObjectManager.loadScene("MainScene");
		}
		else // Sinon on signal que aucune comp�tence n'est selectionn� ou que aucun niveau n'est disponible
        {
			// Si pas de competence selectionn�e
			if(nbCompActive == 0)
            {
				Debug.Log("Pas de comp�tence s�lectionn�e");
            }
			else if (copyLevel.Count == 0) // Si pas de niveau dispo
            {
				Debug.Log("Pas de niveau disponible pour l'ensemble des comp�tences selectionn�es");
			}
            else
            {
				Debug.Log("Erreur run level ");
			}
        }
	}

	public void infoCompetence(GameObject comp)
	{
		panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text = comp.GetComponent<Competence>().info;
		comp.transform.Find("Label").GetComponent<Text>().fontStyle = FontStyle.Bold;
	}

	public void resetViewInfoCompetence(GameObject comp)
    {
		panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text = "";
		comp.transform.Find("Label").GetComponent<Text>().fontStyle = FontStyle.Normal;
	}

	// Cgarement des parametre des comp�tences
	private void loadParamComp()
    {
		StreamReader reader = new StreamReader(Application.dataPath + "/StreamingAssets/ParamPrefab/ParamCompetence.csv");
		bool endOfFile = false;
		while (!endOfFile)
		{
			string data_string = reader.ReadLine();
			if (data_string == null)
			{
				endOfFile = true;
				break;
			}
			var data = data_string.Split(';');

			// On parcourt les �l�ment est on charge les parametre de la comp�tence sur la bonne comp�tence
			// Nom de la competence, titre, info
			foreach (GameObject comp in competence_f)
			{
				// Si c'est la comp�tence
				if (comp.name == data[0])
				{
					// On charge le text de la comp�tence
					comp.transform.Find("Label").GetComponent<Text>().text = data[1];
					// On charge les info de la comp�tence qui sera affich� lorsque l'on survolera celle-ci avec la souris
					comp.GetComponent<Competence>().info = data[2];
					// (temporaire) On charge si la comp�tence peut �tre selectionn�e (est-elle impl�ment�e)
					comp.GetComponent<Competence>().active = Convert.ToBoolean(data[3]);
					// On charge le vecteur des comp�tences qui seront automatiquement selectionn�es si la comp�tence est s�l�ctionn�e
					var data_link = data[4].Split(',');
					foreach (string value in data_link)
					{
						comp.GetComponent<Competence>().compLink.Add(value);
					}
					// On charge le vecteur des comp�tences qui seront automatiquement gris�es si la comp�tence est s�l�ctionn�e
					var data_noPossible = data[5].Split(',');
					foreach (string value in data_noPossible)
					{
						comp.GetComponent<Competence>().compNoPossible.Add(value);
					}
				}
			}
		}
	}

	// D�sactive les toogles pas encore impl�ment�
	private void desactiveToogleComp()
    {
		foreach (GameObject comp in competence_f)
		{
			if (!comp.GetComponent<Competence>().active)
			{
				comp.GetComponent<Toggle>().interactable = false;
			}
		}
	}

	// Lis tous les fichiers XML des niveaux afin de charger quelle comp�tence se trouve dans quel niveau  
	private void readXMLinfo()
    {
        foreach(string level in gameData.levelList["Campagne"])
        {
			XmlDocument doc = new XmlDocument();
			if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				doc.LoadXml(level);
				loadInfo(doc, level);
			}
			else
			{
				doc.Load(level);
				loadInfo(doc, level);
			}
		}
	}

	// Parcourt le noeud d'information est apelle les bonnes fonctions pour traiter l'information du niveau
	private void loadInfo(XmlDocument doc, string namelevel)
	{
		XmlNode root = doc.ChildNodes[1];
		foreach (XmlNode child in root.ChildNodes)
		{
			switch (child.Name)
			{
				case "info":
					foreach (XmlNode infoNode in child.ChildNodes)
					{
                        switch (infoNode.Name)
                        {
							case "comp":
								addInfo(infoNode, namelevel);
								break;
						}
					}
					break;
			}
		}
	}

	// Associe � chaque comp�tence renseigner sa pr�sence dans le niveau
	private void addInfo(XmlNode node, string namelevel)
    {
		foreach (GameObject comp in competence_f)
		{
			if (node.Attributes.GetNamedItem("name").Value == comp.name)
			{
				comp.GetComponent<Competence>().listLevel.Add(namelevel);
			}
		}
	}
}