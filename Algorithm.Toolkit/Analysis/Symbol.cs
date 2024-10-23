namespace Toolkit.Private.Analysis
{
    public class Symbol
    {
        public static readonly Symbol EOF = new Symbol("END OF FILE", SymbolTypes.Operator);
        /// <summary>
        /// get symbol name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// get symbol type
        /// </summary>
        internal SymbolTypes Type { get; private set; }

        public Symbol(string name, SymbolTypes type)
        {
            Name = name;
            Type = type;
        }

        public override string ToString()
        {
            return $"{Name}:{Type}";
        }
    }
}
