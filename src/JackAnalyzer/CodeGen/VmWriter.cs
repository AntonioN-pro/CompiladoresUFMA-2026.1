using System.Text;

namespace JackAnalyzer.CodeGen;

public sealed class VmWriter
{
    private readonly StringBuilder _code = new();
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public void WritePush(VmSegment segment, int index) => WriteLine($"push {MapSegment(segment)} {index}");
    public void WritePop(VmSegment segment, int index) => WriteLine($"pop {MapSegment(segment)} {index}");
    public void WriteArithmetic(string command) => WriteLine(command);
    public void WriteLabel(string label) => WriteLine($"label {label}");
    public void WriteGoto(string label) => WriteLine($"goto {label}");
    public void WriteIf(string label) => WriteLine($"if-goto {label}");
    public void WriteCall(string name, int nArgs) => WriteLine($"call {name} {nArgs}");
    public void WriteFunction(string name, int nLocals) => WriteLine($"function {name} {nLocals}");
    public void WriteReturn() => WriteLine("return");

    public void Save(string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        File.WriteAllText(outputPath, _code.ToString(), Utf8NoBom);
    }

    private void WriteLine(string line) => _code.AppendLine(line);

    private static string MapSegment(VmSegment segment) => segment switch
    {
        VmSegment.Constant => "constant",
        VmSegment.Argument => "argument",
        VmSegment.Local => "local",
        VmSegment.Static => "static",
        VmSegment.This => "this",
        VmSegment.That => "that",
        VmSegment.Pointer => "pointer",
        VmSegment.Temp => "temp",
        _ => throw new InvalidOperationException($"Segmento VM inválido: {segment}")
    };
}
