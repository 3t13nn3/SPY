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
	private Family scriptContainer_f = FamilyManager.getFamily(new AllOfComponents(typeof(UITypeContainer))); // Les containers scripts
	private Family dropZone_f = FamilyManager.getFamily(new AllOfComponents(typeof(DropZoneComponent))); // Les drops zones
	private Family containerActionBloc_f = FamilyManager.getFamily(new AllOfComponents(typeof(UIActionType), typeof(ContainerActionBloc))); // Les blocs qui ne sont pas de base

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
			Dp.SetActive(value);
			Dp.transform.GetChild(0).gameObject.SetActive(false); // On est sur que les bares sont d�sactiv�es
		}
	}

	// Lors de la selection (d�but d'un drag) d'un block de la librairie
	// Cr�e un game object action = � l'action selectionn� dans librairie pour ensuite pouvoir le manipuler (durant le drag et le drop)
	public void beginDragElementFromLibrary(BaseEventData element)
    {
		// On verifie si c'est un up droit ou gauche
		if ((element as PointerEventData).button == PointerEventData.InputButton.Left)
		{
			// On cr�er le block action associ� � l'�l�ment
			creationActionBlock(element.selectedObject);
		}

		// On active les drops zone 
		dropZoneActivated(true);
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
				// Si l'�l�ment est dans un bloc sp�cial, on bouge le block enti�rement
                if(element.selectedObject.transform.parent.gameObject.GetComponent<ContainerActionBloc>().blockSpecial)
                {
                    // Si c'est un block condition d'op�rateur on bouge le block
                    if (element.selectedObject.transform.parent.gameObject.GetComponent<ContainerActionBloc>().containerCondition && element.selectedObject.transform.parent.gameObject.GetComponent<BaseCondition>())
                    {
						itemDragged = element.selectedObject.transform.parent.gameObject;
					}// sinon On bouge le container parent parent
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
			}
			if(itemDragged != null)
            {
				// On active les drops zone 
				dropZoneActivated(true);
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
			// On commence par regarder si il y a un container point� et sinon on supprime l'objet drag
			if (viewportContainerPointed_f.Count <= 0)
			{
				// remove item and all its children
				for (int i = 0; i < itemDragged.transform.childCount; i++)
					UnityEngine.Object.Destroy(itemDragged.transform.GetChild(i).gameObject);
				itemDragged.transform.DetachChildren();
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

		if (itemDragged != null)
		{
			/*
			// Si c'est un container qui ne doit pas contenir de script
			if (lastEditableContainer.GetComponent<UITypeContainer>().notScriptContainer)
			{
				// On associe l'element au container parent
				itemDragged.transform.SetParent(lastEditableContainer.transform.parent.transform);
				// On met l'�l�ment � la position voulue
				itemDragged.transform.SetSiblingIndex(redBar.transform.parent.parent.transform.GetSiblingIndex());
				GameObjectManager.refresh(itemDragged);
			}// Sinon si le container cible est le parent, alors l'index � prendre est la cible
			*/
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

            // Si l'�l�ment est lach� dans un container base condition
            // Il remplace le drop zone sur lequel il est drop
            if (lastEditableContainer.GetComponent<BaseCondition>())
            {
				// On desactive la zone
				redBar.transform.parent.gameObject.SetActive(false);
			}


		}
	}



	// Recr�er la taille du contenaire (action) selon le nombre d'�l�ment qu'il contient
	private void resizeContainerActionBloc(GameObject element)
    {
		Debug.Log("Resize bloc name : " + element.name);
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
		Debug.Log("Name element : " + element.name);
		Debug.Log("Name element prefab associate : " + element.GetComponent<ElementToDrag>().actionPrefab.name);
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
	}


	// Supprime l'elementd
	public void deleteElement(GameObject element)
	{
		// On note l'action point�
		GameObject actionPointed = null;
		if(actionPointed_f.Count > 0)
        {
			actionPointed = actionPointed_f.getAt(actionPointed_f.Count - 1); actionPointed_f.getAt(actionPointed_f.Count - 1);
			Debug.Log(actionPointed.name);
		}

		// On v�rifie qu'il y a bien un objet point� pour la suppression
		if(actionPointed != null)
        {
			GameObjectManager.addComponent<ResetBlocLimit>(actionPointed);
			UISystem.instance.startUpdatePlayButton();

			if (!actionPointed.GetComponent<UIActionType>().container)
			{
				foreach (GameObject bloc in containerActionBloc_f)
				{
					MainLoop.instance.StartCoroutine(waitForResizeContainer(bloc));
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

}