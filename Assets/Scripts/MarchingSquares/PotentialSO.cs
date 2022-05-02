using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "PotentialSO", menuName = "Scriptable Objects/MarchingDevelopment/PotentialSO", order = 0)]
public class PotentialSO : ScriptableObject {
    [SerializeField] private string potential;

    public float Evaluate(Vector2 pos) {
        potential = potential.Replace("x",$"{pos.x}");
        potential = potential.Replace("y",$"{pos.y}");
        ExpressionEvaluator.Evaluate(potential, out float result);
        return result;
    }
    public void ValidatePotential() {
        string validSymbols = new string("xy+-*/%^()");
        List<string> validCharStrings = new List<string>(){"sqrt","floor","ceil","round","cos","sin","tan","pi",""};
        StringBuilder stringToValidate = new StringBuilder();
        foreach (char character in potential) {
            if (char.IsDigit(character) || validSymbols.Contains(character)) {
                if(!validCharStrings.Contains(stringToValidate.ToString())) {throw new InvalidPotentialExpressionException();}
                stringToValidate.Clear();
            } else {
                stringToValidate.Append(character);
            }
        }
        if(stringToValidate.ToString() != "") {throw new InvalidPotentialExpressionException();}
        return;
    }
    
    private void OnValidate() {
        potential = potential.ToLower();
        potential = potential.Replace(" ","");
        ValidatePotential();
    }
}

[System.Serializable]
public class InvalidPotentialExpressionException : System.Exception
{
    public InvalidPotentialExpressionException() { }
    public InvalidPotentialExpressionException(string message) : base(message) { }
    public InvalidPotentialExpressionException(string message, System.Exception inner) : base(message, inner) { }
    protected InvalidPotentialExpressionException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
