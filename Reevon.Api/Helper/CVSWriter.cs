using System.Reflection;
using System.Text;
using Reevon.Api.Extension;

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

    private void WriteHeader(PropertyInfo[] props)
    {
        foreach (PropertyInfo prop in props)
        {
            _sb.Append(prop.Name);
            _sb.Append(Separator);
        }
        _sb.Pop();
        _sb.AppendLine();
    }

    public string Write()
    {
        var props = typeof(T).GetProperties();
        WriteHeader(props);
        foreach (T element in _data)
        {
            foreach(PropertyInfo prop in props)
            {
                string value = prop.GetValue(element)?.ToString() ?? "";
                if(value.Contains(Separator))
                {
                    value = $"\"{value}\"";
                }
                _sb.Append(value);
                _sb.Append(Separator);
            }
            _sb.Pop();
            _sb.AppendLine();
        }
        return _sb.ToString();
    }
}