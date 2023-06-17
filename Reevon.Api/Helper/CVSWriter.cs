using System.Reflection;
using System.Text;

namespace Reevon.Api.Helper;

public class CVSWriter<T> where T : class
{
    private readonly List<T> _data;

    private StringBuilder _sb = new();

    public char Separator { get; set; } = ',';

    public CVSWriter(List<T> data)
    {
        _data = data;
    }

    private void WriteHeader()
    {
        var props = typeof(T).GetProperties();
        foreach (PropertyInfo prop in props)
        {
            _sb.Append(prop.Name);
            _sb.Append(Separator);
        }
        _sb.AppendLine();
    }

    public string Write()
    {
        var props = typeof(T).GetProperties();
        WriteHeader();
        foreach (T element in _data)
        {
            foreach(PropertyInfo prop in props)
            {
                string values = prop.GetValue(element)?.ToString() ?? "";
                _sb.Append(values);
                _sb.Append(Separator);
            }
            _sb.AppendLine();
        }
        return _sb.ToString();
    }
}