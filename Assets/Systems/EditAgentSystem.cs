using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 
/// agentSelect
///		Pour enregistrer sur qu'elle agents le syst�me va travaill�
///	modificationAgent
///		Pour les appel ext�rieurs, permet de trouver l'agent (et le consid�r� comme selectionn�) en fonction de son nom
///		Renvoie True si trouv�, sinon false
/// setAgentName
///		Pour changer le nom d'un agent
///	majDisplayCardAgent
///		Met � jour l'affichage des info de l'agent dans �a fiche
///	newScriptContainerLink
///		Supprime le lien existant avec le container Script actuelle et recherche le containe script identique au nouveau nom de l'agent
///		
/// </summary>

public class EditAgentSystem : FSystem 
{
	// Les familles
	private Family agent_f = FamilyManager.getFamily(new AllOfComponents(typeof(AgentEdit), typeof(ScriptRef))); // On r�cup�re les agents pouvant �tre �dit�
	private Family viewportContainer_f = FamilyManager.getFamily(new AllOfComponents(typeof(ViewportContainer))); // Les container contenant les container �ditable

	// Les variables
	private GameObject agentSelected = null;

	// L'instance
	public static EditAgentSystem instance;

	public EditAgentSystem()
	{
		instance = this;
	}


	// Enregistre l'agent selectionn� dans la variable agentSelected
	public void agentSelect(BaseEventData agent)
    {
		agentSelected = agent.selectedObject;
	}


	// Utilis� principalement par les syst�me ext�rieur
	// D�finie l'agent sur lequel les modifications seront opport�
	// Renvoie True si agent trouv�, sinon false
	public bool modificationAgent(string nameAgent)
    {
		foreach (GameObject agent in agent_f)
        {
			//Debug.Log("Nom agent : " + agent.GetComponent<AgentEdit>().agentName);
			if (agent.GetComponent<AgentEdit>().agentName == nameAgent)
            {
				agentSelected = agent;
				return true;
			}
        }
		return false;
	}


	// Associe le nouveau nom re�ue � l'agent selectionn�
	// Met � jours son affichage dans �a fiche
	// Met � jours le lien qu'il � avec le script container du m�me nom
	public void setAgentName(string newName)
    {
        if (agentSelected.GetComponent<AgentEdit>().editName)
        {
			// Si le changement de nom entre l'agent et le container est automatique, on change aussi le nom du container
			if (agentSelected.GetComponent<AgentEdit>().editNameAuto)
			{
				UISystem.instance.setContainerName(agentSelected.GetComponent<AgentEdit>().agentName, newName);
			}
			// On met � jours le nom de l'agent
			agentSelected.GetComponent<AgentEdit>().agentName = newName;
			// On met � jour l'affichage du nom dans le containe fiche de l'agent
			majDisplayCardAgent(newName);
			// On associe la fiche du l'agent au nouveau container
			newScriptContainerLink();
		}
	}

	// Met � jours l'affichage du nom de l'agent dans �a fiche
	public void majDisplayCardAgent(string newName)
    {
		agentSelected.GetComponent<ScriptRef>().uiContainer.GetComponentInChildren<TMP_InputField>().text = newName;
		UISystem.instance.refreshUI();
	}

	// Associe la fiche de l'agent au nouveau container (correspondant au nom de l'agent)
	public void newScriptContainerLink()
    {
		// On supprime le lien actuel
		agentSelected.GetComponent<ScriptRef>().scriptContainer = null;
		// On parcourt la liste des container pour y associer celui du m�me nom (si il existe)
		foreach (GameObject container in viewportContainer_f)
		{
			if (container.GetComponentInChildren<UITypeContainer>().associedAgentName == agentSelected.GetComponent<AgentEdit>().agentName)
			{
				agentSelected.GetComponent<ScriptRef>().scriptContainer = container.transform.Find("ScriptContainer").gameObject;
			}
		}
	}

}