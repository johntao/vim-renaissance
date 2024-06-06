using VimReimagination.Model;
using VimReimagination.Service;
using Cmd = VimReimagination.Model.Commands.All;
internal class CommandInfo(Cmd code, string description, char qwertyKey, char dvorakKey, Action run)
{
  public readonly Cmd Code = code;
  public readonly string Description = description;
  public readonly char QwertyKey = qwertyKey;
  public readonly char DvorakKey = dvorakKey;
  public readonly Action Run = run;
  public char YourChoice { get; internal set; } = ' ';
}
internal class CommandService(
  IWindow win,
  IBufferService buffer,
  PatternMotionService.IGo pm,
  BasicMotionService.IGo bm
) : CommandService.IExe
{
  #region types and static
  internal interface IExe
  {
    IList<CommandInfo> List { get; }
    IList<char> QwertyKeys { get; }
    IList<char> DvorakKeys { get; }
  }
  private static CommandInfo[] _list = null!;
  private static char[] _dvorakKeys = null!;
  private static char[] _qwertyKeys = null!;
  #endregion
  private readonly BasicMotionService.IGo _bm = bm;
  private readonly PatternMotionService.IGo _pm = pm;
  private readonly IWindow _win = win;
  private readonly IBufferService _buffer = buffer;
  public IList<CommandInfo> List
  {
    get
    {
      _list ??= [
        new(Cmd.Row_Pattern_BigWordStart_Back,    "Move by pattern 'Start of Big Word'  , Back , Row dir", 'q', '"', () => _pm.Row(TextPattern.BigWordStart, Direction.RowBackward)),
        new(Cmd.Row_Pattern_BigWordEnd_Back,      "Move by pattern 'End of Big Word'    , Back , Row dir", 'w', '<', () => _pm.Row(TextPattern.BigWordEnd, Direction.RowBackward)),
        new(Cmd.Row_Pattern_BigWordStart_Forth,   "Move by pattern 'Start of Big Word'  , Forth, Row dir", 'e', '>', () => _pm.Row(TextPattern.BigWordStart, Direction.RowForward)),
        new(Cmd.Row_Pattern_BigWordEnd_Forth,     "Move by pattern 'End of Big Word'    , Forth, Row dir", 'r', 'p', () => _pm.Row(TextPattern.BigWordEnd, Direction.RowForward)),
        new(Cmd.Row_Pattern_SmallWordStart_Back,  "Move by pattern 'Start of Small Word', Back , Row dir", 'a', 'a', () => _pm.Row(TextPattern.SmallWordStart, Direction.RowBackward)),
        new(Cmd.Row_Pattern_SmallWordEnd_Back,    "Move by pattern 'End of Small Word'  , Back , Row dir", 's', 'o', () => _pm.Row(TextPattern.SmallWordEnd, Direction.RowBackward)),
        new(Cmd.Row_Pattern_SmallWordStart_Forth, "Move by pattern 'Start of Small Word', Forth, Row dir", 'd', 'e', () => _pm.Row(TextPattern.SmallWordStart, Direction.RowForward)),
        new(Cmd.Row_Pattern_SmallWordEnd_Forth,   "Move by pattern 'End of Small Word'  , Forth, Row dir", 'f', 'u', () => _pm.Row(TextPattern.SmallWordEnd, Direction.RowForward)),
        new(Cmd.Row_1unit_Back,  "Move 1 unit, Back , Row dir", 'h', 'd', () => _bm.Row(-1)),
        new(Cmd.Col_1unit_Forth, "Move 1 unit, Forth, Col dir", 'j', 'h', () => _bm.Col(1)),
        new(Cmd.Col_1unit_Back,  "Move 1 unit, Back , Col dir", 'k', 't', () => _bm.Col(-1)),
        new(Cmd.Row_1unit_Forth, "Move 1 unit, Forth, Row dir", 'l', 'n', () => _bm.Row(1)),
        new(Cmd.Row_FullScreen_Back_StopOnEdge,  "Move 1 full screen width , Back , Row dir, Stop on edge", 'H', 'D', () => _bm.RowStop(-_win.Width)),
        new(Cmd.Col_FullScreen_Forth_StopOnEdge, "Move 1 full screen height, Forth, Col dir, Stop on edge", 'J', 'H', () => _bm.Col(_win.Height)),
        new(Cmd.Col_FullScreen_Back_StopOnEdge,  "Move 1 full screen height, Back , Col dir, Stop on edge", 'K', 'T', () => _bm.Col(-_win.Height)),
        new(Cmd.Row_FullScreen_Forth_StopOnEdge, "Move 1 full screen width , Forth, Row dir, Stop on edge", 'L', 'N', () => _bm.RowStop(_win.Width)),
        new(Cmd.SmallDelete,     "Delete the character under the cursor", 'x', 'q', () => _buffer.SetChar(' ')),
        new(Cmd.SaveFile,        "Dump current buffer to file", 'z', ';', () => _buffer.SaveFile()),
      ];
      return _list;
    }
  }
  public IList<char> QwertyKeys
  {
    get
    {
      _qwertyKeys ??= List.Select(q => q.QwertyKey).ToArray();
      return _qwertyKeys;
    }
  }
  public IList<char> DvorakKeys
  {
    get
    {
      _dvorakKeys ??= List.Select(q => q.DvorakKey).ToArray();
      return _dvorakKeys;
    }
  }
}