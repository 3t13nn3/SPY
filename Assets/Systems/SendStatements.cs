using UnityEngine;
using FYFY;
using DIG.GBLXAPI;
using System;
using System.IO;
using System.Collections.Generic;
using DIG.GBLXAPI.Builders;
using Newtonsoft.Json;

public class SendStatements : FSystem {

    private Family f_actionForLRS = FamilyManager.getFamily(new AllOfComponents(typeof(ActionPerformedForLRS)));
  
    public static SendStatements instance;

    public SendStatements()
    {
        instance = this;
    }
	
	protected override void onStart()
    {
		initGBLXAPI();
    }

    public void initGBLXAPI()
    {
        if (!GBLXAPI.IsInit)
            GBLXAPI.Init(GBL_Interface.lrsAddresses);

        GBLXAPI.debugMode = false;

        string sessionID = Environment.MachineName + "-" + DateTime.Now.ToString("yyyy.MM.dd.hh.mm.ss");
        //Generate player name unique to each playing session (computer name + date + hour)
        GBL_Interface.playerName = String.Format("{0:X}", sessionID.GetHashCode());
        GBL_Interface.userUUID = GBL_Interface.playerName;
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        
        // Do not use callbacks because in case in the same frame actions are removed on a GO and another component is added in another system, family will not trigger again callback because component will not be processed
        foreach (GameObject go in f_actionForLRS)
        {
            ActionPerformedForLRS[] listAP = go.GetComponents<ActionPerformedForLRS>();
            int nb = listAP.Length;
            ActionPerformedForLRS ap;
            if (!this.Pause)
            {
                for (int i = 0; i < nb; i++)
                {
                    ap = listAP[i];
                    //If no result info filled
                    if (!ap.result)
                    {
                        /*Debug.Log("verb : " + ap.verb);
                        Debug.Log("object Type : " + ap.objectType);
                        Debug.Log("activity extensions : " + ap.activityExtensions);
                        Debug.Log("cas 1");
                        if (ap.activityExtensions != null)
                        {
                            foreach (KeyValuePair<string, string> entry in ap.activityExtensions)
                                Debug.Log(entry);
                        }*/
                        Debug.Log("object type : "+ap.objectType);
                        GBL_Interface.SendStatement(ap.verb, ap.objectType, ap.activityExtensions);
                        
                    }
                    else
                    {
                        /*Debug.Log(ap.verb);
                        Debug.Log(ap.objectType);
                        Debug.Log(ap.activityExtensions);
                        Debug.Log("cas 2");*/
                        bool? completed = null, success = null;

                        if (ap.completed > 0)
                            completed = true;
                        else if (ap.completed < 0)
                            completed = false;

                        if (ap.success > 0)
                            success = true;
                        else if (ap.success < 0)
                            success = false;

                        GBL_Interface.SendStatementWithResult(ap.verb, ap.objectType, ap.activityExtensions, ap.resultExtensions, completed, success, ap.response, ap.score, ap.duration);
                    
                    }
                    
                }
            }
            for (int i = nb - 1; i > -1; i--)
            {
                GameObjectManager.removeComponent(listAP[i]);
            }
        }
    }

    /*public static void testSendStatement()
    {
        Debug.Log(GBL_Interface.playerName + " asks to send statement...");
        GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
        {
            verb = "dragged",
            objectType = "block"
        });
    }*/
}