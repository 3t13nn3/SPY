using UnityEngine;
using FYFY;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

/// <summary>
/// Manage InGame UI (Play/Pause/Stop, reset, go back to main menu...)
/// Switch to edition/execution view
/// Need to be binded after LevelGenerator
/// </summary>
public class UISystem : FSystem {
	private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Player"));
	private Family f_currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(BasicAction), typeof(LibraryItemRef), typeof(CurrentAction)));
	private Family f_agents = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef)));
	private Family f_viewportContainer = FamilyManager.getFamily(new AllOfComponents(typeof(ViewportContainer))); // Les containers viewport
	private Family f_scriptContainer = FamilyManager.getFamily(new AllOfComponents(typeof(UIRootContainer)), new AnyOfTags("ScriptConstructor")); // Les containers de scripts
	private Family f_resetButton = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("ResetButton")); // Les petites balayettes de chaque panneau d'édition
	private Family f_removeButton = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("RemoveButton")); // Les petites poubelles de chaque panneau d'édition

	private Family f_newEnd = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
	private Family f_updateStartButton = FamilyManager.getFamily(new AllOfComponents(typeof(NeedRefreshPlayButton)));

	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

	private Family f_enabledinventoryBlocks = FamilyManager.getFamily(new AllOfComponents(typeof(ElementToDrag)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	// Test 
	private Family f_coeur = FamilyManager.getFamily(new AnyOfTags("Coeur"));

	private GameData gameData;

	public GameObject buttonExecute;
	public GameObject buttonPause;
	public GameObject buttonNextStep;
	public GameObject buttonContinue;
	public GameObject buttonSpeed;
	public GameObject buttonStop;
	public GameObject menuEchap;
	public GameObject canvas;
	public GameObject libraryPanel;
	
	//xAPI statement
	//public static string allActionExecuted = "";
	//public static string actionExecuted = "";
	public static string objectType = "level";
	public static string verb = "played";
	public static string level;
	/////////
	
	public static UISystem instance;

	public UISystem(){
		instance = this;
	}

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();
		
		f_currentActions.addEntryCallback(keepCurrentActionViewable);

		f_playingMode.addEntryCallback(delegate {
			copyEditableScriptsToExecutablePanels();
			setExecutionView(true);
		});

		f_editingMode.addEntryCallback(delegate {
			setExecutionView(false);
		});

		f_enabledinventoryBlocks.addEntryCallback(delegate { MainLoop.instance.StartCoroutine(forceLibraryRefresh()); });
		f_enabledinventoryBlocks.addExitCallback(delegate { MainLoop.instance.StartCoroutine(forceLibraryRefresh()); });

		f_newEnd.addEntryCallback(levelFinished);

		f_updateStartButton.addEntryCallback(delegate {
			MainLoop.instance.StartCoroutine(updatePlayButton());
			foreach (GameObject go in f_updateStartButton)
				foreach (NeedRefreshPlayButton need in go.GetComponents<NeedRefreshPlayButton>())
					GameObjectManager.removeComponent(need);
		});

		MainLoop.instance.StartCoroutine(forceLibraryRefresh());


	}

	// Lors d'une fin d'exécution de séquence, gére les différents éléments à ré-afficher ou si il faut sauvegarder la progression du joueur
	private void levelFinished(GameObject go)
	{
		// On réaffiche les différents panels pour la création de séquence
		setExecutionView(false);

		// En cas de fin de niveau
		if (go.GetComponent<NewEnd>().endType == NewEnd.Win)
		{
			// Hide library panel
			GameObjectManager.setGameObjectState(libraryPanel.transform.parent.parent.gameObject, false);
			// Hide menu panel
			GameObjectManager.setGameObjectState(buttonExecute.transform.parent.gameObject, false);
			// Inactive of each editable panel
			foreach (GameObject brush in f_resetButton)
				brush.GetComponent<Button>().interactable = false;
			foreach (GameObject trash in f_removeButton)
				trash.GetComponent<Button>().interactable = false;
			// Sauvegarde de l'état d'avancement des niveaux (niveau et étoile)
			PlayerPrefs.SetInt(gameData.levelToLoad.Item1, gameData.levelToLoad.Item2 + 1);
			PlayerPrefs.Save();
		}
		// for other end type, nothing to do more
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
        //Active/désactive le menu echap si on appuit sur echap
        if (Input.GetKeyDown(KeyCode.Escape))
        {
			setActiveEscapeMenu();
        }
	}


	// Active ou désactive le bouton play si il y a ou non des actions dans un container script
	private IEnumerator updatePlayButton()
	{
		yield return null;
		buttonExecute.GetComponent<Button>().interactable = false;
		foreach (GameObject container in f_scriptContainer)
		{
			if (container.GetComponentsInChildren<BaseElement>().Length > 0)
			{
				buttonExecute.GetComponent<Button>().interactable = true;
			}
		}
	}

	private IEnumerator forceLibraryRefresh()
    {
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(libraryPanel.GetComponent<RectTransform>());
	}

	// keep current executed action viewable in the executable panel
	private void keepCurrentActionViewable(GameObject go){
		if (go.activeInHierarchy)
		{
			Vector3 v = GetGUIElementOffset(go.GetComponent<RectTransform>());
			if (v != Vector3.zero)
			{ // if not visible in UI
				ScrollRect containerScrollRect = go.GetComponentInParent<ScrollRect>();
				containerScrollRect.content.localPosition += GetSnapToPositionToBringChildIntoView(containerScrollRect, go.GetComponent<RectTransform>());
			}
		}
	}

	public Vector3 GetSnapToPositionToBringChildIntoView(ScrollRect scrollRect, RectTransform child){
		Canvas.ForceUpdateCanvases();
		Vector3 viewportLocalPosition = scrollRect.viewport.localPosition;
		Vector3 childLocalPosition   = child.localPosition;
		Vector3 result = new Vector3(
			0,
			0 - (viewportLocalPosition.y + childLocalPosition.y),
			0
		);
		return result;
	}

	public Vector3 GetGUIElementOffset(RectTransform rect){
        Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height);
        Vector3[] objectCorners = new Vector3[4];
        rect.GetWorldCorners(objectCorners);


		var xnew = 0f;
        var ynew = 0f;
        var znew = 0f;
 
        for (int i = 0; i < objectCorners.Length; i++){
			if (objectCorners[i].x < screenBounds.xMin)
                xnew = screenBounds.xMin - objectCorners[i].x;

            if (objectCorners[i].x > screenBounds.xMax)
                xnew = screenBounds.xMax - objectCorners[i].x;

            if (objectCorners[i].y < screenBounds.yMin)
                ynew = screenBounds.yMin - objectCorners[i].y;

            if (objectCorners[i].y > screenBounds.yMax)
                ynew = screenBounds.yMax - objectCorners[i].y;
				
        }
 
        return new Vector3(xnew, ynew, znew);
 
    }

	// On affiche ou non la partie librairie/programmation sequence en fonction de la valeur reçue
	public void setExecutionView(bool value){
		// Toggle library and editable panel
		GameObjectManager.setGameObjectState(canvas.transform.Find("LeftPanel").gameObject, !value);
		// Toggle all execution panels
		foreach (Transform executablePanel in canvas.transform.Find("ExecutableCanvas"))
			GameObjectManager.setGameObjectState(executablePanel.gameObject, value);
		// Define Menu button states
		GameObjectManager.setGameObjectState(buttonExecute, !value);
		GameObjectManager.setGameObjectState(buttonPause, value);
		GameObjectManager.setGameObjectState(buttonNextStep, false);
		GameObjectManager.setGameObjectState(buttonContinue, false);
		GameObjectManager.setGameObjectState(buttonSpeed, value);
		GameObjectManager.setGameObjectState(buttonStop, value);


		/*
		foreach (GameObject go in f_coeur)
		{	Debug.Log(" test");
			GameObjectManager.setGameObjectState(go, false);
		}*/

		if (gameData.actionsHistory != null)
			foreach (GameObject trash in f_removeButton)
				trash.GetComponent<Button>().interactable = false;
	
	}

	// Permet de relancer le niveau au début
	public void restartScene(){
		initZeroVariableLevel();
		GameObjectManager.loadScene("MainScene");
	}


	// See TitleScreen and MainMenu buttons in editor
	// Permet de revenir à la scéne titre
	public void returnToTitleScreen(){
		initZeroVariableLevel();
		gameData.actionsHistory = null;
		GameObjectManager.loadScene("TitleScreen");
	}


	// Permet de réinitialiser les variables du niveau dans l'objet gameData
	public void initZeroVariableLevel()
    {
		gameData.totalActionBlocUsed = 0;
		gameData.totalStep = 0;
		gameData.totalExecute = 0;
		gameData.totalCoin = 0;
		gameData.levelToLoadScore = null;
		gameData.actionExecutedPerAttempt = "";
		gameData.allActionExecuted = "";

		gameData.healthPoints = 3;
	

		gameData.dialogMessage = new List<(string, float, string, float)>();
		resetGameData();
	}

	private int computeNextLevelIndex(int currentLevelIndex)
	{
		int getLevelNumber(string s)
		{
			string pattern = @"\d+";
			Match match = Regex.Match(s, pattern);
			return int.Parse(match.Value);
		}

		List<string> levelLists = gameData.levelList[gameData.levelToLoad.Item1];
		int currentLevelNumber = getLevelNumber(levelLists[currentLevelIndex].Split("Niveau")[1]);
		int currentDifficulty = 3;
		if (levelLists[currentLevelIndex].Contains("_"))
			currentDifficulty = int.Parse(levelLists[currentLevelIndex].Split("_")[1].First().ToString());
		int nextLevelNumber = currentLevelNumber + 1;
		
		var targetLevels = levelLists
			.Select((l, i) => new { LevelName = l, Index = i })
			.Where(l => l.LevelName.Contains("Niveau"+nextLevelNumber.ToString()))
			.ToList();
		// Debug.Log("target levels :");
		// foreach(var level in targetLevels)
		// {
		// 	Debug.Log(level);
		// }
		if (targetLevels.Count == 0)
			return currentLevelIndex + 1;
		else if (targetLevels.Count == 1)
			return targetLevels[0].Index;
		else
		{
			List<string> previousLevelNumbers = new List<string>();
			for (int i = Math.Max(currentLevelNumber - 2, 1); i <= currentLevelNumber; i++)
				previousLevelNumbers.Add("Niveau"+i.ToString());
			// Debug.Log("previous :");
			// foreach(string level in previousLevelNumbers)
			// {
			// 	Debug.Log(level);
			// }
			Dictionary<int, int> stars = new Dictionary<int, int>();
			for (int i = 0; i < levelLists.Count; i++)
			{
				if (previousLevelNumbers.Any(s => levelLists[i].Contains(s)) &&
					PlayerPrefs.GetInt(gameData.levelToLoad.Item1 + Path.DirectorySeparatorChar + i + gameData.scoreKey, 0) != 0)
				{
					stars[getLevelNumber(levelLists[i].Split("Niveau")[1])] = PlayerPrefs.GetInt(gameData.levelToLoad.Item1 + Path.DirectorySeparatorChar + i + gameData.scoreKey);
				}
			}
			
			float avgStars = 0.0f;
			foreach (KeyValuePair<int, int> item in stars)
				avgStars += item.Value;
			avgStars /= 3;
			
			int nextLevelIndex = 0;
			foreach(var level in targetLevels)
			{
				if (avgStars >= int.Parse(level.LevelName.Split("_")[1].First().ToString()))
					nextLevelIndex = level.Index;
			}
			return nextLevelIndex;
		}
	}


	// See NextLevel button in editor
	// On charge la scéne suivante
	public void nextLevel(){
		// On cherche le prochain niveau à lancer
		int currentLevelIndex = gameData.levelToLoad.Item2;
		// Debug.Log("next level : " + computeNextLevelIndex(currentLevelIndex));
		gameData.levelToLoad.Item2 = computeNextLevelIndex(currentLevelIndex);
		
		// On efface l'historique
		gameData.actionsHistory = null;
		// On recharge la scéne (mais avec le nouveau numéro de niveau)
		restartScene();
	}

	// Reset les données du gameData pour la gestion des fonctionalités dans les niveaux
	private void resetGameData()
	{
		gameData.GetComponent<GameData>().executeLvlByComp = false;
	}


	// See ReloadLevel and RestartLevel buttons in editor
	// Fait recommencer la scéne mais en gardant l'historique des actions
	public void retry(){
		if (gameData.actionsHistory != null)
			UnityEngine.Object.DontDestroyOnLoad(gameData.actionsHistory);
		restartScene();
	}

	// Copie les blocs du panneau d'édition dans le panneau d'exécution
	private void copyEditableScriptsToExecutablePanels(){
		//clean container for each robot and copy the new sequence
		foreach (GameObject robot in f_player) {
			GameObject executableContainer = robot.GetComponent<ScriptRef>().executableScript;
			// Clean robot container
			for(int i = executableContainer.transform.childCount - 1; i >= 0; i--) {
				Transform child = executableContainer.transform.GetChild(i);
				GameObjectManager.unbind(child.gameObject);
				child.SetParent(null); // beacause destroying is not immediate, we remove this child from its parent, then Unity can take the time he wants to destroy GameObject
				GameObject.Destroy(child.gameObject);
			}

			//copy editable script
			GameObject editableContainer = null;
			// On parcourt les scripts containers pour identifer celui associé au robot 
			foreach (GameObject container in f_viewportContainer)
				// Si le container comporte le même nom que le robot
				if (container.GetComponentInChildren<UIRootContainer>().scriptName == robot.GetComponent<AgentEdit>().associatedScriptName)
					// On recupére le container qui contient le script à associer au robot
					editableContainer = container.transform.Find("ScriptContainer").gameObject;

			// Si on a bien trouvé un container associé
			if (editableContainer != null)
			{
				// we fill the executable container with actions of the editable container
				EditingUtility.fillExecutablePanel(editableContainer, executableContainer, robot.tag);
				// bind all child
				foreach (Transform child in executableContainer.transform)
					GameObjectManager.bind(child.gameObject);
				// On développe le panneau au cas où il aurait été réduit
				robot.GetComponent<ScriptRef>().executablePanel.transform.Find("Header").Find("Toggle").GetComponent<Toggle>().isOn = true;
			}
		}
		
		// On notifie les systèmes comme quoi le panneau d'éxecution est rempli
		GameObjectManager.addComponent<ExecutablePanelReady>(MainLoop.instance.gameObject);

		// On harmonise l'affichage de l'UI container des agents
		foreach (GameObject go in f_agents){
			LayoutRebuilder.ForceRebuildLayoutImmediate(go.GetComponent<ScriptRef>().executablePanel.GetComponent<RectTransform>());
			if(go.CompareTag("Player")){				
				GameObjectManager.setGameObjectState(go.GetComponent<ScriptRef>().executablePanel, true);				
			}
		}
	}

	// Permet d'activer ou désactiver le menu echap
	public void setActiveEscapeMenu()
    {
		// Si le menu est activé, le désactiver
        if (menuEchap.activeInHierarchy)
        {
			menuEchap.SetActive(false);
        }// Et inversement
        else
        {
			menuEchap.SetActive(true);
        }
    }

	// see inputFiels in ForBloc prefab in inspector
	public void onlyPositiveInteger(GameObject input, string newValue)
	{
		int res;
		bool success = Int32.TryParse(newValue, out res);
		if (!success || (success && Int32.Parse(newValue) < 0))
		{
			input.GetComponent<TMP_InputField>().text = "0";
			res = 0;
		}
		input.GetComponentInParent<ForControl>().nbFor = res;
	}
}