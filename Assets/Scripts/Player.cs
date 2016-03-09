using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player {

    public int unitDirection { get; set; }

    public bool mouseControlled = true;

    public Player() {

    }

    virtual internal void DoUpdate() {
    }
}
