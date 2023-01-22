using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FYFY;
using Newtonsoft.Json.Serialization;
using TMPro;
using UnityEngine.UI;

public class IndiceSystem : FSystem {

	private Family f_msgIndice = FamilyManager.getFamily(new AnyOfTags("MsgNewIndice"));
	
	private GameData gameData;
	public GameObject indicePanel;
	public Button indiceButton;
	public static bool showSecondInd;
	private int nIndice;
	private bool showMsg;


	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		showMsg = false;
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();
		/* ancienne version
		 nIndice = -1;
		 */
		showSecondInd = false;
		GameObjectManager.setGameObjectState(indicePanel.transform.parent.gameObject, false);
		//si des indices ont été défini pour le niveau on affiche le boutton "indice"
		if (gameData == null || gameData.indiceMessage.Count == 0)
		{
			GameObjectManager.setGameObjectState(indiceButton.gameObject, false); 
		}
		
	}

	protected override void onProcess(int familiesUpdateCount)
	{
		/* ancienne version
		 si le joueur a fait deux tentatives mais n'a toujours pas fini le niveau on déploque le deuxième indice
		if (showSecondInd) indiceButton.interactable = true;
		 */
		if (UISystem.attempt >= 2 && !showMsg)
		{
			MainLoop.instance.StartCoroutine(ShowMsgNewIndice());
			showMsg = true;
		}
	}


	public void showIndicePanel()
	{
		/* ancienne version
		si (c'est le premier indice (indice 0) à afficher ou que le deuxième a été déploqué) et
		qu'il reste des indices à afficher alors on affiche l'indice
		
		if ((nIndice == -1 || showSecondInd) && nIndice + 1 < gameData.indiceMessage.Count)
		*/
		
		nIndice = -1;
		if (nIndice + 1 < gameData.indiceMessage.Count)
		{
			GameObjectManager.setGameObjectState(indicePanel.transform.parent.gameObject, true);
			nIndice += 1; //on incrémente le nombre d'indice affiché 
			configureIndice();
			/* ancienne version
			si c'est le premier indice qu'on affiche et qu'il a déja fait deux tentatives alors afficher les deux indices dicrectement
			
			if (nIndice == 0 && showSecondInd) 
			*/
			
			if (showSecondInd) 
			{
				setActiveNextButton(true);
				setActiveOKButton(false);
			}
			else 
			{
				setActiveNextButton(false);
				setActiveOKButton(true);
			}
			//showSecondInd = false; //si le deuxième indice a déja été affiché on met la var à false
                          //pour que le bouton indice soit désactivé par la méthode onprocess
		}
	}
	
	
	public void configureIndice()
	{
		GameObject textGO = indicePanel.transform.Find("Text").gameObject;
		if (gameData.indiceMessage[nIndice].Item1 != null)
		{
			GameObjectManager.setGameObjectState(textGO, true);
			textGO.GetComponent<TextMeshProUGUI>().text = gameData.indiceMessage[nIndice].Item1;
			/////////////////////
			Dictionary<string, string> dic = new Dictionary<string, string>();
			dic.Add("number", UISystem.level);
			dic.Add("value", gameData.indiceMessage[nIndice].Item1);
			if (nIndice == 0)
				dic.Add("type", "basic");
			else 
				dic.Add("type", "advanced");
			Debug.Log(dic.Values);
			GameObjectManager.addComponent<ActionPerformedForLRS>(textGO, new
			{
				verb = "requested",
				objectType = "hint",
				activityExtensions = dic
			});
			////////////////////
			if (gameData.indiceMessage[nIndice].Item2 != -1)
				((RectTransform)textGO.transform).sizeDelta = new Vector2(((RectTransform)textGO.transform).sizeDelta.x, gameData.indiceMessage[nIndice].Item2);
			else
				((RectTransform)textGO.transform).sizeDelta = new Vector2(((RectTransform)textGO.transform).sizeDelta.x, textGO.GetComponent<LayoutElement>().preferredHeight);
		}
		else
			GameObjectManager.setGameObjectState(textGO, false);
	}

	
	public void nextIndice()
	{
		nIndice++; // On incr�mente le nombre de dialogue
		configureIndice();
		setActiveOKButton(true);
		setActiveNextButton(false);
	}
	
	
	
	// Active ou non le bouton Ok du panel dialogue
	public void setActiveOKButton(bool active)
	{
		GameObjectManager.setGameObjectState(indicePanel.transform.Find("Buttons").Find("OKButtonIndice").gameObject, active);
	}


	// Active ou non le bouton next du panel dialogue
	public void setActiveNextButton(bool active)
	{
		GameObjectManager.setGameObjectState(indicePanel.transform.Find("Buttons").Find("NextButtonIndice").gameObject, active);
	}
	
	
	public void closeIndicePanel()
	{
		GameObjectManager.setGameObjectState(indicePanel.transform.parent.gameObject, false);
		/*ancienne version 
		indiceButton.interactable = false; //on désactive le bouton indice 
		*/
	}
	
	private IEnumerator ShowMsgNewIndice()
	{
		//Print the time of when the function is first called.
		Debug.Log("Started Coroutine at timestamp : " + Time.time);

		foreach (GameObject go in f_msgIndice)
		{
			GameObjectManager.setGameObjectState(go, true);
		}
	
		//yield on a new YieldInstruction that waits for 5 seconds.
		yield return new WaitForSeconds(5);

		foreach (GameObject go in f_msgIndice)
		{
			GameObjectManager.setGameObjectState(go, false);
		}

		//After we have waited 5 seconds print the time again.
		Debug.Log("Finished Coroutine at timestamp : " + Time.time);
	}
}