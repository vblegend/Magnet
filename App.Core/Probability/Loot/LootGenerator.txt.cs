using App.Core.Probability.Loot.DSL;
using Language.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static System.Formats.Asn1.AsnWriter;

namespace App.Core.Probability
{


    public sealed partial class LootGenerator<TValue>
    {



        private class TxtLootTableParser
        {
            private Int32 currentIndex = 0;
            private String[] lines;
            private String Directory = "";
            private LootGenerator<String>.LootGroup root = new LootGenerator<String>.LootGroup();
            private LootFileLexer lexer;


            public TxtLootTableParser(String filename)
            {
                this.lines = File.ReadAllLines(filename);
                this.Directory = Path.GetDirectoryName(filename);
                LootFileLexer lexer = new LootFileLexer(filename, Encoding.UTF8);
                Console.WriteLine(lexer);
            }


            public LootGenerator<String>.LootGroup Parse()
            {
                while (true)
                {
                    if (this.lexer.TestNext(Symbols.KW_EOF)) break;
                    var node = ParseStatement();
                    if (node != null) this.root.Children.Add(node);
                }
                return this.root;
            }




            private LootGenerator<String>.ILoot ParseStatement()
            {
                var token = this.lexer.LookAtHead();
                if (token == null) throw new ParseException(this.lexer.FullPath, token, "Invalid keywords appear in ");
                if (token.Symbol == Symbols.KW_SHARP) return this.ParseInclude();
                if (token.Symbol == Symbols.KW_IMPORT) return this.ParseImport();
                if (token.Symbol == Symbols.KW_EXPORT) return this.ParseExportStatement(currentScope);
                if (token.Symbol == Symbols.KW_FOR) return this.ParseForBlock(currentScope);
                if (token.Symbol == Symbols.KW_WHILE) return this.ParseWhileBlock(currentScope);
                if (token.Symbol == Symbols.KW_IF) return this.ParseIfBlock(currentScope);
                var exp = this.ParseExpression(currentScope);
                return new ExpressionStatement(exp);
            }

            private LootGenerator<String>.ILoot ParseInclude()
            {
                return null;
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

    


            private void ParseGroup()
            {

            }


            private void ParseItem()
            {

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
