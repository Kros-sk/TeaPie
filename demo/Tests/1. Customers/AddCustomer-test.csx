tp.Test("Customer should be created successfully.",
    () => Equal(tp.Response.StatusCode(), 201));

var body = tp.Request.GetBody().ToJson();
var id = (long)body["Id"];

tp.CollectionVariables.Set("NewCustomerId", id);
