using UnityEngine;
using System.Collections;

[System.Serializable]
public class CheckersException : System.Exception {
    public CheckersException() { }
    public CheckersException(string message) : base(message) { }
    public CheckersException(string message, System.Exception inner) : base(message, inner) { }
    protected CheckersException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

public class ExceptionManager {

}
