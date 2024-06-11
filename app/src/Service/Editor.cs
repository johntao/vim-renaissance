namespace VimReimagination.Service;
using VimReimagination.Model;
/// <summary>
/// </summary>
internal class Editor(
  IReadWrite rw,
  IBuffer buffer,
  StatusBar.IWrite status
) : Editor.IRun
{
  #region types and static
  internal interface IRun
  {
    void Run(Dictionary<char, CommandInfo> keymap);
  }
  static Editor() { }
  #endregion
  private readonly IBuffer _buffer = buffer;
  private readonly IReadWrite _rw = rw;
  private readonly StatusBar.IWrite _status = status;
  public void Run(Dictionary<char, CommandInfo> keymap)
  {
    while (true)
    {
      _buffer.IfWindowResizedThenReloadBuffer();
      var readkey = _rw.ReadKey();
      var keychar = readkey.KeyChar;
      if (keymap.TryGetValue(keychar, out var cmd))
      {
        cmd.Run();
        _status.Write($"{keychar} --> {cmd.Code} ");
      }
    }
  }
}