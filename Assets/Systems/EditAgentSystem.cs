using UnityEngine;
using FYFY;

public class EditAgentSystem : FSystem {
	// Systeme qui permet de gerer tous ce que l'on peux �diter, faire varier sur l'agent
	// Actuellement le nom

	public static EditAgentSystem instance;

	// On r�cup�re les agents pouvant �tre �dit�
	private Family agent_f = FamilyManager.getFamily(new AnyOfComponents(typeof(AgentName)));

	// Pour voir si le nom de l'agent change
	private string oldNameAgent = "";

	//GameData object need for param level
	public GameObject gameData;

	public EditAgentSystem()
	{
		if(Application.isPlaying)
		{
			//Si l'objet gameData et vide on le cherche dans la sc�ne
			if(gameData == null)
            {
				gameData = GameObject.Find("GameData");
				// Si pas d'objet GameData dans la sc�ne, on affiche un message d'erreur
				if(gameData == null)
                {
					//Affiche un message d'erreur et une validation permettant de revenir � l'�cran titre
				}
			}

			// Si la comp�tence nameObject n'est pas activ� on donne un nom aux agents
			if (!gameData.GetComponent<CompetenceActive>().nameObject)
            {
				int nbAgent = 1;
                foreach (GameObject agent_go in agent_f)
                {
					agent_go.GetComponent<AgentName>().agentName = "Agent" + nbAgent;

					nbAgent++;
				}
            }
		}
		instance = this;
	}

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
	}
}