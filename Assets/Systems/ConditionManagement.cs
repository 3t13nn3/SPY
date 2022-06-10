using UnityEngine;
using FYFY;

public class ConditionManagement : FSystem {

	public GameObject endPanel;

	public static ConditionManagement instance;

	public ConditionManagement()
	{
		instance = this;
	}

	// Transforme une sequence de condition en une chaine de caract�re
	public string convertionConditionSequence(GameObject condition)
	{
		string chaine = "";
		// On regarde si la condition re�ue est un �l�ment ou bien un op�rator
		// Si c'est un �l�ment, on le traduit en string et on le renvoie 
		if (condition.GetComponent<BaseCondition>().Type == BaseCondition.blockType.Element)
		{
			chaine = chaine + condition.GetComponent<BaseCondition>().conditionType;
		}
		else if (condition.GetComponent<BaseCondition>().Type == BaseCondition.blockType.Operator)
		{
			// Si c'est une n�gation on met "!" puis on fait une r�cursive sur le container et on renvoie le tous traduit en string
			if (condition.GetComponent<BaseCondition>().conditionType == BaseCondition.ConditionType.NotOperator)
            {
                // On v�rifie qu'il y a bien un �l�ment pr�sent
                if (!condition.transform.GetChild(1).gameObject.GetComponent<EndBlockScriptComponent>())
                {
					chaine = chaine + "!" + convertionConditionSequence(condition.transform.GetChild(1).gameObject);
				}
                else
                {
					GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.BadCondition });
				}
			}
			else if (condition.GetComponent<BaseCondition>().conditionType == BaseCondition.ConditionType.AndOperator)
			{
				// Si les c�t� de l'op�rateur sont remplit, alors il compte 6 child, sinon cela veux dire que il manque des conditions
				if(condition.transform.childCount == 6)
                {
					chaine = chaine + "(" + convertionConditionSequence(condition.transform.GetChild(1).gameObject) + "&&" + convertionConditionSequence(condition.transform.GetChild(4).gameObject) + ")";
				}
                else
                {
					GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.BadCondition });
				}
			}
			else if (condition.GetComponent<BaseCondition>().conditionType == BaseCondition.ConditionType.OrOperator)
			{
				// Si les c�t� de l'op�rateur sont remplit, alors il compte 6 child, sinon cela veux dire que il manque des conditions
				if (condition.transform.childCount == 6)
				{
					chaine = chaine + "(" + convertionConditionSequence(condition.transform.GetChild(1).gameObject) + "||" + convertionConditionSequence(condition.transform.GetChild(4).gameObject) + ")";
				}
				else
				{
					GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.BadCondition });
				}
			}
		}

		return chaine;
	}
}