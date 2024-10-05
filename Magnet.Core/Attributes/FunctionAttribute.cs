namespace Magnet.Core
{


    /// <summary>
    /// Define a method that can be called by another script or host
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class FunctionAttribute : Attribute
    {


        public FunctionAttribute()
        {
   
        }
        public FunctionAttribute(string alias)
        {
            this.Alias = alias;
        }

        public string Alias { get; set; }
    }
}
