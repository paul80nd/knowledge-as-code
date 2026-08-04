// In-process unit tests for the kac.core Yaml representation-model helpers, driven through a real
// temp file so LoadFile is covered end to end.

using kac.core;

namespace kac.tests;

public class YamlTests : IDisposable
{
    private readonly string _path = Path.GetTempFileName();

    private YamlDotNet.RepresentationModel.YamlNode Load(string yaml)
    {
        File.WriteAllText(_path, yaml);
        return Yaml.LoadFile(_path);
    }

    [Fact]
    public void Reads_scalars_lists_and_bools()
    {
        var root = Load("name: alpha\nflag: yes\ncount: 7\ntags:\n  - a\n  - b\n");

        Assert.Equal("alpha", Yaml.Str(Yaml.Get(root, "name")));
        Assert.True(Yaml.Bool(Yaml.Get(root, "flag")));
        Assert.Equal(7, Yaml.Int(Yaml.Get(root, "count"), fallback: 0));
        Assert.Equal(["a", "b"], Yaml.StrList(Yaml.Get(root, "tags")));
    }

    [Fact]
    public void Missing_key_is_null_and_Int_falls_back()
    {
        var root = Load("name: alpha\n");

        Assert.Null(Yaml.Get(root, "absent"));
        Assert.Equal(42, Yaml.Int(Yaml.Get(root, "absent"), fallback: 42));
    }

    public void Dispose() => File.Delete(_path);
}
