using UnityEngine;
using FYFY;
using TMPro;
using FYFY_plugins.PointerManager;

/// <summary>
/// This system manages blocs limitation in inventory
/// </summary>
public class BlocLimitationManager : FSystem
{
	private Family f_actions = FamilyManager.getFamily(new AllOfComponents(typeof(PointerSensitive), typeof(LibraryItemRef)));
	private Family f_droppedActions = FamilyManager.getFamily(new AllOfComponents(typeof(Dropped), typeof(LibraryItemRef)));
	private Family f_deletedActions = FamilyManager.getFamily(new AllOfComponents(typeof(AddOne), typeof(ElementToDrag)));
	private Family f_draggableElement = FamilyManager.getFamily(new AllOfComponents(typeof(ElementToDrag)));
	private Family f_resetBlocLimit = FamilyManager.getFamily(new AllOfComponents(typeof(ResetBlocLimit)));

	private GameData gameData;

	protected override void onStart()
	{
		GameObject gd = GameObject.Find("GameData");
		if (gd != null)
		{
			gameData = gd.GetComponent<GameData>();
			// init limitation counters for each draggable elements
			foreach (GameObject go in f_draggableElement)
			{
				// default => hide go
				GameObjectManager.setGameObjectState(go, false);
				// update counter and enable required blocks
				updateBlocLimit(go);
			}
		}

		f_resetBlocLimit.addEntryCallback(delegate (GameObject go) {
			destroyScript(go, true);
			GameObjectManager.unbind(go);
			UnityEngine.Object.Destroy(go);
		});

		f_actions.addEntryCallback(linkTo);

		f_droppedActions.addEntryCallback(useAction);
		f_deletedActions.addEntryCallback(unuseAction);
	}

	//Recursive script destroyer
	private void destroyScript(GameObject go, bool refund = false)
	{
		if (go.GetComponent<LibraryItemRef>())
		{
			if (!refund)
				gameData.totalActionBloc++;
			else
				GameObjectManager.addComponent<AddOne>(go.GetComponent<LibraryItemRef>().linkedTo);
		}

		foreach (Transform child in go.transform)
			destroyScript(child.gameObject, refund);
	}

	// Find item in library to hook to this GameObject
	private void linkTo(GameObject go)
	{
		if (go.GetComponent<LibraryItemRef>().linkedTo == null)
		{
			if (go.GetComponent<BasicAction>())
				go.GetComponent<LibraryItemRef>().linkedTo = getLibraryItemByName(go.GetComponent<BasicAction>().actionType.ToString());
			else if (go.GetComponent<BaseCaptor>())
				go.GetComponent<LibraryItemRef>().linkedTo = getLibraryItemByName(go.GetComponent<BaseCaptor>().captorType.ToString());
			else if (go.GetComponent<BaseOperator>())
				go.GetComponent<LibraryItemRef>().linkedTo = getLibraryItemByName(go.GetComponent<BaseOperator>().operatorType.ToString());
			else if (go.GetComponent<WhileControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = getLibraryItemByName("While");
			else if (go.GetComponent<ForeverControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = getLibraryItemByName("Forever");
			else if (go.GetComponent<ForControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = getLibraryItemByName("For");
			else if (go.GetComponent<IfElseControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = getLibraryItemByName("IfElse");
			else if (go.GetComponent<IfControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = getLibraryItemByName("If");
		}
	}

	private GameObject getLibraryItemByName(string name)
	{
		foreach (GameObject item in f_draggableElement)
			if (item.name == name)
				return item;
		return null;
	}

	// Met � jour la limite du nombre de fois o� l'on peux utiliser un bloc (si il y a une limite)
	// Le d�sactive si la limite est atteinte
	// Met � jour le compteur
	private void updateBlocLimit(GameObject draggableGO){
		if (gameData.actionBlocLimit.ContainsKey(draggableGO.name))
		{
			bool isActive = gameData.actionBlocLimit[draggableGO.name] != 0; // negative means no limit
			GameObjectManager.setGameObjectState(draggableGO, isActive);
			if (isActive)
			{
				if (gameData.actionBlocLimit[draggableGO.name] < 0)
					// unlimited action => hide counter
					GameObjectManager.setGameObjectState(draggableGO.transform.GetChild(1).gameObject, false);
				else
				{
					// limited action => init and show counter
					GameObject counterText = draggableGO.transform.GetChild(1).gameObject;
					counterText.GetComponent<TextMeshProUGUI>().text = "Reste " + gameData.actionBlocLimit[draggableGO.name].ToString();
					GameObjectManager.setGameObjectState(counterText, true);
				}
			}
		}
	}

	// Remove one item from library
	private void useAction(GameObject go){
		LibraryItemRef lir = go.GetComponent<LibraryItemRef>();
		string actionKey = lir.linkedTo.name;
		if(actionKey != null && gameData.actionBlocLimit.ContainsKey(actionKey))
		{
			if (gameData.actionBlocLimit[actionKey] > 0)
				gameData.actionBlocLimit[actionKey] -= 1;
			updateBlocLimit(lir.linkedTo);		
		}
		GameObjectManager.removeComponent<Dropped>(go);
	}
	
	// Restore item in library
	private void unuseAction(GameObject go){
		AddOne[] addOnes =  go.GetComponents<AddOne>();
		if(gameData.actionBlocLimit.ContainsKey(go.name)){
			if (gameData.actionBlocLimit[go.name] >= 0)
				gameData.actionBlocLimit[go.name] += addOnes.Length;
			updateBlocLimit(go);
		}
		foreach(AddOne a in addOnes){
			GameObjectManager.removeComponent(a);	
		}
	}
}