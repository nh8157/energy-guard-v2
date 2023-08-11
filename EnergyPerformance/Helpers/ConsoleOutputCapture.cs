using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyPerformance.Helpers;
public class ConsoleOutputCapture: IDisposable
{
    private readonly StringWriter _stringWriter;
    private readonly TextWriter _originalOutput;

    public ConsoleOutputCapture()
    {
        _stringWriter = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_stringWriter);
    }

    public string GetCapturedOutput()
    {
        return _stringWriter.ToString();
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _stringWriter.Dispose();
    }
}
