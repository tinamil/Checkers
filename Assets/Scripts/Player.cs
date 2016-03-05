using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {
    

    [HideInInspector]
    public IList<Piece> pieces { get; set; }

    public int unitDirection { get; set; }

    // Use this for initialization
    void Awake () {
        pieces = new List<Piece>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
