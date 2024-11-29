tp.SetVariable("VariableToRemove", "anyValue");
var value = tp.GetVariable<string>("VariableToRemove");

if (tp.ContainsVariable("VariableToRemove"))
{
    tp.RemoveVariable("VariableToRemove");
}

tp.SetVariable("VariableWithDeleteTag", "anyValue", "delete");
var success = tp.RemoveVariablesWithTag("delete");

return success ? 0 : -1;
