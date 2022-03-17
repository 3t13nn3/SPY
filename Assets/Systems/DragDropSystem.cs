using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
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
	private Family scriptContainer_f = FamilyManager.getFamily(new AllOfComponents(typeof(UITypeContainer))); // Les containers scripts

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
	}


	// Lors de la selection (d�but d'un drag) d'un block de la sequence
	// l'en�lve de la hi�rarchie de la sequence d'action 
	public void beginDragElementFromEditableScript(BaseEventData element)
    {
		// On note le container utilis�
		lastEditableContainer = element.selectedObject.transform.parent.gameObject;

		// On verifie si c'est un up droit ou gauche
		if ((element as PointerEventData).button == PointerEventData.InputButton.Left)
		{
			// On enregistre l'objet sur lequel on va travailler le drag and drop dans le syst�me
			itemDragged = element.selectedObject;
			// On l'associe (temporairement) au Canvas Main
			GameObjectManager.setGameObjectParent(itemDragged, mainCanvas, true);
			itemDragged.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
			// exclude this GameObject from the EventSystem
			itemDragged.GetComponent<Image>().raycastTarget = false;
			if (itemDragged.GetComponent<BasicAction>())
				foreach (Image child in itemDragged.GetComponentsInChildren<Image>())
					child.raycastTarget = false;
			// Restore action and subactions to inventory
			foreach (BaseElement actChild in itemDragged.GetComponentsInChildren<BaseElement>())
				GameObjectManager.addComponent<AddOne>(actChild.gameObject);
			lastEditableContainer.transform.parent.GetComponentInParent<ScrollRect>().enabled = false;

			// Rend le bouton d'execution acitf (ou non)
			UISystem.instance.startUpdatePlayButton();
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
				UISystem.instance.refreshUI();
				// Suppression de l'item stocker en donn�e syst�me
				itemDragged = null;
				lastEditableContainer.transform.parent.parent.GetComponent<ScrollRect>().enabled = true;
			}
            else // sinon on ajoute l'�l�ment au container point�
            {
				GameObject container = viewportContainerPointed_f.First().transform.Find("ScriptContainer").gameObject;
				// On r�cup�re qu'elle container est pointer
				// Et on ajouter l'action � la fin du container �ditable
				dropElementInContainer(container.transform.Find("EndZoneActionBloc").Find("DropZone").gameObject);
			}
		}
	}


	// Place l'element dans la place cibl� (position de l'element associer au radar) du container editable
	public void dropElementInContainer(GameObject redBar)
	{
		// On note le container utilis�
		lastEditableContainer = redBar.transform.parent.parent.gameObject;


		if (itemDragged != null)
		{
			// On associe l'element au container
			GameObjectManager.setGameObjectParent(itemDragged, redBar.transform.parent.parent.gameObject, true);
			itemDragged.transform.SetParent(redBar.transform.parent.parent.gameObject.transform);
			// On met l'�l�ment � la position voulue
			itemDragged.transform.SetSiblingIndex(redBar.transform.parent.transform.GetSiblingIndex());
			// On le met � la taille voulue
			itemDragged.transform.localScale = new Vector3(1, 1, 1);
			// Pour r�activ� la selection posible
			itemDragged.GetComponent<Image>().raycastTarget = true;
			if (itemDragged.GetComponent<BasicAction>())
			{
				foreach (Image child in itemDragged.GetComponentsInChildren<Image>())
				{
					child.raycastTarget = true;
				}
			}

			// update limit bloc
			foreach (BaseElement actChild in itemDragged.GetComponentsInChildren<BaseElement>())
				GameObjectManager.addComponent<Dropped>(actChild.gameObject);

			if (itemDragged.GetComponent<UITypeContainer>())
				itemDragged.GetComponent<Image>().raycastTarget = true;

			// Lance le son de d�p�t du block d'action
			audioSource.Play();

			UISystem.instance.startUpdatePlayButton();
			itemDragged = null;
			lastEditableContainer.transform.parent.parent.GetComponent<ScrollRect>().enabled = true;
			UISystem.instance.refreshUI();
		}
	}


	// On cr�er l'action block en fonction de l'element re�u
	private void creationActionBlock(GameObject element)
    {
		// On r�cup�re le pref fab associ� � l'action de la libriaire
		GameObject prefab = element.GetComponent<ElementToDrag>().actionPrefab;
		// Create a dragged GameObject
		itemDragged = UnityEngine.Object.Instantiate<GameObject>(prefab, element.transform);
		BaseElement action = itemDragged.GetComponent<BaseElement>();
		itemDragged.GetComponent<UIActionType>().linkedTo = element;
		// On l'ajoute au famille de FYFY
		GameObjectManager.bind(itemDragged);
		// exclude this GameObject from the EventSystem
		itemDragged.GetComponent<Image>().raycastTarget = false;
		if (itemDragged.GetComponent<BasicAction>())
			foreach (Image child in itemDragged.GetComponentsInChildren<Image>())
				child.raycastTarget = false;
	}


	// Supprime l'element
	public void deleteElement(GameObject element)
    {
		GameObjectManager.addComponent<ResetBlocLimit>(actionPointed_f.getAt(actionPointed_f.Count - 1));
		UISystem.instance.startUpdatePlayButton();
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