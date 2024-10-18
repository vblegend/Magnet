using System;
using System.Collections.Generic;


namespace App.Core.Probability
{
    public class Loot<TValue>
    {
        internal Loot(TValue value, String[] parameters)
        {
            this.Value = value;
            this.Parameters = parameters;
        }
        public readonly TValue Value;
        public readonly String[] Parameters;
        public override string ToString()
        {
            return Value! + " " + String.Join(" ", Parameters);
        }
    }




    public sealed partial class LootGenerator<TValue>
    {
        private interface ILoot
        {
            Double Probability { get; }
        }

        private class LootGroup : ILoot
        {
            public Double Probability { get; set; }
            public List<ILoot> Children { get; private set; } = new List<ILoot>();
        }

        private class LootItem : ILoot
        {
            public Double Probability { get; private set; }
            public Loot<TValue> Item { get; private set; }
            public LootItem(Loot<TValue> item, double probability)
            {
                Item = item;
                Probability = probability;
            }
        }


        private LootGroup root = null;
        private readonly Random _random = new Random();


        public List<Loot<TValue>> Generate(Double dropRating = 1.0)
        {
            List<Loot<TValue>> generatedLoot = new List<Loot<TValue>>(64);
            GenerateFromGroup(root, generatedLoot, dropRating);
            return generatedLoot;
        }

        private void GenerateFromGroup(LootGroup group, List<Loot<TValue>> generatedLoot, Double dropRating = 1.0)
        {
            if (_random.NextDouble() / dropRating < group.Probability)
            {
                foreach (ILoot loot in group.Children)
                {
                    if (loot is LootItem item)
                    {
                        if (_random.NextDouble() / dropRating < item.Probability)
                        {
                            generatedLoot.Add(item.Item);
                        }
                    }
                    else if (loot is LootGroup subGroup)
                    {
                        GenerateFromGroup(subGroup, generatedLoot, dropRating);
                    }
                }
            }
        }


    }
}

