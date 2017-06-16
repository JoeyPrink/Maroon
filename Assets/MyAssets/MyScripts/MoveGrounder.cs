﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using VRTK;

public class MoveGrounder : VRTK_InteractableObject
{
    // TODO get index? of controller right or left

    public override void StartUsing(GameObject usingObject)
    {
        base.StartUsing(usingObject);

        Debug.Log("Grounder USED via pointer");
        // TODO move Application.LoadLevel(this.LevelName);
    }

    public override void StopUsing(GameObject usingObject)
    {
        base.StopUsing(usingObject);
    }

    // Update is called once per frame
    protected override void Update()
    {
        // TODO check movement
    }

    //helper fct, unused for now
    private void loadLevel(string name)
    {
        Debug.Log("ENTER BUTTON pressed, will load " + name);
        SceneManager.LoadScene(name);
    }
}

