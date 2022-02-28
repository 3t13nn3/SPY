using UnityEngine;

public class CompetenceActive : MonoBehaviour {
	// Vous trouverez ici l'ensemble des comp�tences PIAF pr�sent (si possible et paramatrable) dans le jeu
	// Celle-ci sont toutes d�sactiv�es par d�faut, seul le parametrage lors de la cr�ation de niveau d�terminera la pr�sence des comp�tences dans le niveau.

	// C1.1 Nommer des objets et s�quence d'actions
	public bool nameObject = false;
	// C1.3 Identifier les param�tres d'entr�e d'une s�quence d'actions
	public bool multiAgent = false;
	// C1.5 Pr�dir le r�sultat d'une s�quence d'actions
	public bool predictionSequence = false;
	// C1.6 Utiliser des objets dont la valeur peut changer
	public bool variableValue = false;
	// C1.7 Reconna�tre, parmi des objets et s�quences d'action connus, lesquels peuvent �tre utilis�s pour atteindre un nouvel objectif
	public bool similarProblemSolution = false;
	// C2.1 Ordonner une s�quence d'actions pour atteindre un objectif
	public bool orderSequence = false;
	// C2.2 Compl�ter une s�quence d'actions pour atteindre un objectif simple
	public bool completeSequence = false;
	// C2.5 Combiner des s�quences d'actions pour atteindre un objectif
	public bool combineSequence = false;
	// C2.6 D�composer des objectifs en sous-objectifs plus simple
	public bool subGoal = false;
	// C3.1 R�p�ter une s�quence d'actions un nombre donn� de fois
	public bool forAction = false;
	// C3.2 R�p�ter une s�quence d'actions jusqu'� ce qu'un objectif soit atteint
	public bool whileAction = false;
	// C3.3 Int�grer une condition simple dans une s�quence d'actions
	public bool conditionAction = false;
	// C3.4 Int�grer une condition complexe dans une s�quence d'actions
	public bool multiConditionAction = false;
	// C4.1 Comparer deux objets selon un crit�re donn�
	public bool comparisonCriterion = false;
	// C4.2 Comparer deux s�quences d'actions selon un crit�re donn�
	public bool comparisonSequence = false;
	// C4.3 Am�liorer une s�quence d'actions par rapport � un crit�re donn�
	public bool completeSequenceWithCriterion = false;
	// C5.2 Traduire des objets ou s�quences d'actions entre repr�sentations formelles
	public bool consolePresence = false;
	// C6.4 Etendre ou modifier une s�quence d'actions pour atteindre un nouvelle objectif
	public bool oneSequenceMultiGoal = false;
}