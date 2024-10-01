using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



public interface IVariableStore<T>
{
    T this[Int32 index] { get; set; }
    void Set(Int32 index, T value);
    T Get(Int32 index);
}


internal class StringVariables : IVariableStore<String>
{

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private String[] Variables;

    public StringVariables(Int32 capacity = 1024)
    {
        Variables = new String[capacity];
    }

    public String this[Int32 index]
    {
        get
        {
            return Variables[index];
        }
        set
        {
            Variables[index] = value;
        }
    }

    public string Get(int index)
    {
        return Variables[index];
    }

    public void Set(Int32 index, String value)
    {
        Variables[index] = value;
    }

}




public static class GLOBAL
{









    public static readonly IVariableStore<String> STR = new StringVariables();




}
