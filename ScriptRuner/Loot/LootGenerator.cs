using System;
using System.Collections.Generic;


namespace ScriptRuner.Loot
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



    /// <summary>
    /// ս��Ʒ������
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public sealed partial class LootGenerator<TValue>
    {
        private interface ILoot
        {

            /// <summary>
            /// ����
            /// </summary>
            Double Probability { get; }

            /// <summary>
            /// ����� ���ܱ���Ӱ��
            /// </summary>
            Boolean Strict { get; }
        }

        private class LootGroup : ILoot
        {
            public Boolean Strict { get; set; }
            public Double Probability { get; set; }
            public List<ILoot> Children { get; private set; } = new List<ILoot>();
        }

        private class LootItem : ILoot
        {
            public Boolean Strict { get; set; }
            public Double Probability { get; private set; }
            public Loot<TValue> Item { get; private set; }

            public LootItem(Loot<TValue> item, double probability, Boolean strict)
            {
                Item = item;
                this.Strict = strict;
                Probability = probability;
            }
        }


        private LootGroup root = null;
        private readonly Random _random = new Random();


        /// <summary>
        /// �������ս��Ʒ�б�
        /// </summary>
        /// <param name="dropRating">��̬�����ʣ�10 Ϊ10�����ʣ� 0.1Ϊ  1/10 ����</param>
        /// <returns></returns>
        public List<Loot<TValue>> Generate(Double dropRating = 1.0)
        {
            List<Loot<TValue>> generatedLoot = new List<Loot<TValue>>(64);
            GenerateFromGroup(root, generatedLoot, dropRating);
            return generatedLoot;
        }

        private void GenerateFromGroup(LootGroup group, List<Loot<TValue>> generatedLoot, Double dropRating = 1.0)
        {
            var value = group.Strict ? _random.NextDouble() : _random.NextDouble() / dropRating;
            if (value < group.Probability)
            {
                foreach (ILoot loot in group.Children)
                {
                    if (loot is LootItem item)
                    {
                        value = loot.Strict ? _random.NextDouble() : _random.NextDouble() / dropRating;
                        if (value < item.Probability)
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

