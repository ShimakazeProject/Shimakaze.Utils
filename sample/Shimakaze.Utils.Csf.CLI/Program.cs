using System;

using Shimakaze.Utils.Csf;

if (args.Length < 1)
{
    Action<string> printf = Console.WriteLine;
    printf("<input> [<output>] [-v <byte>] [-b <int>]");
    printf("");
    printf("<input>  input file");
    printf("<output>  output file (Default is <inputWithoutExtension>.Extension)");
    printf("-b <int> Set Buffer Length (Default: 1024)");
    printf("-v <byte> Set Version (Default use the latest)");

    throw new ArgumentException("There are not enough parameters");
}

byte version = 0;
int buffer = 1024;
string input = string.Empty;
string output = string.Empty;

for (var i = 0; i < args.Length; i++)
{
    var arg = args[i];
    if (version == 0)
    {
        if (arg.Equals("-v"))
            version = byte.Parse(args[++i]);
        else if (arg.StartsWith("-v"))
            version = byte.Parse(arg[2..].TrimStart(':', '=', '-', '+'));

        if (version != 0)
            continue;
    }
    if (buffer == 1024)
    {
        if (arg.Equals("-b"))
            buffer = int.Parse(args[++i]);
        else if (arg.StartsWith("-b"))
            buffer = int.Parse(arg[2..].TrimStart(':', '=', '-', '+'));

        if (buffer != 10240)
            continue;
    }
    if (string.IsNullOrEmpty(input))
        input = arg;
    else if (string.IsNullOrEmpty(output))
        output = arg;

}
if (version == 0)
    await CsfUtils.Convert(input, output, buffer);
else
    await CsfUtils.Convert(input, output, buffer, version);