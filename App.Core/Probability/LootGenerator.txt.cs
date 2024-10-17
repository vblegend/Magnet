using System;
using System.IO;

namespace App.Core.Probability
{


    public sealed partial class LootGenerator<TValue>
    {



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
                        group.Children.Add(subGroup);
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
                        var parameters = parts[2..];
                        var loot = new Loot<String>(itemName, parameters);
                        group.Children.Add(new LootGenerator<String>.LootItem(loot, probability));
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
