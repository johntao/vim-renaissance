using Cmd = VimReimagination.NormalCommand;
namespace VimReimagination.Service;
internal class CustomizingKeymapTask(ITextRenderer tr, ITableRenderer tbl) : ICustomizingKeymapTask
{
  private static readonly CommandInfo[] _stuff =
  [
    new(Cmd.MoveHorizontalByPatternBigWordStartBackward, "Move horizontally by pattern 'Start of Big Word' backward", 'q', '"'),
    new(Cmd.MoveHorizontalByPatternBigWordEndBackward, "Move horizontally by pattern 'End of Big Word' backward", 'w', '<'),
    new(Cmd.MoveHorizontalByPatternBigWordStartForward, "Move horizontally by pattern 'Start of Big Word' forward", 'e', '>'),
    new(Cmd.MoveHorizontalByPatternBigWordEndForward, "Move horizontally by pattern 'End of Big Word' forward", 'r', 'p'),
    new(Cmd.MoveHorizontalByPatternSmallWordStartBackward, "Move horizontally by pattern 'Start of Small Word' backward", 'a', 'a'),
    new(Cmd.MoveHorizontalByPatternSmallWordEndBackward, "Move horizontally by pattern 'End of Small Word' backward", 's', 'o'),
    new(Cmd.MoveHorizontalByPatternSmallWordStartForward, "Move horizontally by pattern 'Start of Small Word' forward", 'd', 'e'),
    new(Cmd.MoveHorizontalByPatternSmallWordEndForward, "Move horizontally by pattern 'End of Small Word' forward", 'f', 'u'),
    new(Cmd.MoveHorizontal1uBackward, "Move horizontally by 1 unit backward", 'h', 'd'),
    new(Cmd.MoveVertical1uForward, "Move vertically by 1 unit forward", 'j', 'h'),
    new(Cmd.MoveVertical1uBackward, "Move vertically by 1 unit backward", 'k', 't'),
    new(Cmd.MoveHorizontal1uForward, "Move horizontally by 1 unit forward", 'l', 'n'),
    new(Cmd.MoveHorizontalFullScreenBackwardStop, "Move horizontally by full screen width backward, stop at the edge", 'H', 'D'),
    new(Cmd.MoveVerticalFullScreenForwardStop, "Move vertically by full screen height forward, stop at the edge", 'J', 'H'),
    new(Cmd.MoveVerticalFullScreenBackwardStop, "Move vertically by full screen height backward, stop at the edge", 'K', 'T'),
    new(Cmd.MoveHorizontalFullScreenForwardStop, "Move horizontally by full screen width forward, stop at the edge", 'L', 'N'),
    new(Cmd.MoveHorizontal45uBackward, "Experimental, move horizontally by 45 units backward", 'n', 'b'),
    new(Cmd.MoveHorizontal45uForward, "Experimental, move horizontally by 45 units forward", '.', 'v'),
  ];
  static readonly char[] _dvorakKeys = _stuff.Select(q => q.DvorakKey).ToArray();
  static readonly char[] _qwertyKeys = _stuff.Select(q => q.QwertyKey).ToArray();
  private readonly ITextRenderer _tr = tr;
  private readonly ITableRenderer _tbl = tbl;

  public Dictionary<char, Cmd> Run(ChoosingKeymapTaskResult result)
  {
    var normalCommands = _stuff.Select(q => q.Command).ToArray();
    return result switch
    {
      ChoosingKeymapTaskResult.UseDefaultQwerty => _qwertyKeys.Zip(normalCommands).ToDictionary(),
      ChoosingKeymapTaskResult.MapQwertyToDvorak => _dvorakKeys.Zip(normalCommands).ToDictionary(),
      ChoosingKeymapTaskResult.MapByUser => MapByUser().Zip(normalCommands).ToDictionary(),
      _ => throw new InvalidOperationException(),
    };
  }
  private char[] MapByUser()
  {
    _tr.CursorVisible = false;
    const string Message = """
Press any key to map the following command to that key
Press Enter to cancel mapping and map the rest using QWERTY
Press Backspace to cancel mapping and map the rest using Dvorak
Press arrow keys to navigate
""";
    _tr.WriteLine(Message);
    _tbl.Initialize(To5ColTable(_stuff));
    var isLooping = true;
    var useQwerty = false;
    while (isLooping)
    {
      var readkey = _tr.ReadKey();
      var top = _tr.CursorTop;
      switch (readkey)
      {
        case var q when q.Key is ConsoleKey.UpArrow:
          if (top <= _tbl.StartLineIdx) break;
          _tr.Write(' ');
          --_tr.CursorTop;
          _tr.Write('>');
          break;
        case var q when q.Key is ConsoleKey.DownArrow:
          if (top >= _tbl.EndLineIdx) break;
          _tr.Write(' ');
          ++_tr.CursorTop;
          _tr.Write('>');
          break;
        case var q when !char.IsControl(q.KeyChar):
          var currentIdx = top - _tbl.StartLineIdx;
          var hasConflict = _stuff.Any((item, idx) => item.YourChoice == readkey.KeyChar && idx != currentIdx);
          if (hasConflict)
          {
            _tbl.UpdateChoice("!!");
            break;
          }
          var item = _stuff[currentIdx];
          item.YourChoice = readkey.KeyChar;
          _tbl.UpdateChoice(item.YourChoice + " ");
          if (top >= _tbl.EndLineIdx) break;
          _tr.Write(' ');
          ++_tr.CursorTop;
          _tr.Write('>');
          break;
        case var q when q.Key is ConsoleKey.Backspace:
          isLooping = false;
          break;
        case var q when q.Key is ConsoleKey.Enter:
          useQwerty = true;
          isLooping = false;
          break;
      }
    }
    _tr.CursorVisible = true;
    return _stuff.Select(q =>
    {
      if (q.YourChoice == ' ')
        q.YourChoice = useQwerty ? q.QwertyKey : q.DvorakKey;
      return q.YourChoice;
    }).ToArray(); ;
  }
  internal static IEnumerable<string[]> To5ColTable(CommandInfo[] stuffs)
  {
    yield return new[] { "NormalCommand", "Description", "Qwerty", "Dvorak", "YourChoice" };
    foreach (var item in stuffs)
    {
      yield return new[]
      {
        item.Command.ToString(),
        item.Description,
        item.QwertyKey.ToString(),
        item.DvorakKey.ToString(),
        item.YourChoice.ToString(),
      };
    }
  }
  internal class CommandInfo(Cmd command, string description, char qwertyKey, char dvorakKey)
  {
    public Cmd Command { get; } = command;
    public string Description { get; } = description;
    public char QwertyKey { get; } = qwertyKey;
    public char DvorakKey { get; } = dvorakKey;
    public char YourChoice { get; internal set; } = ' ';
  }
}
internal interface ICustomizingKeymapTask
{
  Dictionary<char, Cmd> Run(ChoosingKeymapTaskResult result);
}