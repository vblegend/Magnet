﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;



public interface IVariableStore<T>
{
    T this[Int32 index] { get; set; }
    void Set(Int32 index, T value);
    T Get(Int32 index);
}

internal class ClassVariables<TObject> : IVariableStore<TObject>
{

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private TObject[] Variables;

    public ClassVariables(Int32 capacity = 1024)
    {
        Variables = new TObject[capacity];
    }

    public TObject this[Int32 index]
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

    public TObject Get(int index)
    {
        return Variables[index];
    }

    public void Set(Int32 index, TObject value)
    {
        Variables[index] = value;
    }

}



public class GlobalVariableStore 
{
    public readonly IVariableStore<String> S = new ClassVariables<String>();
    public readonly IVariableStore<Int64> I = new ClassVariables<Int64>();
    public readonly IVariableStore<DateTime> T = new ClassVariables<DateTime>();
    public readonly IVariableStore<Boolean> B = new ClassVariables<Boolean>();












}