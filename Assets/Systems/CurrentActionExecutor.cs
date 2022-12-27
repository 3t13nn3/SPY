using UnityEngine;
using FYFY;
using System.Collections;
using TMPro;

/// <summary>
/// This system executes new currentActions
/// </summary>
public class CurrentActionExecutor : FSystem {
	private Family f_wall = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall", "Door"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family f_activableConsole = FamilyManager.getFamily(new AllOfComponents(typeof(Activable),typeof(Position),typeof(AudioSource)));
    private Family f_newCurrentAction = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction), typeof(BasicAction)));
	private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef)), new AnyOfTags("Player"));

	private Family f_coeur1 = FamilyManager.getFamily(new AnyOfTags("Coeur"));
    private Family f_coeur2 = FamilyManager.getFamily(new AnyOfTags("Coeur2"));
    private Family f_coeur3 = FamilyManager.getFamily(new AnyOfTags("Coeur3"));

	private GameData gameData;
	private Family f_trap = FamilyManager.getFamily(new AnyOfTags("Trap"));
	private Family f_msgWarning = FamilyManager.getFamily(new AnyOfTags("MsgWarning"));

	protected override void onStart()
	{
		f_newCurrentAction.addEntryCallback(onNewCurrentAction);
		Pause = true;

		GameObject goData = GameObject.Find("GameData");
        if (goData != null)
        {
            gameData = goData.GetComponent<GameData>();
        }
	}

	protected override void onProcess(int familiesUpdateCount)
	{
		// count inaction if a robot have no CurrentAction
		foreach (GameObject robot in f_player)
			if (robot.GetComponent<ScriptRef>().executableScript.GetComponentInChildren<CurrentAction>() == null)
				robot.GetComponent<ScriptRef>().nbOfInactions++;
		Pause = true;
	}


	private IEnumerator ShowMsgWarning()
    {
        //Print the time of when the function is first called.
        Debug.Log("Started Coroutine at timestamp : " + Time.time);

		foreach (GameObject go in f_msgWarning)
		{
			GameObjectManager.setGameObjectState(go, true);
		}
	
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(2);

		foreach (GameObject go in f_msgWarning)
		{
			GameObjectManager.setGameObjectState(go, false);
		}

        //After we have waited 5 seconds print the time again.
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
    }

	private void lose_healthPoint ()
	{

		GameObject go_levelHP= GameObject.Find("LevelHP");

		MainLoop.instance.StartCoroutine(ShowMsgWarning());


        Debug.Log("Nb de points de vie : "+gameData.healthPoints);

		gameData.healthPoints -= 1;

		go_levelHP.GetComponent<TextMeshProUGUI>().text = gameData.healthPoints + " / 3";

        foreach (GameObject go in f_coeur1)
		{	
			GameObjectManager.setGameObjectState(go, gameData.healthPoints > 0);
		}

        foreach (GameObject go in f_coeur2)
		{	
			GameObjectManager.setGameObjectState(go, gameData.healthPoints > 1);
		}

        foreach (GameObject go in f_coeur3)
		{	
			GameObjectManager.setGameObjectState(go, gameData.healthPoints > 2);
		}

        if (gameData.healthPoints < 1)
        {
        	GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.Dead});
        }
	}

	// each time a new currentAction is added, 
	private void onNewCurrentAction(GameObject currentAction) {
		Pause = false; // activates onProcess to identify inactive robots
		
		CurrentAction ca = currentAction.GetComponent<CurrentAction>();	

		// process action depending on action type
		switch (currentAction.GetComponent<BasicAction>().actionType){
			case BasicAction.ActionType.Forward:
				ApplyForward(ca.agent);
				break;
			case BasicAction.ActionType.TurnLeft:
				ApplyTurnLeft(ca.agent);
				break;
			case BasicAction.ActionType.TurnRight:
				ApplyTurnRight(ca.agent);
				break;
			case BasicAction.ActionType.TurnBack:
				ApplyTurnBack(ca.agent);
				break;
			case BasicAction.ActionType.Wait:
				break;
			case BasicAction.ActionType.Activate:
				Position agentPos = ca.agent.GetComponent<Position>();
				foreach ( GameObject actGo in f_activableConsole){
					if(actGo.GetComponent<Position>().x == agentPos.x && actGo.GetComponent<Position>().y == agentPos.y){
						actGo.GetComponent<AudioSource>().Play();
						// toggle activable GameObject
						if (actGo.GetComponent<TurnedOn>())
							GameObjectManager.removeComponent<TurnedOn>(actGo);
						else
							GameObjectManager.addComponent<TurnedOn>(actGo);
					}
				}
				ca.agent.GetComponent<Animator>().SetTrigger("Action");
				break;
		}

		//Debug.Log("ENFIN !!!!!!!!!");

		//update_healthPoint();


		// notify agent moving
		if (ca.agent.CompareTag("Drone") && !ca.agent.GetComponent<Moved>())
			GameObjectManager.addComponent<Moved>(ca.agent);
	}

	private void ApplyForward(GameObject go){
		switch (go.GetComponent<Direction>().direction){
			case Direction.Dir.North:
				if(!checkObstacle(go.GetComponent<Position>().x, go.GetComponent<Position>().y - 1)){
					go.GetComponent<Position>().x = go.GetComponent<Position>().x;
					go.GetComponent<Position>().y = go.GetComponent<Position>().y - 1;
				}
				break;
			case Direction.Dir.South:
				if(!checkObstacle(go.GetComponent<Position>().x,go.GetComponent<Position>().y + 1)){
					go.GetComponent<Position>().x = go.GetComponent<Position>().x;
					go.GetComponent<Position>().y = go.GetComponent<Position>().y + 1;
				}
				break;
			case Direction.Dir.East:
				if(!checkObstacle(go.GetComponent<Position>().x + 1, go.GetComponent<Position>().y)){
					go.GetComponent<Position>().x = go.GetComponent<Position>().x + 1;
					go.GetComponent<Position>().y = go.GetComponent<Position>().y;
				}
				break;
			case Direction.Dir.West:
				if(!checkObstacle(go.GetComponent<Position>().x - 1, go.GetComponent<Position>().y)){
					go.GetComponent<Position>().x = go.GetComponent<Position>().x - 1;
					go.GetComponent<Position>().y = go.GetComponent<Position>().y;
				}
				break;
		}
		//check if the case is a trap, parcourir tous les pièges et vérfier s'il correspond à la position actuelle comme methode checkObstacle
		// les pièges sont regroupé dans une famille
		checkTrap(go.GetComponent<Position>().x, go.GetComponent<Position>().y);

	}

	private void ApplyTurnLeft(GameObject go){
		switch (go.GetComponent<Direction>().direction){
			case Direction.Dir.North:
				go.GetComponent<Direction>().direction = Direction.Dir.West;
				break;
			case Direction.Dir.South:
				go.GetComponent<Direction>().direction = Direction.Dir.East;
				break;
			case Direction.Dir.East:
				go.GetComponent<Direction>().direction = Direction.Dir.North;
				break;
			case Direction.Dir.West:
				go.GetComponent<Direction>().direction = Direction.Dir.South;
				break;
		}
	}

	private void ApplyTurnRight(GameObject go){
		switch (go.GetComponent<Direction>().direction){
			case Direction.Dir.North:
				go.GetComponent<Direction>().direction = Direction.Dir.East;
				break;
			case Direction.Dir.South:
				go.GetComponent<Direction>().direction = Direction.Dir.West;
				break;
			case Direction.Dir.East:
				go.GetComponent<Direction>().direction = Direction.Dir.South;
				break;
			case Direction.Dir.West:
				go.GetComponent<Direction>().direction = Direction.Dir.North;
				break;
		}
	}

	private void ApplyTurnBack(GameObject go){
		switch (go.GetComponent<Direction>().direction){
			case Direction.Dir.North:
				go.GetComponent<Direction>().direction = Direction.Dir.South;
				break;
			case Direction.Dir.South:
				go.GetComponent<Direction>().direction = Direction.Dir.North;
				break;
			case Direction.Dir.East:
				go.GetComponent<Direction>().direction = Direction.Dir.West;
				break;
			case Direction.Dir.West:
				go.GetComponent<Direction>().direction = Direction.Dir.East;
				break;
		}
	}

	private bool checkObstacle(int x, int z){
		foreach( GameObject go in f_wall){
			if(go.GetComponent<Position>().x == x && go.GetComponent<Position>().y == z)
				return true;
		}
		return false;
	}

	private void checkTrap(int x, int z){
		Debug.Log("check trap in  x "+x + "z " + z);
		foreach( GameObject go in f_trap){
			Debug.Log("trap exist ? ");
			Debug.Log(" x " + go.GetComponent<Position>().x + " y " + go.GetComponent<Position>().y);
			if(go.GetComponent<Position>().x == x && go.GetComponent<Position>().y == z)
				lose_healthPoint();
		}
	}
}
