using System;
using System.Collections.Generic;
using System.IO;

namespace App.Core.Probability
{


    public sealed class LootGenerator<TValue>
    {
        private interface ILoot
        {
            double Probability { get; }
        }

        private class LootGroup : ILoot
        {
            public double Probability { get; set; }

            public List<ILoot> Items { get; set; } = new List<ILoot>();
        }

        private class LootItem : ILoot
        {
            public TValue Item { get; set; }
            public double Probability { get; set; }
            public int Quantity { get; set; }
            public LootItem(TValue item, double probability, int quantity)
            {
                Item = item;
                Probability = probability;
                Quantity = quantity;
            }
        }


        private LootGroup root = null;
        private readonly Random _random = new Random();


        public List<TValue> Generate(Double dropRate = 1.0)
        {
            List<TValue> generatedLoot = new List<TValue>();
            GenerateFromGroup(root, generatedLoot, dropRate);
            return generatedLoot;
        }

        private void GenerateFromGroup(LootGroup group, List<TValue> generatedLoot, Double dropRate = 1.0)
        {
            if (_random.NextDouble() / dropRate < group.Probability)
            {
                foreach (ILoot loot in group.Items)
                {
                    if (loot is LootItem item)
                    {
                        if (_random.NextDouble() / dropRate < item.Probability)
                        {
                            for (int i = 0; i < item.Quantity; i++)
                            {
                                generatedLoot.Add(item.Item);
                            }
                        }
                    }
                    else if (loot is LootGroup subGroup)
                    {
                        GenerateFromGroup(subGroup, generatedLoot, dropRate);
                    }
                }
            }

        }




        private class TxtLootTableParser
        {
            private Int32 currentIndex = 0;
            private String[] lines;

            public TxtLootTableParser(String[] lines)
            {
                this.lines = lines;
            }

            // 解析一个组
            public LootGenerator<String>.LootGroup Parse()
            {
                LootGenerator<String>.LootGroup group = new LootGenerator<String>.LootGroup();
                group.Probability = 1.0;
                while (currentIndex < lines.Length)
                {
                    string line = lines[currentIndex].Trim();
                    currentIndex++;

                    if (string.IsNullOrEmpty(line))
                        continue;
                    if (line.StartsWith("//"))
                        continue;

                    // 检查是否是一个子组的开始
                    if (line.StartsWith("{"))
                    {
                        LootGenerator<String>.LootGroup subGroup = Parse(); // 递归解析子组
                        group.Items.Add(subGroup);
                    }
                    // 检查是否是一个组的结束
                    else if (line.StartsWith("}"))
                    {
                        string[] probabilityParts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        group.Probability = probabilityParts.Length > 1 ? ParseProbability(probabilityParts[1]) : 1.0;
                        return group;
                    }
                    // 解析单个战利品物品
                    else
                    {
                        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        string itemName = parts[0];
                        double probability = ParseProbability(parts[1]);
                        int quantity = int.Parse(parts[2]);
                        LootGenerator<String>.LootItem item = new LootGenerator<String>.LootItem(itemName, probability, quantity);
                        group.Items.Add(item);
                    }
                }
                return group;
            }

            private static double ParseProbability(string probabilityStr)
            {
                string[] parts = probabilityStr.Split('/');
                return parts.Length == 2 ? (double)int.Parse(parts[0]) / int.Parse(parts[1]) : 1.0;
            }
        }

        public static LootGenerator<String> Load(String filename)
        {
            var lootGenerator = new LootGenerator<String>();
            var lines = File.ReadAllLines(filename);
            var parser = new TxtLootTableParser(lines);
            lootGenerator.root = parser.Parse();
            return lootGenerator;
        }

    }
}

