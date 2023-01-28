﻿using FYFY;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manage steps (automatic simulation or controled by player)
/// </summary>
public class StepSystem : FSystem {

    private Family f_newEnd = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
    private Family f_newStep = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private Family f_currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction)));

    private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
    private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

    private Family f_coeur1 = FamilyManager.getFamily(new AnyOfTags("Coeur"));
    private Family f_coeur2 = FamilyManager.getFamily(new AnyOfTags("Coeur2"));
    private Family f_coeur3 = FamilyManager.getFamily(new AnyOfTags("Coeur3"));

    private float timeStepCpt;
	private GameData gameData;
    private int nbStep;
    private bool newStepAskedByPlayer;

    protected override void onStart()
    {
        nbStep = 0;
        newStepAskedByPlayer = false;
        GameObject go = GameObject.Find("GameData");
        if (go != null)
        {
            gameData = go.GetComponent<GameData>();
            timeStepCpt = 1 / gameData.gameSpeed_current;
        }
        f_newStep.addEntryCallback(onNewStep);


    

        f_playingMode.addEntryCallback(delegate
        {
            // count a new execution
            gameData.totalExecute++;
            gameData.totalStep++;
            gameData.actionExecutedPerAttempt = "";
            timeStepCpt = 1 / gameData.gameSpeed_current;
            nbStep++;
            Pause = false;
            setToDefaultTimeStep();

            
            // test point de vies 
            /*
            gameData.healthPoints -= 1;
            Debug.Log("Nb de points de vie : "+gameData.healthPoints);

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
            }*/


        });

        f_editingMode.addEntryCallback(delegate
        {
            Pause = true;
        });
    }

    private void onNewStep(GameObject go)
    {
        GameObjectManager.removeComponent(go.GetComponent<NewStep>());  
        timeStepCpt = (1 / gameData.gameSpeed_current) + timeStepCpt; // le "+ timeStepCpt" permet de prendre en compte le débordement de temps de la frame précédente
        //Debug.Log(" Nouveau step ");
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
        //Organize each steps
        if (f_newEnd.Count == 0 && (playerHasNextAction() || timeStepCpt > 0))
        {
            //activate step
            if (timeStepCpt <= 0)
            {
                GameObjectManager.addComponent<NewStep>(MainLoop.instance.gameObject);
                gameData.totalStep++;
                nbStep++;
                //Debug.Log(" Nouvelle action");
                if (newStepAskedByPlayer)
                {
                    newStepAskedByPlayer = false;
                    Pause = true;
                }
            }
            else
                timeStepCpt -= Time.deltaTime;
        }
        else // newEnd_f.Count > 0 || (!playerHasNextAction && timeStep CPt <=0)
        {
            MainLoop.instance.StartCoroutine(delayCheckEnd());
            //xAPI tracer la tentative du joueur pour le niveau
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("number", UISystem.level);
            dic.Add("attempt", gameData.totalExecute.ToString());
            dic.Add("script", gameData.actionExecutedPerAttempt);
            GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
            {
                verb = UISystem.verb,
                objectType = UISystem.objectType,
                activityExtensions = dic
            });
            ////////
            Pause = true;
        }
    }

    private IEnumerator delayCheckEnd()
    {
        // wait a new possible current action
        yield return null;
        yield return null;
        // If there are still no actions => return to edit mode
        if (!playerHasNextAction() || f_newEnd.Count > 0)
        {
            GameObjectManager.addComponent<EditMode>(MainLoop.instance.gameObject);
            // We save history if no end or win
            if (f_newEnd.Count <= 0)
                GameObjectManager.addComponent<AskToSaveHistory>(MainLoop.instance.gameObject);
        }
        else
            Pause = false;
    }

    // Check if one of the robot programmed by the player has a next action to perform
    private bool playerHasNextAction(){
		CurrentAction act;
		foreach(GameObject go in f_currentActions){
			act = go.GetComponent<CurrentAction>();
			if(act.agent != null && act.agent.CompareTag("Player") && act.GetComponent<BaseElement>().next != null)
				return true;
		}
        return false;
    }

    // See PauseButton, ContinueButton in editor
    public void autoExecuteStep(bool on){
        Pause = !on;
    }

    // See NextStepButton in editor
    public void goToNextStep(){
        Pause = false;
        newStepAskedByPlayer = true;
    }

    // See StopButton in editor
    public void cancelTotalStep(){ //click on stop button
        gameData.totalStep -= nbStep;
        nbStep = 0;
    }

    // See SpeedButton in editor
    public void speedTimeStep(){
        gameData.gameSpeed_current = gameData.gameSpeed_default * 3f;
    }

    // See ContinueButton in editor
    public void setToDefaultTimeStep(){
        gameData.gameSpeed_current = gameData.gameSpeed_default;
    }
}