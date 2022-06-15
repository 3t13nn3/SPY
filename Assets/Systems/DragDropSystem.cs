using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

/// Ce syst�me permet la gestion du drag and drop des diff�rents �l�ments de construction de la s�quence d'action.
/// Il g�re entre autre :
///		Le drag and drop d'un �l�ment du panel librairie vers une s�quence d'action
///		Le drag and drop d'un �l�ment d'une s�quence d'action vers une s�quence d'action (la m�me ou autre)
///		Le drag and drop d'un �l�ment (libraie ou sequence d'action) vers l'ext�rieur (pour le supprimer)
///		Le clique droit sur un �l�ment dans une sequence d'action pour le supprimer
///		Le double click sur un �l�ment pour l'ajouter � la derni�re s�quence d'action utilis�
/// 
/// <summary>
/// waitForResizeContainer (IEnumerator)
///		Attendre un cycle d'update pour lancer le recalcule des tailles des containers (utilis� pour laisse � FYFY le temps de mettre � jours les familles)
/// dropZoneActivated
///		Permet d'activer ou d�sactiver toutes les drops zones
///	dropZoneContainerDragDesactived
///		En cas de drag and drop d'un container, permet de d�sactiver ses drops zones (uniquement les siennes)
/// beginDragElementFromLibrary
///		Pour le d�but du drag and drop d'un �l�ment venant de la librairie
/// beginDragElementFromEditableScript
///		Pour le d�but du drag and drop d'un �l�ment venant de la s�quence d'action en construction
/// dragElement
///		Pendant le drag d'un �l�ment
/// endDragElement
///		A la fin d'un drag and drop si l'�l�ment n'est pas lach� dans un container pour la cr�ation d'une s�quence
/// dropElementInContainer
///		A la fin d'un drag and drop si l'�l�ment est lach� dans un container pour la cr�ation d'une s�quence
///	resizeContainerActionBloc
///		Rearrange correctement la taille des containers (par copy)
/// creationActionBlock
///		Cr�ation d'un block d'action lors de la selection de l'element correspondant dans la librairie
/// deleteElement
///		Destruction d'une block d'action
/// clickLibraryElementForAddInContainer
///		Ajout d'une action dans la derni�re sequence d'action modifi�, correspondant � l'�l�ment ayant re�ue un double click.
/// tcheckDoubleClick
///		Regarde si un double click � eu lieu sur l'�l�ment auquel il est ratach�
/// </summary>

public class DragDropSystem : FSystem
{
	// Les familles
    private Family viewportContainerPointed_f = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(ViewportContainer))); // Les container contenant les container �ditable
	private Family actionPointed_f = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(UIActionType), typeof(Image)));  // Les block d'actions pointer
	//private Family actionPointed_f = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(Image)), new AnyOfComponents(typeof(UIActionType), typeof(BaseCondition))); // Les block d'actions, op�rateurs et �l�ments pointer
	private Family dropZone_f = FamilyManager.getFamily(new AllOfComponents(typeof(DropZoneComponent))); // Les drops zones
	private Family containerActionBloc_f = FamilyManager.getFamily(new AllOfComponents(typeof(UIActionType), typeof(ContainerActionBloc))); // Les blocs qui ne sont pas de base
	private Family endzone_f = FamilyManager.getFamily(new AllOfComponents(typeof(EndBlockScriptComponent))); // Les end zones

	// Les variables
	private GameObject itemDragged; // L'item (ici block d'action) en cours de drag
	public GameObject mainCanvas; // Le canvas principal
	public GameObject lastEditableContainer; // Le dernier container �dit� 
	public AudioSource audioSource; // Pour le son d'ajout de block
	public GameObject buttonPlay;
	//Pour la gestion du double click
	private float lastClickTime;
	public float catchTime;

	// L'instance
	public static DragDropSystem instance;

	public DragDropSystem()
    {
		instance = this;
		

	}


	// Besoin d'attendre l'update pour effectuer le recalcule de la taille des container
	private IEnumerator waitForResizeContainer(GameObject bloc)
	{
		yield return null;
		resizeContainerActionBloc(bloc);
	}


	// On active toutes les drop zone
	private void dropZoneActivated(bool value)
	{
		foreach (GameObject Dp in dropZone_f)
		{
			GameObjectManager.setGameObjectState(Dp, value);
			Dp.transform.GetChild(0).gameObject.SetActive(false); // On est sur que les bares sont d�sactiv�es
		}
	}

	// Lors de la selection (d�but d'un drag) d'un block de la librairie
	// Cr�e un game object action = � l'action selectionn� dans librairie pour ensuite pouvoir le manipuler (durant le drag et le drop)
	public void beginDragElementFromLibrary(BaseEventData element)
    {
		// On active les drops zone 
		dropZoneActivated(true);

		// On verifie si c'est un up droit ou gauche
		if ((element as PointerEventData).button == PointerEventData.InputButton.Left)
		{
			// On cr�er le block action associ� � l'�l�ment
			creationActionBlock(element.selectedObject);
		}

	}


	// Lors de la selection (d�but d'un drag) d'un block de la sequence
	// l'en�lve de la hi�rarchie de la sequence d'action 
	public void beginDragElementFromEditableScript(BaseEventData element)
    {
		// On verifie si c'est un up droit ou gauche et si ce n'est pas un drop bar
		if ((element as PointerEventData).button == PointerEventData.InputButton.Left)
		{
			// On note le container utilis�
			lastEditableContainer = element.selectedObject.transform.parent.gameObject;
			// On enregistre l'objet sur lequel on va travailler le drag and drop dans le syst�me
			// Si c'est un drop zone dans un bloc special, alors on selectionne le container
			if (element.selectedObject.name == "EndZoneActionBloc")
            {
				// Si l'�l�ment est dans un bloc sp�cial (for, if, operator), on selectionne le parent
                if(element.selectedObject.transform.parent.gameObject.GetComponent<ContainerActionBloc>().blockSpecial)
                {
                    // Si c'est un block condition et op�rateur on selectionne le parent direct comme �l�ment � drag and drop
                    if (element.selectedObject.transform.parent.gameObject.GetComponent<ContainerActionBloc>().containerCondition && element.selectedObject.transform.parent.gameObject.GetComponent<BaseCondition>())
                    {
						itemDragged = element.selectedObject.transform.parent.gameObject;
					}// sinon on selectionne le parent du parent comme �l�ment � drag and drop
                    else
                    {
						itemDragged = element.selectedObject.transform.parent.parent.gameObject;
					}
				}
                else
                {
					itemDragged = null;
				}
			}// Sinon c'est l'objet selectionn�
            else
            {
				itemDragged = element.selectedObject;
				// Si l'�l�ment s�l�tionner est dans un emplacement op�rator
				// Alors on r�active la dropzone
				if (itemDragged.transform.parent.GetComponent<ContainerActionBloc>().containerCondition && itemDragged.transform.parent.GetComponent<BaseCondition>())
                {
					activateEndZone(itemDragged);
				}
			}
			if(itemDragged != null)
            {
				// On active les drops zone 
				dropZoneActivated(true);

				// On active les zones possible pour l'�l�ment drag
				// Si l'�l�ment est un block d'action
				if (itemDragged.GetComponent<BaseElement>())
                {
					dropZoneActivateView(true, false);
				}
                else{
					dropZoneActivateView(true, true);
				}

				// On l'associe (temporairement) au Canvas Main
				GameObjectManager.setGameObjectParent(itemDragged, mainCanvas, true);
				itemDragged.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
				// exclude this GameObject from the EventSystem
				itemDragged.GetComponent<Image>().raycastTarget = false;
				//if (itemDragged.GetComponent<BasicAction>())
				foreach (Image child in itemDragged.GetComponentsInChildren<Image>())
					child.raycastTarget = false;
				// Restore action and subactions to inventory
				foreach (BaseElement actChild in itemDragged.GetComponentsInChildren<BaseElement>())
					GameObjectManager.addComponent<AddOne>(actChild.gameObject);
				//lastEditableContainer.transform.parent.GetComponentInParent<ScrollRect>().enabled = false;

				// Rend le bouton d'execution acitf (ou non)
				UISystem.instance.startUpdatePlayButton();
			}
		}
	}


	// Pendant le drag d'un block, permet de lui faire suivre le mouvement de la souris
	public void dragElement()
	{
		if(itemDragged != null) {
			itemDragged.transform.position = Input.mousePosition;
		}
	}


	// Determine si l'element associer � l'�venement Pointer Up se trouv� dans une zone de container ou non
	// D�truite l'objet si pas dans un container, sinon rien
	public void endDragElement()
	{
		if (itemDragged != null)
		{
			dropZoneActivateView(false);

			// On commence par regarder si il y a un container point� et sinon on supprime l'objet drag
			if (viewportContainerPointed_f.Count <= 0)
			{
				/*
				// remove item and all its children
				for (int i = 0; i < itemDragged.transform.childCount; i++)
					UnityEngine.Object.Destroy(itemDragged.transform.GetChild(i).gameObject);
				itemDragged.transform.DetachChildren();
				*/
				// Suppresion des famille de FYFY
				GameObjectManager.unbind(itemDragged);
				// D�struction du block
				UnityEngine.Object.Destroy(itemDragged);

				// Rafraichissement de l'UI
				UISystem.instance.refreshUIButton();
				// Suppression de l'item stocker en donn�e syst�me
				itemDragged = null;
				//lastEditableContainer.transform.parent.parent.GetComponent<ScrollRect>().enabled = true;
				// On d�sactive les drop zone
				dropZoneActivated(false);
			}
            else // sinon on ajoute l'�l�ment au container point�
            {
				GameObject container = viewportContainerPointed_f.First().transform.Find("ScriptContainer").gameObject;
				// On r�cup�re qu'elle container est pointer
				// Et on ajouter l'action � la fin du container �ditable
				dropElementInContainer(container.transform.Find("EndZoneActionBloc").Find("DropZone").gameObject);
			}

			// Remmettre � jour l'association du scrollbar avec le container le plus grand  
		}
	}


	// Place l'element dans la place cibl� (position de l'element associer au radar) du container editable
	public void dropElementInContainer(GameObject redBar)
	{
		dropZoneActivateView(false);

		// Variable pour valider si c'est le bon block dans le bon type de container
		bool goodTypeBlock = false;
		// On note le container utilis�
		// Choisis le container � associer en fonction des renseignant dans DropZoneComponent
        if (redBar.GetComponent<DropZoneComponent>().parentTarget)
        {
			GameObject target = redBar.GetComponent<DropZoneComponent>().target;
			lastEditableContainer = target.transform.parent.gameObject;
		}
        else
        {
			lastEditableContainer =  redBar.GetComponent<DropZoneComponent>().target;
		}

        // On v�rifie que le type de l'�l�ment est compatible avec le container
        //Si c'est de type action
        if (itemDragged.GetComponent<BaseElement>())
        {
            // Si le container n'est pas bien un container condition
            if (!lastEditableContainer.GetComponent<ContainerActionBloc>().containerCondition)
            {
				goodTypeBlock = true;
			}
        }
		else if (itemDragged.GetComponent<BaseCondition>())
        {
			// Si le container est bien un container condition
			if (lastEditableContainer.GetComponent<ContainerActionBloc>().containerCondition)
			{
				goodTypeBlock = true;
			}
		}

		// Si on a bien un item et qu'il est paus� dans le bon block
		if (itemDragged != null && goodTypeBlock)
		{
			if (redBar.GetComponent<DropZoneComponent>().parentTarget)
            {
				// On associe l'element au container parent
				itemDragged.transform.SetParent(lastEditableContainer.transform);
				// On met l'�l�ment � la position voulue
				itemDragged.transform.SetSiblingIndex(redBar.GetComponent<DropZoneComponent>().target.transform.GetSiblingIndex());
				GameObjectManager.refresh(itemDragged);
			}// Sinon l'index � prendre est le parent de la drop zone
            else
            {
				// On associe l'element au container
				itemDragged.transform.SetParent(lastEditableContainer.transform);
				// On met l'�l�ment � la position voulue
				itemDragged.transform.SetSiblingIndex(redBar.transform.parent.transform.GetSiblingIndex()); 
				GameObjectManager.refresh(itemDragged);
			}
			// On le met � la taille voulue
			itemDragged.transform.localScale = new Vector3(1, 1, 1);
			// Pour r�activ� la selection posible
			itemDragged.GetComponent<Image>().raycastTarget = true;
			//if (itemDragged.GetComponent<BasicAction>())
			//{
			foreach (Image child in itemDragged.GetComponentsInChildren<Image>())
				child.raycastTarget = true;
			//}

			// update limit bloc
			foreach (BaseElement actChild in itemDragged.GetComponentsInChildren<BaseElement>())
			{
				GameObjectManager.addComponent<Dropped>(actChild.gameObject);
			}

			if (itemDragged.GetComponent<UITypeContainer>())
            {
				itemDragged.GetComponent<Image>().raycastTarget = true;
			}

			// Lance le son de d�p�t du block d'action
			audioSource.Play();

			UISystem.instance.startUpdatePlayButton();
			itemDragged = null;
			mainCanvas.transform.Find("EditableCanvas").GetComponent<ScrollRect>().enabled = true;
			UISystem.instance.refreshUIButton();

			// On d�sactive les drop zone
			dropZoneActivated(false);
			/*
			// Si le container est un container d'un bloc d'action
			if (lastEditableContainer.GetComponent<UITypeContainer>().actionContainer)
            {
				resizeContainerActionBloc(lastEditableContainer.transform.parent.gameObject);
            }
			*/

			// Si l'�l�ment est lach� dans un container condition
			// Il remplace l'�l�mentsur lequel il est drop
			if (lastEditableContainer.GetComponent<ContainerActionBloc>().containerCondition)
			{
				// Si c'est une end zone, on la d�sactive
				if (redBar.transform.parent.gameObject.name == "EndZoneActionBloc")
				{
					// Sinon on suprime l'�l�ment qu'il remplace
					redBar.transform.parent.gameObject.SetActive(false);
				}
				else// Sinon on suprime l'�l�ment qu'il remplace
				{
					GameObjectManager.addComponent<ResetBlocLimit>(redBar.GetComponent<DropZoneComponent>().target);
				}
			}
		} // Sinon si pas le bon container on alerte l'utilisateur de son erreur
        else
        {
			Debug.Log("Dsl, ce block ne va pas dans ce type de container");
			GameObjectManager.addComponent<ResetBlocLimit>(itemDragged);
		}
		// Evite d'autre drag and drop par m�garde
		itemDragged = null;
	}



	// Recr�er la taille du contenaire (action) selon le nombre d'�l�ment qu'il contient
	private void resizeContainerActionBloc(GameObject element)
    {
		// On note le container dans lequel on � mis l'�l�ment
		GameObject parentBloc = element.transform.parent.gameObject;
		// On copy l'�l�ment
		GameObject copy = UnityEngine.Object.Instantiate<GameObject>(element);
		// On l'ajoute � FYFY
		GameObjectManager.bind(copy);
		// On associe l'element au container
		copy.transform.SetParent(parentBloc.transform);
		// On met l'�l�ment � la position voulue
		copy.transform.SetSiblingIndex(element.transform.GetSiblingIndex());
		// On retir le element de FYFY
		GameObjectManager.unbind(element);
		// D�struction du element
		UnityEngine.Object.Destroy(element);
		// On oublie pas d'associer le nouveau container cr�er en tant que dernier container utilis�
		// (Car c'est l� que l'on a d�pos� le nouvel �l�ment)
		lastEditableContainer = copy.transform.Find("Container").gameObject;
	}


	// On cr�er l'action block en fonction de l'element re�u
	private void creationActionBlock(GameObject element)
    {
		// On r�cup�re le pref fab associ� � l'action de la libriaire
		GameObject prefab = element.GetComponent<ElementToDrag>().actionPrefab;
		// Create a dragged GameObject
		itemDragged = UnityEngine.Object.Instantiate<GameObject>(prefab, element.transform);
		//On l'attache au canvas pour le drag ou l'on veux
		itemDragged.transform.SetParent(mainCanvas.transform);
		// Si c'est un basic action
		if(itemDragged.GetComponent<Highlightable>() is BasicAction)
        {
			BaseElement action = itemDragged.GetComponent<BaseElement>();
			itemDragged.GetComponent<UIActionType>().linkedTo = element;
		}
		// On l'ajoute au famille de FYFY
		GameObjectManager.bind(itemDragged);
		// exclude this GameObject from the EventSystem
		itemDragged.GetComponent<Image>().raycastTarget = false;
		//if (itemDragged.GetComponent<BasicAction>())
		foreach (Image child in itemDragged.GetComponentsInChildren<Image>())
			child.raycastTarget = false;

		// On active les zones possible pour l'�l�ment drag
		// Si l'�l�ment est un block d'action
		if (itemDragged.GetComponent<BaseElement>())
		{
			dropZoneActivateView(true, false);
		}
		else
		{
			dropZoneActivateView(true, true);
		}
	}


	// Supprime l'element
	public void deleteElement(GameObject element)
	{
		// On note l'action point�
		GameObject actionPointed = null;
		if(actionPointed_f.Count > 0)
        {
			actionPointed = actionPointed_f.getAt(actionPointed_f.Count - 1); actionPointed_f.getAt(actionPointed_f.Count - 1);
		}

		// On v�rifie qu'il y a bien un objet point� pour la suppression
		if(actionPointed != null)
        {
			GameObject eleDel = null;
			// Si l'�l�ment est un end block, alors l'�l�ment � supprimer c'est le bloc entier
			if (actionPointed.GetComponent<EndBlockScriptComponent>())
            {
				eleDel = actionPointed.transform.GetComponent<EndBlockScriptComponent>().rootElement;
			}
            else // Sinon c'est l'�l�ment point�
            {
				eleDel = actionPointed;
			}
			// On demande la r�activation de la end zone (utile surtout pour les blocks de op�rator)
			activateEndZone(eleDel);
			//On associe � l'�l�ment le component ResetBlocLimit pour d�clancher le script de destruction de l'�l�ment
			GameObjectManager.addComponent<ResetBlocLimit>(eleDel);
			UISystem.instance.startUpdatePlayButton();

			if (!actionPointed.GetComponent<UIActionType>().container)
			{
				foreach (GameObject bloc in containerActionBloc_f)
				{
					// Commentaire temporaire
					//MainLoop.instance.StartCoroutine(waitForResizeContainer(bloc));
				}
			}
		}
	}


	// Si double click sur l'�l�ment, ajoute le block d'action au dernier container utilis�
	public void clickLibraryElementForAddInContainer(BaseEventData element)
    {
		if (tcheckDoubleClick())
		{
			// On cr�er le block action
			creationActionBlock(element.selectedObject);
			// On l'envoie vers l'editable container
			dropElementInContainer(lastEditableContainer.transform.Find("EndZoneActionBloc").Find("DropZone").gameObject);
		}
	}


	// V�rifie si le double click � eu lieu
	private bool tcheckDoubleClick()
	{
		//check double click
		// On met � jours le timer du dernier clique
		// et on retourne la r�ponse
		if (Time.time - lastClickTime < catchTime)
        {
			lastClickTime = Time.time;
			return true;
		}
        else
        {
			lastClickTime = Time.time;
			return false;
		}

	}

	// R�active une end zone situer juste en dessous de l'�l�ment re�ue en param�tre dans l'index du container
	// Principalement utilis� pour la gestion des dropzones des block op�rator
	private void activateEndZone(GameObject ele)
    {
		// On r�cup�re la place de l'item dans l'op�rator
		int index = ele.transform.GetSiblingIndex();
		// La place juste en dessous est utilis� par la dropzone, on l'active
		GameObject child = ele.transform.parent.GetChild(index + 1).gameObject;
		child.SetActive(true);
	}

	// Active la visualisation des zones ou l'on peux pauser l'�l�ment que l'on drag and drop
	// Grise les autres zones
	// Param�tre : 
	//   - calue : Activation de la zone ou non
	//   - condition : zone de d�p�t pour els conditions � activ� ou non
	private void dropZoneActivateView(bool value, bool condition = false)
    {
		// Si la value est vrai on active la visualisation des zones voulue
        if (value)
        {
			// On active la visualisation des drop zone de condition disponible et on grise les drop zone de cr�ation de sequence
            if (condition)
            {
				foreach (GameObject endZone in endzone_f)
				{
					if (endZone.transform.Find("DropZone").GetComponent<DropZoneComponent>().target.GetComponent<ContainerActionBloc>().containerCondition)
                    {
						//endZone.transform.Find("ViewContainerTrue").gameObject.SetActive(true);
						endZone.transform.GetComponent<Outline>().enabled = true;
					}
                    else
                    {
						//endZone.transform.Find("ViewContainerFalse").gameObject.SetActive(true);
						endZone.transform.GetComponent<Image>().color = new Color32(148, 148, 148, 255);
						endZone.transform.Find("DropZone").gameObject.SetActive(false);
					}
				}
            }// On fait l'inverse
            else
            {
				foreach (GameObject endZone in endzone_f)
				{
					if (endZone.transform.Find("DropZone").GetComponent<DropZoneComponent>().target.GetComponent<ContainerActionBloc>().containerCondition)
					{
						//endZone.transform.Find("ViewContainerFalse").gameObject.SetActive(true);
						//endZone.transform.Find("DropZone").gameObject.SetActive(false);
						endZone.transform.GetComponent<Image>().color = new Color32(148, 148, 148, 255);
						endZone.transform.Find("DropZone").gameObject.SetActive(false);
					}
					else
					{
						//endZone.transform.Find("ViewContainerTrue").gameObject.SetActive(true);
						endZone.transform.GetComponent<Outline>().enabled = true;
					}
				}
			}
        }// On d�sactive toutes les visualisations
        else
        {
			foreach (GameObject endZone in endzone_f)
			{
				//endZone.transform.Find("ViewContainerTrue").gameObject.SetActive(false);
				//endZone.transform.Find("ViewContainerFalse").gameObject.SetActive(false);
				endZone.transform.GetComponent<Image>().color = endZone.GetComponent<EndBlockScriptComponent>().baseColor;
				endZone.transform.GetComponent<Outline>().enabled = false;
			}
		}
    }

	// En fonction de la valeur, change l'�tat du oultine de l'element re�ue et desactive la red barre de l'�l�ment
	// Si value = false; desactive le outline de l'�l�ment
	// Si value = true; active le outline de l'�l�ment
	public void activeOutlineConditionContainer(GameObject element, bool value)
    {
		// Active ou desactive la outline de l'�l�ment
		element.GetComponent<Outline>().enabled = value;
		// Desactive la red barre de la drop box associ�
		element.GetComponent<Highlightable>().dropZoneChild.gameObject.transform.Find("PositionBar").gameObject.SetActive(false);

	}

}