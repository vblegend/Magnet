namespace Magnet.Context
{
    public sealed class ScriptAttribute : Attribute
    {
        public ScriptAttribute()
        {
        }

        public ScriptAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
