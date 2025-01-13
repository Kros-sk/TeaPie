tp.Test("Customer should be deleted successfully.",
    () => tp.Response.StatusCode().Should().Be(200));

tp.RemoveVariable("NewCustomerId");
