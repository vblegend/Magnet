using System;
using System.Collections.Generic;
using System.IO;

namespace App.Core.Probability
{


    public sealed partial class LootGenerator<TValue>
    {



        private class TxtLootTableParser
        {
            private Int32 currentIndex = 0;
            private String[] lines;
            private String Directory = "";
            public TxtLootTableParser(String filename)
            {
                this.lines = File.ReadAllLines(filename);
                this.Directory = Path.GetDirectoryName(filename);
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

                    if (line.StartsWith("#include"))
                    {
                        var fileName = line.Substring(8).Trim();
                        var parser = new TxtLootTableParser(Path.Combine(Directory, fileName));
                        var root = parser.Parse();
                        group.Children.AddRange(root.Children);
                    }
                    // 检查是否是一个子组的开始
                    else if (line.StartsWith("{"))
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
            var parser = new TxtLootTableParser(filename);
            lootGenerator.root = parser.Parse();
            return lootGenerator;
        }








    }
}
