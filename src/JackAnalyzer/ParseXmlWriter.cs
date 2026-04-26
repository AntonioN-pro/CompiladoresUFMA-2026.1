using System.Text;

namespace JackAnalyzer;

public sealed class ParseXmlWriter
{
    private readonly StringBuilder _xml = new();
    private int _indent;

    public void Open(string tag)
    {
        WriteIndent();
        _xml.Append('<').Append(tag).AppendLine(">");
        _indent += 2;
    }

    public void Close(string tag)
    {
        _indent -= 2;
        WriteIndent();
        _xml.Append("</").Append(tag).AppendLine(">");
    }

    public void Token(ParserToken token)
    {
        WriteIndent();
        _xml.Append('<').Append(token.XmlTag).Append("> ")
            .Append(Escape(token.Value))
            .Append(" </").Append(token.XmlTag).AppendLine(">");
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, _xml.ToString(), Encoding.UTF8);
    }

    private void WriteIndent() => _xml.Append(' ', _indent);

    private static string Escape(string value) => value
        .Replace("&", "&amp;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;")
        .Replace("\"", "&quot;");
}
