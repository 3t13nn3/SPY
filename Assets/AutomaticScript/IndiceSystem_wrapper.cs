using UnityEngine;
using FYFY;

public class IndiceSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject indicePanel;
	public UnityEngine.UI.Button indiceButton;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "indicePanel", indicePanel);
		MainLoop.initAppropriateSystemField (system, "indiceButton", indiceButton);
	}

	public void showIndicePanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "showIndicePanel", null);
	}

	public void configureIndice()
	{
		MainLoop.callAppropriateSystemMethod (system, "configureIndice", null);
	}

	public void nextIndice()
	{
		MainLoop.callAppropriateSystemMethod (system, "nextIndice", null);
	}

	public void setActiveOKButton(System.Boolean active)
	{
		MainLoop.callAppropriateSystemMethod (system, "setActiveOKButton", active);
	}

	public void setActiveNextButton(System.Boolean active)
	{
		MainLoop.callAppropriateSystemMethod (system, "setActiveNextButton", active);
	}

	public void closeIndicePanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "closeIndicePanel", null);
	}

}
