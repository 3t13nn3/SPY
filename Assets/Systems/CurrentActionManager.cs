using System;
using UnityEngine;
using FYFY;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Data;

/// <summary>
/// Manage CurrentAction components, parse scripts and define first action, next actions, evaluate boolean expressions (if and while)...
/// </summary>
public class CurrentActionManager : FSystem
{
	private Family f_executionReady = FamilyManager.getFamily(new AllOfComponents(typeof(ExecutablePanelReady)));
	private Family f_ends = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
	private Family f_newStep = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private Family f_currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(BasicAction),typeof(LibraryItemRef), typeof(CurrentAction)));
	private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));

	private Family f_wall = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"));
	private Family f_drone = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Drone"));
	private Family f_door = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
	private Family f_redDetector = FamilyManager.getFamily(new AllOfComponents(typeof(Rigidbody), typeof(Detector), typeof(Position)));
	private Family f_activableConsole = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(AudioSource)));
	private Family f_exit = FamilyManager.getFamily(new AllOfComponents(typeof(Position), typeof(AudioSource)), new AnyOfTags("Exit"));

	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

	private Family f_trap = FamilyManager.getFamily(new AnyOfTags("Trap"));

	public static CurrentActionManager instance;
	
	private Dictionary<String, bool> ifWhileOpen = new Dictionary<string, bool>();

	public CurrentActionManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		f_executionReady.addEntryCallback(initFirstsActions);
		f_newStep.addEntryCallback(delegate { onNewStep(); });
		f_editingMode.addEntryCallback(delegate {
			// remove all player's current actions
			foreach (GameObject currentAction in f_currentActions)
				if (currentAction.GetComponent<CurrentAction>().agent.CompareTag("Player"))
					GameObjectManager.removeComponent<CurrentAction>(currentAction);
		});
		f_playingMode.addEntryCallback(delegate {
			// reset inaction counters
			foreach (GameObject robot in f_player)
				robot.GetComponent<ScriptRef>().nbOfInactions = 0;
		});
	}

	private void initFirstsActions(GameObject go)
	{
		// init first action if no ends occur (possible for scripts with bad condition)
		if (f_ends.Count <= 0)
		{
			// init currentAction on the first action of players
			bool atLeastOneFirstAction = false;
			foreach (GameObject player in f_player)
				if (addCurrentActionOnFirstAction(player) != null)
					atLeastOneFirstAction = true;
			if (!atLeastOneFirstAction)
			{
				GameObjectManager.addComponent<EditMode>(MainLoop.instance.gameObject);
			}
			else
			{
				// init currentAction on the first action of ennemies
				bool forceNewStep = false;
				foreach (GameObject drone in f_drone)
					if (!drone.GetComponent<ScriptRef>().executableScript.GetComponentInChildren<CurrentAction>() && !drone.GetComponent<ScriptRef>().scriptFinished)
						addCurrentActionOnFirstAction(drone);
					else
						forceNewStep = true; // will move currentAction on next action

				if (forceNewStep)
					onNewStep();
			}
		}

		GameObjectManager.removeComponent<ExecutablePanelReady>(go);
	}

	private GameObject addCurrentActionOnFirstAction(GameObject agent)
    {
		GameObject firstAction = null;
		// try to get the first action
		Transform container = agent.GetComponent<ScriptRef>().executableScript.transform;
		if (container.childCount > 0)
		{
			firstAction = getFirstActionOf(container.GetChild(0).gameObject, agent);
			Debug.Log("call getFirstActionOf in addCurrentActionOnFirstAction" + firstAction);
		}

		if (firstAction != null)
		{
			// Set this action as CurrentAction
			GameObjectManager.addComponent<CurrentAction>(firstAction, new { agent = agent });
		}
		

		return firstAction;
	}

	// get first action inside "action", it could be control structure (if, for...) => recursive call
	private GameObject getFirstActionOf(GameObject action, GameObject agent)
	{
		if (action == null)
			return null;
		bool var = false;
		if (action.GetComponent<BasicAction>())
		{
			
			//xAPI
			UISystem.allActionExecuted =
				UISystem.allActionExecuted + '-' + action.GetComponentInChildren<BasicAction>().actionType;
			UISystem.actionExecuted =
				UISystem.actionExecuted + '-' + action.GetComponentInChildren<BasicAction>().actionType;
			/////
			Debug.Log("in getFirstActionOf " + action.GetComponentInChildren<BasicAction>().actionType);
			return action;
		}
		else
		{
			// check if action is a IfControl
			if (action.GetComponent<IfControl>())
			{
				IfControl ifCont = action.GetComponent<IfControl>();
				IfElseControl ifElseCont = action.GetComponent<IfElseControl>();
				// check if this IfControl include a child and if condition is evaluated to true
				if (ifCont.firstChild != null && ifValid(ifCont.condition, agent)) {
					Debug.Log("if block");
					
					UISystem.allActionExecuted =
						UISystem.allActionExecuted + "-If-";
					UISystem.actionExecuted =
						UISystem.actionExecuted + "-If-";
					
					return getFirstActionOf(ifCont.firstChild, agent);
					
				}
				else if (ifElseCont &&
				         ifElseCont.firstChild != null)
				{
					Debug.Log("else block");
					UISystem.allActionExecuted =
						UISystem.allActionExecuted + "-Else-";
					UISystem.actionExecuted =
						UISystem.actionExecuted + "-Else-";
					return getFirstActionOf(action.GetComponent<IfElseControl>().elseFirstChild, agent);
				}
				else
				{
					// this if doesn't contain action or its condition is false => get first action of next action (could be if, for...)
					return getFirstActionOf(ifCont.next, agent);
				}
					
			}
			// check if action is a WhileControl
			else if (action.GetComponent<WhileControl>())
			{
				WhileControl whileCont = action.GetComponent<WhileControl>();
				// check if condition is evaluated to true
				if (ifValid(whileCont.condition, agent))
				{
					
					Debug.Log(whileCont.GetHashCode().ToString());
					if (!ifWhileOpen.ContainsKey(whileCont.GetHashCode().ToString()))
						ifWhileOpen[whileCont.GetHashCode().ToString()] = false;
					
					if (!ifWhileOpen[whileCont.GetHashCode().ToString()])
					{
						UISystem.allActionExecuted =
							UISystem.allActionExecuted + "-While-";
						UISystem.actionExecuted =
							UISystem.actionExecuted + "-While-";
						ifWhileOpen[whileCont.GetHashCode().ToString()] = true;
					}
					// get first action of its first child (could be if, for...)
					return getFirstActionOf(whileCont.firstChild, agent);
				}

				else
				{
					if (ifWhileOpen.ContainsKey(whileCont.GetHashCode().ToString()) && ifWhileOpen[whileCont.GetHashCode().ToString()])
					{
						ifWhileOpen[whileCont.GetHashCode().ToString()] = false;
						/////////xAPI
						UISystem.allActionExecuted =
							UISystem.allActionExecuted + "-EndWhile-";
						UISystem.actionExecuted =
							UISystem.actionExecuted + "-EndWhile-";
						//////////////
					}
					// this condition is false => get first action of next action (could be if, for...)
					return getFirstActionOf(whileCont.next, agent);
				}
					
			}
			// check if action is a ForControl
			else if (action.GetComponent<ForControl>())
			{
				ForControl forCont = action.GetComponent<ForControl>();
				// check if this ForControl include a child and nb iteration != 0 and end loop not reached
				if (forCont.firstChild != null && forCont.nbFor != 0 && forCont.currentFor < forCont.nbFor)
				{
					///////////xAPI 
					if (forCont.currentFor == 0)
					{
						Debug.Log("we add for in getfirstaction "+ forCont.firstChild);
						UISystem.allActionExecuted =
							UISystem.allActionExecuted + "-For-";
						UISystem.actionExecuted =
							UISystem.actionExecuted + "-For-";
					}
					///////////
					forCont.currentFor++;
					forCont.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = (forCont.currentFor).ToString() + " / " + forCont.nbFor.ToString();
					// get first action of its first child (could be if, for...)
					return getFirstActionOf(forCont.firstChild, agent);
				}
				else
				{
					if (forCont.currentFor >= forCont.nbFor)
					{
						///////////xAPI 
						//Debug.Log("we add for in getfirstaction " + "Endfor");
						UISystem.allActionExecuted =
							UISystem.allActionExecuted + "-EndFor-";
						UISystem.actionExecuted =
							UISystem.actionExecuted + "-EndFor-";
						///////////xAPI 
					}
					// this for doesn't contain action or nb iteration == 0 or end loop reached => get first action of next action (could be if, for...)
					return getFirstActionOf(forCont.next, agent);
				}
			}
			// check if action is a ForeverControl
			else if (action.GetComponent<ForeverControl>())
			{
				// always return firstchild of this ForeverControl
				return getFirstActionOf(action.GetComponent<ForeverControl>().firstChild, agent);
			}
		}
		return null;
	}

	// Return true if "condition" is valid and false otherwise
	private bool ifValid(List<string> condition, GameObject agent)
	{

		string cond = "";
		for (int i = 0; i < condition.Count; i++)
		{
			if (condition[i] == "(" || condition[i] == ")" || condition[i] == "OR" || condition[i] == "AND" || condition[i] == "NOT")
			{
				cond = cond + condition[i] + " ";
			}
			else
			{
				cond = cond + checkCaptor(condition[i], agent) + " ";
			}
		}

		DataTable dt = new DataTable();
		var v = dt.Compute(cond, "");
		bool result;
		try
		{
			result = bool.Parse(v.ToString());
		}
		catch
		{
			result = false;
		}
		return result;
	}

	// return true if the captor is true, and false otherwise
	private bool checkCaptor(string ele, GameObject agent)
	{

		bool ifok = false;
		// get absolute target position depending on player orientation and relative direction to observe
		// On commence par identifier quelle case doit �tre regard�e pour voir si la condition est respect�e
		Vector2 vec = new Vector2();
		switch (agent.GetComponent<Direction>().direction)
		{
			case Direction.Dir.North:
				vec = new Vector2(0, -1);
				break;
			case Direction.Dir.South:
				vec = new Vector2(0, 1);
				break;
			case Direction.Dir.East:
				vec = new Vector2(1, 0);
				break;
			case Direction.Dir.West:
				vec = new Vector2(-1, 0);
				break;
		}

		// check target position
		switch (ele)
		{
			case "Wall": // walls
				foreach (GameObject wall in f_wall)
					if (wall.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
					 wall.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y && wall.GetComponent<Renderer>().enabled)
						ifok = true;
				break;
			case "Trap": // trap 
				foreach (GameObject trap in f_trap)
					if (trap.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
					 trap.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
						ifok = true;
				break;
			case "FieldGate": // doors
				foreach (GameObject door in f_door)
					if (door.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
					 door.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
						ifok = true;
				break;
			case "Enemie": // ennemies
				foreach (GameObject drone in f_drone)
					if (drone.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
						drone.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
						ifok = true;
				break;
			case "Terminal": // consoles
				foreach (GameObject console in f_activableConsole)
				{
					vec = new Vector2(0, 0);
					if (console.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
						console.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
						ifok = true;
				}
				break;
			case "RedArea": // detectors
				foreach (GameObject detector in f_redDetector)
					if (detector.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
					 detector.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
						ifok = true;
				break;
			case "Exit": // exits
				foreach (GameObject exit in f_exit)
				{
					vec = new Vector2(0, 0);
					if (exit.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
					 exit.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
						ifok = true;
				}
				break;
		}
		return ifok;

	}

	// one step consists in removing the current actions this frame and adding new CurrentAction components next frame
	private void onNewStep(){
		GameObject nextAction;

	Debug.Log(" AHHHHHA Nouveau step ");

		foreach(GameObject currentActionGO in f_currentActions){
			CurrentAction currentAction = currentActionGO.GetComponent<CurrentAction>();
			Debug.Log(" call getNextAction in onNewStep");

			nextAction = getNextAction(currentActionGO, currentAction.agent);
			// check if we reach last action of a drone
			if(nextAction == null && currentActionGO.GetComponent<CurrentAction>().agent.CompareTag("Drone"))
				currentActionGO.GetComponent<CurrentAction>().agent.GetComponent<ScriptRef>().scriptFinished = true;
			else if(nextAction != null){
				//ask to add CurrentAction on next frame => this frame we will remove current CurrentActions
				MainLoop.instance.StartCoroutine(delayAddCurrentAction(nextAction, currentAction.agent));
			}
			GameObjectManager.removeComponent<CurrentAction>(currentActionGO);
		}
	}

	// return the next action to execute, return null if no next action available
	private GameObject getNextAction(GameObject currentAction, GameObject agent)
	{
		BasicAction current_ba = currentAction.GetComponent<BasicAction>();
		if (current_ba != null)
		{
			// if next is not defined or is a BasicAction we return it
			if (current_ba.next == null || current_ba.next.GetComponent<BasicAction>())
			{
				//xAPI
				UISystem.allActionExecuted =
					UISystem.allActionExecuted + '-' + current_ba.next.GetComponent<BasicAction>().actionType;
				UISystem.actionExecuted =
					UISystem.actionExecuted + '-' + current_ba.next.GetComponentInChildren<BasicAction>().actionType;
				/////
				//Debug.Log("in getNextActionOf " + current_ba.next.GetComponentInChildren<BasicAction>().actionType);
				return current_ba.next;
			}
			else
			{
				//Debug.Log("call getFirstActionOf in getNextAction =====> BasicAction");
				return getFirstActionOf(current_ba.next, agent);
			}
				
		}
		else if (currentAction.GetComponent<WhileControl>())
        {
			if(ifValid(currentAction.GetComponent<WhileControl>().condition, agent))
			{
				
	            if (currentAction.GetComponent<WhileControl>().firstChild == null || currentAction
		                .GetComponent<WhileControl>().firstChild.GetComponent<BasicAction>())
	            {
		            ///////////xAPI
		            if (currentAction.GetComponent<WhileControl>().firstChild != null)
		            {
			            UISystem.allActionExecuted =
				            UISystem.allActionExecuted + "-While-" + currentAction.GetComponent<WhileControl>()
					            .firstChild.GetComponent<BasicAction>().actionType +"-EndWhile-";
			            UISystem.actionExecuted =
				            UISystem.actionExecuted + "-While-" + currentAction.GetComponent<WhileControl>().firstChild
					            .GetComponent<BasicAction>().actionType +"-EndWhile-";
			            ////////////////
		            }

		            return currentAction.GetComponent<WhileControl>().firstChild;
		            }
	            else
	            {
		            //Debug.Log("call getFirstActionOf in getNextAction ====> WhileControl return firstchild");
		            return getFirstActionOf(currentAction.GetComponent<WhileControl>().firstChild, agent);
	            }
		            
			}
            else
            {
	            if (currentAction.GetComponent<WhileControl>().next == null ||
	                currentAction.GetComponent<WhileControl>().next.GetComponent<BasicAction>())
	            {
		            ///////////xAPI
		            if (currentAction.GetComponent<WhileControl>().next != null)
		            {
			            UISystem.allActionExecuted =
			            UISystem.allActionExecuted + "-" + currentAction.GetComponent<WhileControl>()
				            .next.GetComponent<BasicAction>().actionType;
		            UISystem.actionExecuted =
			            UISystem.actionExecuted + "-" + currentAction.GetComponent<WhileControl>().next
				            .GetComponent<BasicAction>().actionType;
		            }
		            //////////
		            return currentAction.GetComponent<WhileControl>().next;
	            }
	            else
	            {
		            //Debug.Log("call getFirstActionOf in getNextAction  ====> WhileControl return next");
		            return getFirstActionOf(currentAction.GetComponent<WhileControl>().next, agent);
	            }
					
			}
		}
		// currentAction is not a BasicAction
		// check if it is a ForAction
		else if(currentAction.GetComponent<ForControl>()){
			ForControl forAct = currentAction.GetComponent<ForControl>();
			// ForAction reach the number of iterations
			if (!forAct.gameObject.GetComponent<WhileControl>() && forAct.currentFor >= forAct.nbFor){
				// reset nb iteration to 0
				forAct.currentFor = 0;
				forAct.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
				// return next action
				if (forAct.next == null || forAct.next.GetComponent<BasicAction>())
				{
					///////////xAPI
					if (forAct.next != null)
					{
						UISystem.allActionExecuted =
						UISystem.allActionExecuted +"-"+ forAct.next.GetComponent<BasicAction>().actionType;
						UISystem.actionExecuted =
						UISystem.actionExecuted +"-"+ forAct.next.GetComponent<BasicAction>().actionType;
					}
					////////////////
					return forAct.next;
				}

				else
				{
					//Debug.Log("call getFirstActionOf in getNextAction  ====> ForControl return next");
					return getFirstActionOf(forAct.next , agent);
				}
					
			}
			// iteration are available
			else{
				// in case ForAction has no child
				if (forAct.firstChild == null)
				{
					if (!forAct.gameObject.GetComponent<WhileControl>()) {
						// reset nb iteration to 0
						forAct.currentFor = 0;
						forAct.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
					}
					// return next action
					if (forAct.next == null || forAct.next.GetComponent<BasicAction>())
					{
						///////////xAPI
						if (forAct.next != null)
						{
							UISystem.allActionExecuted =
								UISystem.allActionExecuted +"-"+ forAct.next.GetComponent<BasicAction>().actionType;
							UISystem.actionExecuted =
								UISystem.actionExecuted +"-"+ forAct.next.GetComponent<BasicAction>().actionType;
						}
						////////////////
						return forAct.next;
					}
					else
					{
						//Debug.Log("call getFirstActionOf in getNextAction  ====> ForControl return next");
						return getFirstActionOf(forAct.next, agent);
					}
						
				}
				else
				// return first child
				{
					// add one iteration
					forAct.currentFor++;
					forAct.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
					// return first child
					if (forAct.firstChild == null || forAct.firstChild.GetComponent<BasicAction>())
					{
						if (forAct.next != null)
						{
							UISystem.allActionExecuted =
								UISystem.allActionExecuted + "-For-" + forAct.firstChild.GetComponent<BasicAction>().actionType + "EndFor";
							UISystem.actionExecuted =
								UISystem.actionExecuted + "-For-" + forAct.firstChild.GetComponent<BasicAction>().actionType + "EndFor";
						}
						////////////////
						return forAct.firstChild;
					}
					else
					{
						//Debug.Log("call getFirstActionOf in getNextAction  ====> ForControl return firstchild");
						return getFirstActionOf(forAct.firstChild, agent);
					}
						
				}
			}
		}
		// check if it is a IfAction
		else if(currentAction.GetComponent<IfControl>()){
			Debug.Log("we add if in getnextaction");
			// check if IfAction has a first child and condition is true
			IfControl ifAction = currentAction.GetComponent<IfControl>();
			if (ifValid(ifAction.condition, agent)) {
				// return first action
				
				if (ifAction.firstChild != null && ifAction.firstChild.GetComponent<BasicAction>())
				{
					
					UISystem.allActionExecuted =
						UISystem.allActionExecuted + "-If-" + ifAction.firstChild.GetComponent<BasicAction>().actionType + "EndIf";
					UISystem.actionExecuted =
						UISystem.actionExecuted + "-If-" + ifAction.firstChild.GetComponent<BasicAction>().actionType + "EndIf";
					return ifAction.firstChild;
				}
				
				else if (ifAction.firstChild != null)
				{
					//Debug.Log("call getFirstActionOf in getNextAction  ====> IfControl return firstchild");
					return getFirstActionOf(ifAction.firstChild, agent);
				}

				else
				{
					//Debug.Log("call getFirstActionOf in getNextAction  ====> IfControl return next");
					return getFirstActionOf(ifAction.next, agent);
				}
					
			}
			else if (currentAction.GetComponent<IfElseControl>()) {
				IfElseControl ifElse = currentAction.GetComponent<IfElseControl>();
				// return first child
				if (ifElse.elseFirstChild != null && ifElse.elseFirstChild.GetComponent<BasicAction>())
				{
					UISystem.allActionExecuted =
						UISystem.allActionExecuted + "-Else-" + ifElse.firstChild.GetComponent<BasicAction>().actionType + "EndIf";
					UISystem.actionExecuted =
						UISystem.actionExecuted + "-Else-" + ifElse.firstChild.GetComponent<BasicAction>().actionType + "EndIf";
					return ifElse.elseFirstChild;
				}
				else if (ifElse.elseFirstChild != null)
				{
					//Debug.Log("call getFirstActionOf in getNextAction  ====> IfElseControl return firstchild");
					return getFirstActionOf(ifElse.elseFirstChild, agent);
				}
					
				else
				{
					//Debug.Log("call getFirstActionOf in getNextAction  ====> IfElseControl return next");
					return getFirstActionOf(ifAction.next, agent);
				}
					
			}
			else
			{
				// return next action
				//Debug.Log("call getFirstActionOf in getNextAction  ====>  return next");
				getFirstActionOf(ifAction.next, agent);
			}
		}
		// check if it is a ForeverAction
		else if(currentAction.GetComponent<ForeverControl>()){
			ForeverControl foreverAction = currentAction.GetComponent<ForeverControl>();
			if (foreverAction.firstChild == null || foreverAction.firstChild.GetComponent<BasicAction>())
				return foreverAction.firstChild;
			else
				return getFirstActionOf(foreverAction.firstChild, agent);
		}

		return null;
	}
	

	private IEnumerator delayAddCurrentAction(GameObject nextAction, GameObject agent)
	{
		yield return null; // we add new CurrentAction next frame otherwise families are not notified to this adding because at the begining of this frame GameObject already contains CurrentAction
		GameObjectManager.addComponent<CurrentAction>(nextAction, new { agent = agent });
	}
}