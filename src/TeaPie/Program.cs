using TeaPie.StructureExploration;

Console.WriteLine();

var explorer = new StructureExplorer();
var testCases = explorer.ExploreFileSystem("C:\\Projects\\Topics\\Diploma thesis\\Draft\\TeaPieDraft\\TeaPieDraft\\Scripts");
Console.WriteLine(testCases);
