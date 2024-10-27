
<p align="center" > <span>Magnet</span> </p>
<div align=center><img src="icon.png"></div>

# What is Magnet?
--------------
我原本想做的仅仅是一款游戏的服务器脚本引擎，从一开始想做一个强类型的类TypeScript的脚本引擎<br/>
后面从词法分析到语法分析再生成语法树，做完之后发现编译器后端的复杂程度过高。 即使编译器后端开发完成性能和扩展性也很难达到我的要求<br/>
所以尝试使用Roslyn引擎来定制一款C#脚本引擎，使用Roslyn的好处就是完全不用担心他的性能和扩展性，Roslyn提供了完整的语法树API<br/>
和强大的编译选项搭配上C#的语法和特性完全可以实现一款满足我需求的脚本。<br/>
之所以取名叫做Magnet就是希望他可以像磁铁一样吸到宿主的Project上可以随时取下来。<br/>
当然它不仅能用做游戏的服务器逻辑处理，它可以用作任何需要它的地方。<br/>
当前处于开发阶段，所以部分API可能会有改动, 例子可能导致编译失败，请查看例子源码自行修改。<br/>

🎉如果您的使用过程中发现Bug或有合理的需求欢迎提Issues 或 创建PullRequests

--------------

`C#语法` `高性能` `可扩展` `可调试` `可卸载` `多State` `安全性`

--------------


## 💥使用说明
脚本对象必须直接或间接继承或派生自AbstractScript，且必须使用[ScriptAttribute]标记Class<br/>
每个脚本State都是一个包含所有独立脚本对象实例的对象集，<br/>
每个MagnetScript下所有脚本State都处于同一个AssemblyLoadContext下<br/>
静态变量是所有State都可以访问的,所以在脚本中应慎重使用静态变量。

``` csahrp
// ✔️正确案例
[Script]
public class ScriptExample : AbstractScript
{
    [Function]
    public void Hello(String name)
    {
        this.Output(MessageType.Print, $"Hello {name}!");
    }
}


// ✖️错误案例
[Script]
public class ScriptExample
{
    public void Hello(String name)
    {
        this.Output(MessageType.Print, $"Hello {name}!");
    }
}

// ✖️错误案例
public class ScriptExample : AbstractScript
{
    public void Hello(String name)
    {
        this.Output(MessageType.Print, $"Hello {name}!");
    }
}



```


## 💥脚本基础功能
脚本编译的基础可选项。
``` csahrp
// 脚本名称
options.WithName(name);

// 脚本是否支持异步
options.WithAllowAsync(false);

// 脚本是否支持不安全代码
options.WithAllowUnsafe(true);

// 使用默认的编译抑制诊断
options.UseDefaultSuppressDiagnostics();

// 脚本程序集上下文依赖程序集加载Hook
options.WithAssemblyLoadCallback(AssemblyLoad);

```



## 💥脚本编译输入与输出
支持仅编译、仅加载、从脚本编译加载模式。
``` csahrp
// #1 仅编译，可输出
options.WithCompileKind(CompileKind.Compile);
options.WithOutPutFile("sample.dll");

// #2 从程序集文件加载
options.WithCompileKind(CompileKind.LoadAssembly);
options.WithScanDirectory("./");
options.WithAssemblyFileName("sample.dll");

// #3 从脚本文件编译并加载
options.WithCompileKind(CompileKind.CompileAndLoadAssembly);
options.WithScanDirectory("../../../../Scripts");
```

## 💥脚本编译优化与设置

``` csahrp

// 调试模式 启用脚本内置debugger()函数
options.WithDebug(true);

// 调试模式 不启用脚本内置debugger()函数
options.WithDebug(false);

// 发布模式 编译优化
//options.WithRelease();
```

## 💥添加脚本的程序集引用

``` csahrp
// 添加 System.Threading 程序集的引用
options.AddReferences<Thread>();

// 添加 System.Threading 程序集的引用
options.AddReferences(typeof(Thread));

// 添加 System.Threading 程序集的引用
options.AddReferences("System.Threading.dll");
```


## 💥带有编译检查的类型与命名空间禁用
如果脚本中使用了被禁用的类型或命名空间后，将会触发编译失败。 <br>
ICompileResult.Diagnostics 内会包含诊断错误 同时 ICompileResult.Success = false
``` csahrp
//禁用类型
options.DisableType(typeof(Task));

// 禁用泛类型的严格类型
options.DisableType("System.Collections.Generic.List<string>");
options.DisableType(typeof(List<String>));

// 禁用范类型的基础类型
options.DisableType("System.Collections.Generic.List");
options.DisableGenericBaseType(typeof(List<>));
```



## 💥对象类型替换器（开发中）
在编译脚本阶段，将语法树上的类型替换为新的类型。<br>
如果新类型的成员对象签名与原类型的不一致可能会抛出异常。
``` csahrp
// 替换类型 将脚本内使用的Task 替换为MyTask
options.AddReplaceType(typeof(Task), typeof(MyTask));

// 脚本类型重写器（加强版的AddReplaceType）
options.WithTypeRewriter(new TypeRewriter());

// 类型重写器
internal class TypeRewriter : ITypeRewriter
{
    public bool RewriteType(ITypeSymbol typeSymbolm, out Type newType)
    {
        if (typeSymbolm.ToDisplayString() == "System.Threading.Thread")
        {
            newType = typeof(NewThread);
            return true;
        }
        newType = null;
        return false;
    }
}
```


## 💥类型处理顺序
脚本语法树类型处理逻辑顺序如下：
1. 尝试替换脚本中使用的类型
2. 尝试使用类型重写器重写类型
3. 禁用命名空间检查(包括using和类型的命名空间)
4. 禁用类型检查





## 💥功能扩展分析器
分析器实现了以下三个分析器接口，宿主可以通过分析器实现定制功能开发

`完整例子查看 Magnet.Examples 的 App.Core.Timer.TimerProvider`

| 分析器 | 描述 | 触发时机 |
| ----------- | ----------- | ----------- | 
| IAssemblyAnalyzer | 脚本程序集分析器 | 脚本程序集加载完毕后 | 
| ITypeAnalyzer | Script类型分析器 | 脚本程序集加载完毕后 | 
| IInstanceAsalyzer | 和脚本实例分析器 | 脚本State创建时 | 


``` csahrp
var timerProvider = new TimerProvider();

// 增加一个分析器
options.AddAnalyzer(timerProvider);


public class TimerProvider : ITypeAnalyzer
{

    void ITypeAnalyzer.DefineType(Type type)
    {
    }

    void IAnalyzer.Connect(MagnetScript magnet)
    {

    }

    void IAnalyzer.Disconnect(MagnetScript magnet)
    {
    }
}

```



## 💥脚本依赖注入
Magnet实现了简单的依赖注入功能，支持依赖的Type和Name匹配。

1.全局依赖注入
``` csahrp
// 注册依赖注入
options.RegisterProvider(timerProvider);
options.RegisterProvider<ObjectKilledContext>(new ObjectKilledContext());
options.RegisterProvider(GLOBAL);
options.RegisterProvider<IObjectContext>(new HumContext(), "SELF");
```

2.State级别依赖注入，继承了全局依赖
```csharp
var stateOptions = StateOptions.Default;
stateOptions.Identity = 666;
stateOptions.RegisterProvider(new TimerService());
var stateTest = scriptManager.CreateState(stateOptions);
```
3.脚本间依赖注入，脚本实例创建后会自动注册进State级别的依赖注入列表内


脚本
``` csahrp

[Script]
public class ScriptA : AbstractScript
{
    // 脚本实例依赖注入
    [Autowired]
    private readonly ScriptB scriptB;

    protected override void Initialize()
    {
        Print(scriptB)
    }

}

[Script]
public class ScriptB : AbstractScript
{
    [Autowired(typeof(GlobalVariableStore))]
    protected readonly GlobalVariableStore GLOBAL;

    [Autowired("SELF")]
    protected readonly IObjectContext Player;

    [Autowired]
    private readonly ITimerManager timerManager;

    // 脚本实例依赖注入
    [Autowired]
    private readonly ScriptA scriptA;

    protected override void Initialize()
    {
        Print(scriptA)
    }

}

```


## 💥脚本编译诊断抑制
脚本支持编译诊断抑制的默认等级设置，支持C#编译器的全量诊断代码和以下*Magnet*内置编译诊断

| 诊断ID | 默认级别 | 描述 |
| :-----------: | :-----------: | ----------- | 
| SW001 | Warning | 无效的脚本对象，检测到对象继承了AbstractScript 但未被[ScriptAttribute]标记 | 
| SW002 | Warning | 无效的脚本对象，检测到对象被[ScriptAttribute]标记 但未继承AbstractScript对象 | 
| SW003 | Warning | 不明确的全局字段，字段被标记为static但未被[GlobalAttribute]标记 | 
| SW004 | Warning | 不明确的全局属性，属性被标记为static但未被[GlobalAttribute]标记 | 
| SE001 | Error   | 脚本不允许使用异步操作符号async await | 
| SE002 | Error   | 被禁止使用的命名空间 | 
| SE003 | Error   | 被禁止使用的类型 | 
| SE004 | Error   | 脚本对象禁止实现构造函数 | 
| SE005 | Error   | 脚本对象禁止实现析构函数 | 





``` csharp
// 将SW001诊断提升至Error，没有标记[ScriptAttribute]的脚本对象会导致编译失败
options.AddDiagnosticSuppress("SW001", Microsoft.CodeAnalysis.ReportDiagnostic.Error);

// 将SW002诊断提升至Error，没有继承AbstractScript的脚本对象会导致编译失败
options.AddDiagnosticSuppress("SW002", Microsoft.CodeAnalysis.ReportDiagnostic.Error);

// 将SW003诊断提升至Error，没有标记[GlobalAttribute]的静态字段会导致编译失败
options.AddDiagnosticSuppress("SW003", Microsoft.CodeAnalysis.ReportDiagnostic.Error);

// 将SW004诊断提升至Error，没有标记[GlobalAttribute]的静态属性会导致编译失败
options.AddDiagnosticSuppress("SW004", Microsoft.CodeAnalysis.ReportDiagnostic.Error);
```


## 💥脚本的生命周期
脚本需要继承 AbstractScript

``` csharp
// 脚本初始化，在所有脚本实例创建完成之后且依赖注入完毕之后执行。
protected override void Initialize();

// 脚本停止工作，脚本被Dispose()时或MagnetScript实例调用 Unload(true) 时触发
// 触发该方法后脚本将不可用
protected override void Shutdown();
```



## 💥脚本的输出流
AbstractScript 实现了Output函数以实现输出消息至宿主。

``` csharp
// App.Core 内的例子 实现输出调试信息至输出流
[Conditional("DEBUG")]
public void Debug(Object @object, [CallerFilePath] String callFilePath = null, [CallerLineNumber] Int32 callLineNumber = 0, [CallerMemberName] string callMethod = null)
{
    this.Output(MessageType.Debug, $"{callFilePath}({callLineNumber}) [{callMethod}] => {@object}");
}
```


## 💥脚本之间相互调用
因每个State内的脚本对象是动态创建的，所以脚本对象初始状态下是离散的。<br>
`State与State之间的脚本对象由对象实例进行隔离，无法互相调用，但是可以通过静态变量或全局变量进行交互。`<br>
State中脚本对象之间的调用有以下4种办法。<br>
<br>
1.脚本之间的依赖注入，参考上方的依赖注入（推荐）<br>
2.题本提供的Script方法获取脚本实例<br>
3.脚本提供的Call方法（不推荐、早期API）<br>
4.脚本提供的TryCall方法（不推荐、早期API）

``` csharp
// 调用ScriptB的Test方法，出现错误会抛出异常
Call("ScriptB", "Test", []);

// 尝试调用ScriptB的PrintMessage方法，出现任何错误均不会抛出异常
TryCall("ScriptB", "PrintMessage", ["Help"]);

// 调用ScriptB的PrintMessage方法（支持强类型签名）出现错误会抛出异常
Script<ScriptB>().PrintMessage("AAA");

// 当脚本ScriptB存在时调用ScriptB的PrintMessage方法（支持强类型签名）出现错误会抛出异常
Script<ScriptB>((script) =>
{
    script.PrintMessage("BBB");
});
```

## 💥脚本调试断点
使用Debug模式编译运行脚本时，执行到`debugger()`将自动打开调试器并断点暂停。<br>
Release模式编译时此函数将被编译器优化掉
``` csharp
debugger();
```


## 💥全局变量定义
由于C#的特性通过 static 定义的方法、属性、字段 均可被所有State内使用<br>
所以为了不混淆全局变量与静态变量，增加了`GlobalAttribute` 属性标签<br>
当字段或属性声明为static时，如果未标记[Global]属性，则编译时会产生编译警告但不影响正常运行。

``` csharp
[Global]
[Autowired(typeof(GlobalVariableStore))]
protected readonly static GlobalVariableStore Global;

```



## 💥宿主调用脚本内方法
为保障脚本的可卸载性，脚本的方法委托或实例均以WeakReference返回。<br>
宿主使用MethodDelegate调用方法时，脚本内方法必须被[Function]属性标记<br>
ScriptAs方式获取接口实例则不需要<br>


``` csharp

// 尝试获取stateTest内第一个实现了IPlayLifeEvent接口的脚本对象(推荐)
var weakPlayerLife = stateTest.ScriptAs<IPlayLifeEvent>();
if (weakPlayerLife != null && weakPlayerLife.TryGetTarget(out var lifeEvent))
{   
    // 调用脚本的OnOnline方法
    lifeEvent.OnOnline(null);
    lifeEvent = null;
}

// 创建 stateTest中脚本ScriptA的Main方法委托(推荐)
var weakMain = stateTest.MethodDelegate<Action>("ScriptA", "Main");
if (weakMain != null && weakMain.TryGetTarget(out var main))
{
    // 调用脚本Main方法
    main();
    main = null;
}

// 创建脚本ScriptExample中属性Target的Getter委托
var weakGetter = state?.PropertyGetterDelegate<Double>("ScriptExample", "Target");
if (weakGetter != null && weakGetter.TryGetTarget(out var getter))
{
    // 获取脚本ScriptExample中属性Target值
    Console.WriteLine(getter());
    getter = null;
}


// 创建脚本ScriptExample中属性Target的Setter委托
var weakSetter = state?.PropertySetterDelegate<Double>("ScriptExample", "Target");
if (weakSetter != null && weakSetter.TryGetTarget(out var setter))
{
    // 对脚本ScriptExample中属性Target赋值
    setter(123.45);
    setter = null;
}
```




## 💥脚本卸载
脚本卸载是不可控的，因为dotnet中的程序集卸载是由GC来决定的。<br>
宿主程序中保留脚本内类型的强引用时将会导致卸载失败。

``` csharp
// 卸载脚本，不会销毁所有state，由用户自己选择时机Dispose()
scriptManager.Unload();

// 强制卸载脚本，会销毁所有state
scriptManager.Unload(true);

// 申请内存 触发GC 卸载脚本
while (scriptManager.Status == ScrriptStatus.Unloading && scriptManager.IsAlive)
{
    //GC
    var obj = new byte[1024 * 1024];
    Thread.Sleep(10);
}
```







--------------



# 💥Examples

完整例子请查看 Magnet.Examples 或 Magnet.Test
``` csharp
    private static ScriptOptions Options(String name)
    {
        ScriptOptions options = ScriptOptions.Default;
        // 脚本名称
        options.WithName(name);
        // 调试模式 不启用脚本内置debugger()函数
        options.WithDebug(false);
        // 发布模式 编译优化
        //options.WithRelease();


        // #1 仅编译，可输出
        options.WithCompileKind(CompileKind.Compile);
        options.WithOutPutFile("123.dll");

        // #2 从程序集文件加载
        options.WithCompileKind(CompileKind.LoadAssembly);
        options.WithScanDirectory("./");
        options.WithAssemblyFileName("123.dll");

        // #3 从脚本文件编译并加载
        options.WithCompileKind(CompileKind.CompileAndLoadAssembly);
        options.WithScanDirectory("../../../../Scripts");

        // 定义自定义的编译宏符号
        options.WithCompileSymbols("USE_FILE");

        // 是否支持异步
        options.WithAllowAsync(false);

        // 添加程序集引用
        options.AddReferences<GameScript>();

        var timerProvider = new TimerProvider();
        // 增加一个分析器
        options.AddAnalyzer(timerProvider);

        // 是否支持不安全代码
        options.WithAllowUnsafe(true);

        // 替换类型
        // options.AddReplaceType(typeof(Task), typeof(MyTask));

        //禁用类型
        options.DisableType(typeof(Task));

        // 禁用泛类型的严格类型
        options.DisableType("System.Collections.Generic.List<string>");
        options.DisableType(typeof(List<String>));

        // 禁用范类型的基础类型
        options.DisableType("System.Collections.Generic.List");
        options.DisableGenericBaseType(typeof(List<>));

        // 禁用命名空间
        options.DisableNamespace(typeof(Thread));

        //禁用不安全类型与命名空间
        //options.DisableInsecureTypes();

        // 脚本类型重写器
        options.WithTypeRewriter(new TypeRewriter());
        // 使用默认的抑制诊断
        options.UseDefaultSuppressDiagnostics();
        // 脚本上下文依赖程序集加载Hook
        options.WithAssemblyLoadCallback(AssemblyLoad);
        // 注册依赖注入
        options.RegisterProvider(timerProvider);
        options.RegisterProvider<ObjectKilledContext>(new ObjectKilledContext());
        options.RegisterProvider(GLOBAL);
        options.RegisterProvider<IObjectContext>(new HumContext(), "SELF");

        return options;
    }




    private static WeakReference<Action> TestScriptUnload()
    {
        MagnetScript scriptManager = new MagnetScript(Options("Unload.Test"));
        var result = scriptManager.Compile();
        if (!result.Success)
        {
            foreach (var item in result.Diagnostics)
            {
                Console.WriteLine(item.ToString());
            }
            return null;
        }
        var state = scriptManager.CreateState();
        var weak = state.MethodDelegate<Action>("ScriptExample", "Hello");
        state.Dispose();
        scriptManager.Unload();
        return weak;
    }

    public static void Main()
    {
        MagnetScript scriptManager = new MagnetScript(Options("My.Script"));
        scriptManager.Unloading += ScriptManager_Unloading;
        scriptManager.Unloaded += ScriptManager_Unloaded;

        var result = scriptManager.Compile();
        foreach (var diagnostic in result.Diagnostics)
        {
            Console.WriteLine(diagnostic.ToString());
        }
        if (result.Success)
        {
            var stateOptions = StateOptions.Default;
            stateOptions.RegisterProvider(new TimerService());
            var stateTest = scriptManager.CreateState(stateOptions);
            var weakMain = stateTest.MethodDelegate<Action>("ScriptA", "Main");
            if (weakMain != null && weakMain.TryGetTarget(out var main))
            {
                using (new WatchTimer("With Call Main()")) main();
                main = null;
            }

            var weakPlayerLife = stateTest.ScriptAs<IPlayLifeEvent>();
            if (weakPlayerLife != null && weakPlayerLife.TryGetTarget(out var lifeEvent))
            {
                using (new WatchTimer("With Call OnOnline()")) lifeEvent.OnOnline(null);
                lifeEvent = null;
            }
            stateTest = null;
            scriptManager.Unload(true);
        }
        // wait gc unloaded assembly
        while (scriptManager.Status == ScriptStatus.Unloading && scriptManager.IsAlive)
        {
            var obj = new byte[1024 * 1024];
            Thread.Sleep(10);
        }
    }

    private static void ScriptManager_Unloaded(MagnetScript obj)
    {
        Console.WriteLine($"脚本[{obj.Name}:{obj.UniqueId}]卸载完毕.");
    }

    private static void ScriptManager_Unloading(MagnetScript obj)
    {
        Console.WriteLine($"脚本[{obj.Name}:{obj.UniqueId}]卸载请求.");
    }

```


# Script Examples|脚本例子

``` csharp
    using Magnet.Core;
    using System;



    // A usable script must meet three requirements.
    // 1. The access must be public
    // 2. The [ScriptAttribute] must be marked
    // 3. The AbstractScript class must be inherited

    [Script(nameof(ScriptExample))]
    public class ScriptExample : AbstractScript
    {
        [Autowired("SELF")]
        protected readonly IObjectContext? SELF;

        [Autowired]
        protected readonly GlobalVariableStore? GLOBAL;

        [Function("Hello")]
        public void Hello()
        {
            this.PRINT($"Hello Wrold!");

            // call script method
            Call("ScriptB", "Test", []);
            Call("ScriptB", "PrintMessage", ["Help"]);
            TryCall("ScriptB", "PrintMessage1", ["Help"]);
            Script<ScriptB>().PrintMessage("AAA");
            Script<ScriptB>((script) =>
            {
                script.PrintMessage("BBB");
            });

        }



        public Double Target
        {
            get
            {
                return 3.14;
            }
            set
            {
                this.PRINT(value);
            }
        }
    }
```
