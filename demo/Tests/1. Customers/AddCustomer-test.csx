tp.Test("Customer should be created successfully.",
    () => tp.Response.StatusCode().Should().Be(201));

var body = tp.Request.GetBody().ToJson();
var id = body["Id"];
tp.CollectionVariables.Set("NewCustomerId", id);
