using FluentAssertions;
using TeaPie.Templating;

namespace TeaPie.Tests.Templating;

public class LoopBodyMaskerShould
{
    [Theory]
    [InlineData("{% for item in Items %}{{ item.Name }}{{ forloop.index }}{% endfor %}")]
    [InlineData("{% for item in Items %}{{ label }}{% assign label = item.Name %}{% endfor %}")]
    [InlineData("{% for t in Items %}{% assign greeting = \"Hello\" %}{{ greeting }}, {{ t.Name }}!{% endfor %}")]
    [InlineData("{% for t in Items %}{% assign url = \"{{ApiGatewayBaseUrl}}\" %}{{ url }}{% endfor %}")]
    [InlineData("{% for t in Items %}{% if forloop.first %}{{ t.Name }}{% endif %}{% endfor %}")]
    [InlineData("{% for t in Items %}{% raw %}{{ApiGatewayBaseUrl}}{{ t.Name }}{% endraw %}{% endfor %}")]
    [InlineData("{% assign gatewayName = \"gw\" %}{{ gatewayName }}")]
    [InlineData("{% assign gatewayName = \"gw\" %}{% for o in Outers %}{% for i in o.Items %}{{ gatewayName }}{% endfor %}" +
        "{% endfor %}")]
    [InlineData("{% for o in Outers %}{% assign label = o.Name %}{% for i in o.Items %}{{ o.Name }}{{ i.Value }}" +
        "{{ label }}{{ forloop.index }}{% endfor %}{% endfor %}")]
    public void LeaveTokensBoundByALoopOrAssignUnmasked(string content)
        => ApplyMask(content).Should().Be(content);

    [Theory]
    [InlineData("{{ApiGatewayBaseUrl}}", "<{{ApiGatewayBaseUrl}}>")]
    [InlineData("{{ gatewayName }}{% assign gatewayName = \"gw\" %}", "<{{ gatewayName }}>{% assign gatewayName = \"gw\" %}")]
    [InlineData("{% for t in Items %}POST {{ApiGatewayBaseUrl}}/{{ t.Name }}{{tSomethingElse}}{% endfor %}",
        "{% for t in Items %}POST <{{ApiGatewayBaseUrl}}>/{{ t.Name }}<{{tSomethingElse}}>{% endfor %}")]
    [InlineData("{% for t in Items %}{% if t.Name == \"Acme\" %}{{ApiGatewayBaseUrl}}{% endif %}{% endfor %}",
        "{% for t in Items %}{% if t.Name == \"Acme\" %}<{{ApiGatewayBaseUrl}}>{% endif %}{% endfor %}")]
    [InlineData("{% for t in Items %}{% assign greeting = \"Hi\" %}{{ greeting }}{{greetingX}}{% endfor %}",
        "{% for t in Items %}{% assign greeting = \"Hi\" %}{{ greeting }}<{{greetingX}}>{% endfor %}")]
    [InlineData("{% for t in Items %}{% raw %}{{ t.Name }}{% endraw %}{{ApiGatewayBaseUrl}}{% endfor %}",
        "{% for t in Items %}{% raw %}{{ t.Name }}{% endraw %}<{{ApiGatewayBaseUrl}}>{% endfor %}")]
    [InlineData("{% for t in Items %}{% raw %}{% assign greeting = \"Hi\" %}{% endraw %}{{ greeting }}{% endfor %}",
        "{% for t in Items %}{% raw %}{% assign greeting = \"Hi\" %}{% endraw %}<{{ greeting }}>{% endfor %}")]
    [InlineData("{% for a in Items %}{% assign shared = a.Name %}{% endfor %}{% for b in Items %}{{ shared }}{% endfor %}" +
        "{{ shared }}",
        "{% for a in Items %}{% assign shared = a.Name %}{% endfor %}{% for b in Items %}<{{ shared }}>{% endfor %}" +
        "<{{ shared }}>")]
    [InlineData("{% for o in Outers %}{% for a in o.As %}{% assign flag = true %}{% endfor %}" +
        "{% for b in o.Bs %}{{ flag }}{% endfor %}{{ flag }}{% endfor %}",
        "{% for o in Outers %}{% for a in o.As %}{% assign flag = true %}{% endfor %}" +
        "{% for b in o.Bs %}<{{ flag }}>{% endfor %}<{{ flag }}>{% endfor %}")]
    [InlineData("{% for t in Items %}{{$guid}}-{{ t.Name }}-{{$randomInt 1 100}}{{$add {{X}} {{Y}}}}" +
        "{{$outer {{$inner {{X}} 1}} 2}}{{$add {{X}} 1}}{{$add {{Y}} 1}}{% endfor %}",
        "{% for t in Items %}<{{$guid}}>-{{ t.Name }}-<{{$randomInt 1 100}}><{{$add {{X}} {{Y}}}}>" +
        "<{{$outer {{$inner {{X}} 1}} 2}}><{{$add {{X}} 1}}><{{$add {{Y}} 1}}>{% endfor %}")]
    [InlineData("{% for t in Items %}{{ApiGatewayBaseUrl}}/{{ forloop.index | prepend: \"{{Temp.Id_\" | append: \"}}\" }}" +
        "{% endfor %}",
        "{% for t in Items %}<{{ApiGatewayBaseUrl}}>/{{ forloop.index | prepend: \"{{Temp.Id_\" | append: \"}}\" }}" +
        "{% endfor %}")]
    public void WrapTokensOutsideAnyLoopOrAssignScopeInARawBlock(string content, string expected)
        => ApplyMask(content).Should().Be(expected.Replace("<{{", "{% raw %}{{").Replace("}}>", "}}{% endraw %}"));

    private static string ApplyMask(string content)
    {
        var blocks = new LoopBlockScanner().FindLoopBlocks(content);
        var edits = new LoopBodyMasker().FindMaskEdits(content, blocks);
        return TextEditApplier.Apply(content, edits);
    }
}
