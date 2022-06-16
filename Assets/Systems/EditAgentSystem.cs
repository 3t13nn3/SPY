using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.EventSystems;

/// Ce syst�me g�re tous les �l�ments d'�ditions des agents par l'utilisateur.
/// Il g�re entre autre:
///		Le changement de nom du robot
///		Le changement automatique (si activ�) du nom du container associ� (si container associ�)
///		Le changement automatique (si activ�) du nom du robot lorsque l'on change le nom dans le container associ� (si container associ�)
/// 
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
///		
/// </summary>

public class EditAgentSystem : FSystem 
{
	// Les familles
	private Family agent_f = FamilyManager.getFamily(new AllOfComponents(typeof(AgentEdit), typeof(ScriptRef))); // On r�cup�re les agents pouvant �tre �dit�

	// Les variables
	public GameObject agentSelected = null;

	// L'instance
	public static EditAgentSystem instance;

	public EditAgentSystem()
	{
		instance = this;
	}

	// Utilis� principalement par les syst�mes ext�rieurs
	// D�finie l'agent sur lequel les modifications seront opport�
	// Renvoie le composant AgentEdit de l'agent s�lectionn� s'il a �t� trouv�, sinon null
	public AgentEdit selectLinkedAgentByName(string nameAgent)
    {
		foreach (GameObject agent in agent_f)
        {
			AgentEdit ae = agent.GetComponent<AgentEdit>();
			if (ae.agentName == nameAgent && ae.editState == AgentEdit.EditMode.Synch)
            {
				agentSelected = agent;
				return agentSelected.GetComponent<AgentEdit>();
			}
        }
		return null;
	}


	// Associe le nouveau nom re�ue � l'agent selectionn�
	// Met � jours son affichage dans �a fiche
	// Met � jours le lien qu'il a avec le script container du m�me nom
	public void setAgentName(string newName)
    {
		string oldName = agentSelected.GetComponent<AgentEdit>().agentName;

		if (agentSelected.GetComponent<AgentEdit>().editState != AgentEdit.EditMode.Locked && newName != oldName)
        {
			// On annule la saisie si l'agent est locked ou s'il est synchro et que le nouveau nom choisi est un nom de container editable d�j� d�fini. En effet changer le nom du robot implique de changer aussi le nom du container mais attention il ne peut y avoir de doublons dans les noms des containers editables donc il faut s'assurer que le renommage du container editable a �t� accept� pour pouvoir valider le nouveau nom de l'agent.
			if (agentSelected.GetComponent<AgentEdit>().editState == AgentEdit.EditMode.Locked || (agentSelected.GetComponent<AgentEdit>().editState == AgentEdit.EditMode.Synch && UISystem.instance.nameContainerUsed(newName)))
            { // on annule la saisie
				agentSelected.GetComponent<ScriptRef>().uiContainer.GetComponentInChildren<TMP_InputField>().text = agentSelected.GetComponent<AgentEdit>().agentName;
			}
			else
			{
				if (agentSelected.GetComponent<AgentEdit>().editState == AgentEdit.EditMode.Synch)
				{
					// On met � jours le nom de tous les agents qui auraient le m�me nom pour garder l'association avec le container editable
					foreach (GameObject agent in agent_f)
						if (agent.GetComponent<AgentEdit>().agentName == oldName)
						{
							agent.GetComponent<AgentEdit>().agentName = newName;
							agent.GetComponent<ScriptRef>().uiContainer.GetComponentInChildren<TMP_InputField>().text = newName;
						}
					// Puis on demande la mise � jour du nom du container �ditable
					UISystem.instance.setContainerName(oldName, newName);
				}
                else
				{
					// on ne modifie que l'agent selectionn�
					agentSelected.GetComponent<AgentEdit>().agentName = newName;
				}
				agentSelected.GetComponent<ScriptRef>().uiContainer.GetComponentInChildren<TMP_InputField>().text = newName;
			}

			// On v�rifie si on a une association avec les container �ditables
			UISystem.instance.refreshUINameContainer();
		}
	}
}