using UnityEngine;
using UnityEngine.UI;
using FYFY;
using TMPro;
using System.IO;

public class ParamCompetenceSystem : FSystem
{

	public static ParamCompetenceSystem instance;

	// Famille
	private Family competence_f = FamilyManager.getFamily(new AllOfComponents(typeof(Competence)));

	// Variable
	public GameObject panelInfoComp;

	public ParamCompetenceSystem()
	{
		instance = this;
	}

	protected override void onStart()
	{
		// On d�sactive les comp�tences pas encore impl�ment�
		foreach(GameObject comp in competence_f)
        {
            if (!comp.GetComponent<Competence>().active)
            {
				comp.GetComponent<Toggle>().interactable = false;
			}
        }
		// On charge les donn�es pour chaque comp�tence
		try
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
					if (comp.name == data[0])
					{
						comp.transform.Find("Label").GetComponent<Text>().text = data[1];
						comp.GetComponent<Competence>().info = data[2];
					}
				}
			}
		}
        catch
        {
			Debug.Log("Erreur chargement fichier de parametrage des comp�tences");
        }


	}

	public void startLevel()
    {
		Debug.Log("Start level selon competence");
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
}