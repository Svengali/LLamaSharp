using System.Text;
using LLama.Common;
using LLama.Extensions;

namespace LLama.Unittest;

public sealed class TemplateTests
    : IDisposable
{
    private readonly LLamaWeights _model;

    public TemplateTests()
    {
        var @params = new ModelParams(Constants.GenerativeModelPath)
        {
            ContextSize = 1,
            GpuLayerCount = Constants.CIGpuLayerCount
        };
        _model = LLamaWeights.LoadFromFile(@params);
    }

    public void Dispose()
    {
        _model.Dispose();
    }

    [Fact]
    public void BasicTemplate()
    {
        var templater = new LLamaTemplate(_model);

        Assert.Equal(0, templater.Count);
        templater.Add("assistant", "hello");
        Assert.Equal(1, templater.Count);
        templater.Add("user", "world");
        Assert.Equal(2, templater.Count);
        templater.Add("assistant", "111");
        Assert.Equal(3, templater.Count);
        templater.Add("user", "aaa");
        Assert.Equal(4, templater.Count);
        templater.Add("assistant", "222");
        Assert.Equal(5, templater.Count);
        templater.Add("user", "bbb");
        Assert.Equal(6, templater.Count);
        templater.Add("assistant", "333");
        Assert.Equal(7, templater.Count);
        templater.Add("user", "ccc");
        Assert.Equal(8, templater.Count);

        // Call once with empty array to discover length
        var length = templater.Apply(Array.Empty<byte>());
        var dest = new byte[length];

        Assert.Equal(8, templater.Count);

        // Call again to get contents
        length = templater.Apply(dest);

        Assert.Equal(8, templater.Count);

        var templateResult = Encoding.UTF8.GetString(dest.AsSpan(0, length));
        const string expected = "<|im_start|>assistant\nhello<|im_end|>\n" +
                                "<|im_start|>user\nworld<|im_end|>\n" +
                                "<|im_start|>assistant\n" +
                                "111<|im_end|>" +
                                "\n<|im_start|>user\n" +
                                "aaa<|im_end|>\n" +
                                "<|im_start|>assistant\n" +
                                "222<|im_end|>\n" +
                                "<|im_start|>user\n" +
                                "bbb<|im_end|>\n" +
                                "<|im_start|>assistant\n" +
                                "333<|im_end|>\n" +
                                "<|im_start|>user\n" +
                                "ccc<|im_end|>\n";

        Assert.Equal(expected, templateResult);
    }

    [Fact]
    public void CustomTemplate()
    {
        var templater = new LLamaTemplate("gemma");

        Assert.Equal(0, templater.Count);
        templater.Add("assistant", "hello");
        Assert.Equal(1, templater.Count);
        templater.Add("user", "world");
        Assert.Equal(2, templater.Count);
        templater.Add("assistant", "111");
        Assert.Equal(3, templater.Count);
        templater.Add("user", "aaa");
        Assert.Equal(4, templater.Count);

        // Call once with empty array to discover length
        var length = templater.Apply(Array.Empty<byte>());
        var dest = new byte[length];

        Assert.Equal(4, templater.Count);

        // Call again to get contents
        length = templater.Apply(dest);

        Assert.Equal(4, templater.Count);

        var templateResult = Encoding.UTF8.GetString(dest.AsSpan(0, length));
        const string expected = "<start_of_turn>model\n" +
                                "hello<end_of_turn>\n" +
                                "<start_of_turn>user\n" +
                                "world<end_of_turn>\n" +
                                "<start_of_turn>model\n" +
                                "111<end_of_turn>\n" +
                                "<start_of_turn>user\n" +
                                "aaa<end_of_turn>\n";

        Assert.Equal(expected, templateResult);
    }

    [Fact]
    public void BasicTemplateWithAddAssistant()
    {
        var templater = new LLamaTemplate(_model)
        {
            AddAssistant = true,
        };

        Assert.Equal(0, templater.Count);
        templater.Add("assistant", "hello");
        Assert.Equal(1, templater.Count);
        templater.Add("user", "world");
        Assert.Equal(2, templater.Count);
        templater.Add("assistant", "111");
        Assert.Equal(3, templater.Count);
        templater.Add("user", "aaa");
        Assert.Equal(4, templater.Count);
        templater.Add("assistant", "222");
        Assert.Equal(5, templater.Count);
        templater.Add("user", "bbb");
        Assert.Equal(6, templater.Count);
        templater.Add("assistant", "333");
        Assert.Equal(7, templater.Count);
        templater.Add("user", "ccc");
        Assert.Equal(8, templater.Count);

        // Call once with empty array to discover length
        var length = templater.Apply(Array.Empty<byte>());
        var dest = new byte[length];

        Assert.Equal(8, templater.Count);

        // Call again to get contents
        length = templater.Apply(dest);

        Assert.Equal(8, templater.Count);

        var templateResult = Encoding.UTF8.GetString(dest.AsSpan(0, length));
        const string expected = "<|im_start|>assistant\nhello<|im_end|>\n" +
                                "<|im_start|>user\nworld<|im_end|>\n" +
                                "<|im_start|>assistant\n" +
                                "111<|im_end|>" +
                                "\n<|im_start|>user\n" +
                                "aaa<|im_end|>\n" +
                                "<|im_start|>assistant\n" +
                                "222<|im_end|>\n" +
                                "<|im_start|>user\n" +
                                "bbb<|im_end|>\n" +
                                "<|im_start|>assistant\n" +
                                "333<|im_end|>\n" +
                                "<|im_start|>user\n" +
                                "ccc<|im_end|>\n" +
                                "<|im_start|>assistant\n";

        Assert.Equal(expected, templateResult);
    }

    [Fact]
    public void GetOutOfRangeThrows()
    {
        var templater = new LLamaTemplate(_model);

        Assert.Throws<ArgumentOutOfRangeException>(() => templater[0]);

        templater.Add("assistant", "1");
        templater.Add("user", "2");

        Assert.Throws<ArgumentOutOfRangeException>(() => templater[-1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => templater[2]);
    }

    [Fact]
    public void RemoveMid()
    {
        var templater = new LLamaTemplate(_model);

        templater.Add("assistant", "1");
        templater.Add("user", "2");
        templater.Add("assistant", "3");
        templater.Add("user", "4a");
        templater.Add("user", "4b");
        templater.Add("assistant", "5");

        Assert.Equal("user", templater[3].Role);
        Assert.Equal("4a", templater[3].Content);

        Assert.Equal("assistant", templater[5].Role);
        Assert.Equal("5", templater[5].Content);

        Assert.Equal(6, templater.Count);
        templater.RemoveAt(3);
        Assert.Equal(5, templater.Count);

        Assert.Equal("user", templater[3].Role);
        Assert.Equal("4b", templater[3].Content);

        Assert.Equal("assistant", templater[4].Role);
        Assert.Equal("5", templater[4].Content);
    }

    [Fact]
    public void RemoveLast()
    {
        var templater = new LLamaTemplate(_model);

        templater.Add("assistant", "1");
        templater.Add("user", "2");
        templater.Add("assistant", "3");
        templater.Add("user", "4a");
        templater.Add("user", "4b");
        templater.Add("assistant", "5");

        Assert.Equal(6, templater.Count);
        templater.RemoveAt(5);
        Assert.Equal(5, templater.Count);

        Assert.Equal("user", templater[4].Role);
        Assert.Equal("4b", templater[4].Content);
    }

    [Fact]
    public void RemoveOutOfRange()
    {
        var templater = new LLamaTemplate(_model);

        Assert.Throws<ArgumentOutOfRangeException>(() => templater.RemoveAt(0));

        templater.Add("assistant", "1");
        templater.Add("user", "2");

        Assert.Throws<ArgumentOutOfRangeException>(() => templater.RemoveAt(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => templater.RemoveAt(2));
    }

    [Fact]
    public void Clear_ResetsTemplateState()
    {
        var templater = new LLamaTemplate(_model);
        templater.Add("assistant", "1")
            .Add("user", "2");

        Assert.Equal(2, templater.Count);

        templater.Clear();

        Assert.Equal(0, templater.Count);

        const string userData = nameof(userData);
        templater.Add("user", userData);

        // Generte the template string
        // Call once with empty array to discover length
        var length = templater.Apply(Array.Empty<byte>());
        var dest = new byte[length];

        Assert.Equal(1, templater.Count);

        // Call again to get contents
        _ = templater.Apply(dest);
        var templateResult = Encoding.UTF8.GetString(dest.AsSpan(0, length));

        const string expectedTemplate = $"<|im_start|>user\n{userData}<|im_end|>\n";
        Assert.Equal(expectedTemplate, templateResult);
    }

    [Fact]
    public void EndOTurnToken_ReturnsExpected()
    {
        Assert.Null(_model.Tokens.EndOfTurnToken);
    }

    [Fact]
    public void EndOSpeechToken_ReturnsExpected()
    {
        Assert.Equal("</s>", _model.Tokens.EndOfSpeechToken);
    }
}