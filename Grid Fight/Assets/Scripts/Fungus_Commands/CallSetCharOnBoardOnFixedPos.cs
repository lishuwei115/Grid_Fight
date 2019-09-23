﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

/// <summary>
/// Calls a named method on a GameObject using the GameObject.SendMessage() system.
/// This command is called "Call Method" because a) it's more descriptive than Send Message and we're already have
/// a Send Message command for sending messages to trigger block execution.
/// </summary>
[CommandInfo("Scripting",
                "Call SetCharOnBoardOnFixedPos",
                "Calls a named method on a GameObject using the GameObject.SendMessage() system.")]
public class CallSetCharOnBoardOnFixedPos : Command
{
    public ControllerType playerController;
    public CharacterType ct;
    public Vector2Int pos;

    protected virtual void CallTheMethod()
    {
        BattleManagerScript.Instance.SetCharOnBoardOnFixedPos(playerController, ct, pos);
    }

    #region Public members

    public override void OnEnter()
    {
        CallTheMethod();
        Continue();
    }

    public override Color GetButtonColor()
    {
        return new Color32(235, 191, 217, 255);
    }

    #endregion
}

