using UnityEngine;
using FYFY;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using FYFY_plugins.PointerManager;

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

public class EditableContainerSystem : FSystem 
{
	// Les familles
	private Family f_agent = FamilyManager.getFamily(new AllOfComponents(typeof(AgentEdit), typeof(ScriptRef))); // On r�cup�re les agents pouvant �tre �dit�
	private Family f_viewportContainerPointed = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(ViewportContainer))); // Les container contenant les container �ditable
	private Family f_scriptContainer = FamilyManager.getFamily(new AllOfComponents(typeof(UIRootContainer)), new AnyOfTags("ScriptConstructor")); // Les containers de scripts
	private Family f_refreshSize = FamilyManager.getFamily(new AllOfComponents(typeof(RefreshSizeOfEditableContainer)));
	private Family f_addSpecificContainer = FamilyManager.getFamily(new AllOfComponents(typeof(AddSpecificContainer)));
	
	// Les variables
	public GameObject agentSelected = null;
	private UIRootContainer containerSelected; // Le container selectionn�
	public GameObject EditableCanvas;
	public GameObject prefabViewportScriptContainer;

	// L'instance
	public static EditableContainerSystem instance;

	public EditableContainerSystem()
	{
		instance = this;
	}
	protected override void onStart()
	{
		MainLoop.instance.StartCoroutine(tcheckLinkName());
	}

    protected override void onProcess(int familiesUpdateCount)
    {
        if (f_refreshSize.Count > 0) // better to process like this than callback on family (here we are sure to process all components
        {
			// Update size of parent GameObject
			MainLoop.instance.StartCoroutine(setEditableSize());
			foreach(GameObject go in f_refreshSize)
				foreach (RefreshSizeOfEditableContainer trigger in go.GetComponents<RefreshSizeOfEditableContainer>())
					GameObjectManager.removeComponent(trigger);
		}
		if (f_addSpecificContainer.Count > 0)
			foreach (GameObject go in f_addSpecificContainer)
				foreach (AddSpecificContainer asc in go.GetComponents<AddSpecificContainer>())
				{
					addSpecificContainer(asc.name, asc.editState, asc.script);
					GameObjectManager.removeComponent(asc);
				}
    }

	// utilis� sur le OnSelect du ContainerName dans le prefab ViewportScriptContainer
    public void selectContainer(UIRootContainer container)
	{
		containerSelected = container;
	}

	// used on + button (see in Unity editor)
	public void addContainer()
	{
		addSpecificContainer();
		MainLoop.instance.StartCoroutine(syncEditableScrollBars());
	}

	// Rafraichit le nom des containers
	private void refreshUINameContainer()
	{
		MainLoop.instance.StartCoroutine(tcheckLinkName());
	}

	// Move editable view on the last editable container
	private IEnumerator syncEditableScrollBars()
	{
		// delay three times because we have to wait addSpecificContainer end (that call setEditableSize coroutine)
		yield return null;
		yield return null;
		yield return null;
		// move scroll bar on the last added container
		EditableCanvas.GetComponentInParent<ScrollRect>().verticalScrollbar.value = 1;
		EditableCanvas.GetComponentInParent<ScrollRect>().horizontalScrollbar.value = 1;
	}

	// Ajouter un container � la sc�ne
	private void addSpecificContainer(string name = "", AgentEdit.EditMode editState = AgentEdit.EditMode.Editable, List<GameObject> script = null)
	{
		// On clone le prefab
		GameObject cloneContainer = Object.Instantiate(prefabViewportScriptContainer);
		Transform editableContainers = EditableCanvas.transform.Find("EditableContainers");
		// On l'ajoute � l'�ditableContainer
		cloneContainer.transform.SetParent(editableContainers, false);
		// We secure the scale
		cloneContainer.transform.localScale = new Vector3(1, 1, 1);
		// On regarde combien de viewport container contient l'�ditable pour mettre le nouveau viewport � la bonne position
		cloneContainer.transform.SetSiblingIndex(EditableCanvas.GetComponent<EditableCanvacComponent>().nbViewportContainer);
		// Puis on imcr�mente le nombre de viewport contenue dans l'�ditable
		EditableCanvas.GetComponent<EditableCanvacComponent>().nbViewportContainer += 1;

		// Affiche le bon nom
		if (name != "")
		{
			// On d�finie son nom � celui de l'agent
			cloneContainer.GetComponentInChildren<UIRootContainer>().associedAgentName = name;
			// On affiche le bon nom sur le container
			cloneContainer.GetComponentInChildren<TMP_InputField>().text = name;
		}
		else
		{
			bool nameOk = false;
			for (int i = EditableCanvas.GetComponent<EditableCanvacComponent>().nbViewportContainer; !nameOk; i++)
			{
				// Si le nom n'est pas d�j� utilis� on nomme le nouveau container de cette fa�on
				if (!nameContainerUsed("Script" + i))
				{
					cloneContainer.GetComponentInChildren<UIRootContainer>().associedAgentName = "Script" + i;
					// On affiche le bon nom sur le container
					cloneContainer.GetComponentInChildren<TMP_InputField>().text = "Script" + i;
					nameOk = true;
				}
			}
			MainLoop.instance.StartCoroutine(tcheckLinkName());
		}

		// Si on est en mode Lock, on bloque l'�dition et on interdit de supprimer le script
		if (editState == AgentEdit.EditMode.Locked)
		{
			cloneContainer.GetComponentInChildren<TMP_InputField>().interactable = false;
			cloneContainer.transform.Find("ScriptContainer").Find("Header").Find("RemoveButton").GetComponent<Button>().interactable = false;
		}

		// ajout du script par d�faut
		GameObject dropArea = cloneContainer.GetComponentInChildren<ReplacementSlot>().gameObject;
		if (script != null && dropArea != null)
		{
			for (int k = 0; k < script.Count; k++)
				EditingUtility.addItemOnDropArea(script[k], dropArea);
			GameObjectManager.addComponent<NeedRefreshPlayButton>(MainLoop.instance.gameObject);
		}

		// On ajoute le nouveau viewport container � FYFY
		GameObjectManager.bind(cloneContainer);

		if (script != null && dropArea != null)
			// refresh all the hierarchy of parent containers
			GameObjectManager.addComponent<NeedRefreshHierarchy>(dropArea);

		// Update size of parent GameObject
		MainLoop.instance.StartCoroutine(setEditableSize());
	}

	private IEnumerator setEditableSize()
	{
		yield return null;
		yield return null;
		RectTransform editableContainers = (RectTransform)EditableCanvas.transform.Find("EditableContainers");
		// Resolve bug when creating the first editable component, it is the child of the verticalLayout but not included inside!!!
		// We just disable and enable it and force update rect
		if (editableContainers.childCount > 0)
		{
			editableContainers.GetChild(0).gameObject.SetActive(false);
			editableContainers.GetChild(0).gameObject.SetActive(true);
		}
		editableContainers.ForceUpdateRectTransforms();
		yield return null;
		// compute new size
		((RectTransform)EditableCanvas.transform.parent).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(215, editableContainers.rect.width));
	}

	// Empty the script window
	// See ResetButton in ViewportScriptContainer prefab in editor
	public void resetScriptContainer(bool refund = false)
	{
		// On r�cup�re le contenair point� lors du clic de la balayette
		GameObject scriptContainerPointer = f_viewportContainerPointed.First().transform.Find("ScriptContainer").gameObject;

		// On parcourt le script container pour d�truire toutes les instructions
		for (int i = scriptContainerPointer.transform.childCount - 1; i >= 0; i--)
			if (scriptContainerPointer.transform.GetChild(i).GetComponent<BaseElement>())
				GameObjectManager.addComponent<NeedToDelete>(scriptContainerPointer.transform.GetChild(i).gameObject);
		// Enable the last emptySlot and disable dropZone
		GameObjectManager.setGameObjectState(scriptContainerPointer.transform.GetChild(scriptContainerPointer.transform.childCount - 1).gameObject, true);
		GameObjectManager.setGameObjectState(scriptContainerPointer.transform.GetChild(scriptContainerPointer.transform.childCount - 2).gameObject, false);
	}

	// Remove the script window
	// See RemoveButton in ViewportScriptContainer prefab in editor
	public void removeContainer(GameObject container)
	{
		GameObjectManager.unbind(container);
		Object.Destroy(container);
		// Update size of parent GameObject
		MainLoop.instance.StartCoroutine(setEditableSize());
	}

	// return the container associated to the name. return null if no container with this name axists
	private UIRootContainer selectContainerByName(string name)
	{
		foreach (GameObject container in f_scriptContainer)
		{
			UIRootContainer uiContainer = container.GetComponent<UIRootContainer>();
			if (uiContainer.associedAgentName == name)
				return uiContainer;
		}

		return null;
	}

	// Rename the script window
	// See VontainmerName in ViewportScriptContainer prefab in editor
	public void newNameContainer(string newName)
	{
		string oldName = containerSelected.associedAgentName;
		if (oldName != newName)
		{
			// Si le nom n'est pas utilis�
			if (!nameContainerUsed(newName))
			{
				// On tente de r�cup�rer un agent li� � l'ancien nom
				AgentEdit linkedAgent = selectLinkedAgentByName(oldName);
				// Si l'agent existe, on met � jour son lien (on supprime le lien actuelle)
				if (linkedAgent)
                {
					// On annule la saisie si l'agent est locked ou s'il est synchro et que le nouveau nom choisi est un nom de container editable d�j� d�fini. En effet changer le nom du robot implique de changer aussi le nom du container mais attention il ne peut y avoir de doublons dans les noms des containers editables donc il faut s'assurer que le renommage du container editable a �t� accept� pour pouvoir valider le nouveau nom de l'agent.
					if (agentSelected.GetComponent<AgentEdit>().editState == AgentEdit.EditMode.Locked || (agentSelected.GetComponent<AgentEdit>().editState == AgentEdit.EditMode.Synch && nameContainerUsed(newName)))
					{ // on annule la saisie
						agentSelected.GetComponent<ScriptRef>().executablePanel.GetComponentInChildren<TMP_InputField>().text = agentSelected.GetComponent<AgentEdit>().agentName;
					}
					else
					{
						if (agentSelected.GetComponent<AgentEdit>().editState == AgentEdit.EditMode.Synch)
						{
							// On met � jours le nom de tous les agents qui auraient le m�me nom pour garder l'association avec le container editable
							foreach (GameObject agent in f_agent)
								if (agent.GetComponent<AgentEdit>().agentName == oldName)
								{
									agent.GetComponent<AgentEdit>().agentName = newName;
									agent.GetComponent<ScriptRef>().executablePanel.GetComponentInChildren<TMP_InputField>().text = newName;
								}
							// Puis on met � jour le nom du container �ditable
							UIRootContainer uiContainer = selectContainerByName(oldName);
							if (uiContainer != null)
							{
								containerSelected = uiContainer;
								newNameContainer(newName);
							}
						}
						else
						{
							// on ne modifie que l'agent selectionn�
							agentSelected.GetComponent<AgentEdit>().agentName = newName;
						}
						agentSelected.GetComponent<ScriptRef>().executablePanel.GetComponentInChildren<TMP_InputField>().text = newName;
					}

					// On v�rifie si on a une association avec les container �ditables
					refreshUINameContainer();
				}
				// On change pour son nouveau nom
				containerSelected.associedAgentName = newName;
				containerSelected.transform.Find("ContainerName").GetComponent<TMP_InputField>().text = newName;
			}
			else
			{ // Sinon on annule le changement
				containerSelected.transform.Find("ContainerName").GetComponent<TMP_InputField>().text = oldName;
			}
		}
		MainLoop.instance.StartCoroutine(tcheckLinkName());
	}

	// V�rifie si le nom propos� existe d�j� ou non pour un script container
	private bool nameContainerUsed(string nameTested)
	{
		// On regarde en premier lieu si le nom n'existe pas d�j�
		foreach (GameObject container in f_scriptContainer)
			if (container.GetComponent<UIRootContainer>().associedAgentName == nameTested)
				return true;

		return false;
	}

	// D�finie l'agent sur lequel les modifications seront opport�
	// Renvoie le composant AgentEdit de l'agent s�lectionn� s'il a �t� trouv�, sinon null
	private AgentEdit selectLinkedAgentByName(string nameAgent)
    {
		foreach (GameObject agent in f_agent)
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


	// V�rifie si les noms des containers correspond � un agent et vice-versa
	// Si non, Fait apparaitre le nom en rouge
	private IEnumerator tcheckLinkName()
	{
		yield return null;

		// On parcours les containers et si aucun nom ne correspond alors on met leur nom en gras rouge
		foreach (GameObject container in f_scriptContainer)
		{
			bool nameSame = false;
			foreach (GameObject agent in f_agent)
				if (container.GetComponent<UIRootContainer>().associedAgentName == agent.GetComponent<AgentEdit>().agentName)
					nameSame = true;

			// Si m�me nom trouver on met l'arri�re plan blanc
			if (nameSame)
				container.transform.Find("ContainerName").GetComponent<TMP_InputField>().image.color = Color.white;
			else // sinon rouge 
				container.transform.Find("ContainerName").GetComponent<TMP_InputField>().image.color = new Color(1f, 0.4f, 0.28f, 1f);
		}

		// On fait la m�me chose pour les agents
		foreach (GameObject agent in f_agent)
		{
			bool nameSame = false;
			foreach (GameObject container in f_scriptContainer)
				if (container.GetComponent<UIRootContainer>().associedAgentName == agent.GetComponent<AgentEdit>().agentName)
					nameSame = true;

			// Si m�me nom trouver on met l'arri�re transparent
			if (nameSame)
				agent.GetComponent<ScriptRef>().executablePanel.GetComponentInChildren<TMP_InputField>().image.color = new Color(1f, 1f, 1f, 1f);
			else // sinon rouge 
				agent.GetComponent<ScriptRef>().executablePanel.GetComponentInChildren<TMP_InputField>().image.color = new Color(1f, 0.4f, 0.28f, 1f);
		}
	}
}