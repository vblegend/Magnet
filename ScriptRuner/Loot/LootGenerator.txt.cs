using Language.Parser;
using ScriptRuner.Loot.DSL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace ScriptRuner.Loot
{


    public sealed partial class LootGenerator<TValue>
    {
        private class TxtLootTableParser
        {
            private String Directory = "";
            private LootGenerator<String>.LootGroup root = new LootGenerator<String>.LootGroup();
            private LootFileLexer lexer;


            public TxtLootTableParser(String filename)
            {
                this.Directory = Path.GetDirectoryName(filename)!;
                this.lexer = new LootFileLexer(filename, Encoding.UTF8);
                this.root.Probability = 1.0;
            }

            public LootGenerator<String>.LootGroup Parse()
            {
                while (true)
                {
                    if (this.lexer.TestNext(Symbols.NEWLINE)) continue;
                    if (this.lexer.TestNext(Symbol.EOF)) break;
                    var node = ParseElement();

                    if (node is LootGenerator<String>.LootGroup group && group.Probability == 1.0)
                    {
                        this.root.Children.AddRange(group.Children);
                    }
                    else if (node is LootGenerator<String>.LootItem)
                    {
                        this.root.Children.Add(node);
                    }
                }
                return this.root;
            }

            private LootGenerator<String>.ILoot ParseElement()
            {
                var token = this.lexer.LookAtHead();
                if (token == Token.EOF) return null;
                if (token.Symbol == Symbols.SHARP) return this.ParseInclude();
                if (token is NumberToken) return this.ParseItem();
                throw new Exception();
            }


            private LootGenerator<String>.LootGroup ParseGroup()
            {
                this.lexer.NextOfKind(Symbols.LEFTBRACE);
                var group = new LootGenerator<String>.LootGroup();
                while (true)
                {
                    if (this.lexer.TestNext(Symbols.NEWLINE)) continue;
                    if (this.lexer.LookAtHead(Symbols.RIGHTBRACE))
                    {
                        this.lexer.Next();
                        break;
                    }
                    var item = this.ParseElement();
                    if (item is LootGenerator<String>.LootGroup _group && _group.Probability == 1.0)
                    {
                        group.Children.AddRange(_group.Children);
                    }
                    else if (item != null)
                    {
                        group.Children.Add(item);
                    }
                }
                return group;
            }


            private LootGenerator<String>.ILoot ParseItem()
            {
                var strict = false;
                var probability = this.ParseProbability();

                if (this.lexer.LookAtHead(Symbols.FORCE))
                {
                    this.lexer.Next();
                    strict = true;
                }

                if (this.lexer.LookAtHead(Symbols.LEFTBRACE))
                {
                    var group = ParseGroup();
                    group.Strict = strict;
                    group.Probability = probability;
                    return group;
                }
                else
                {
                    List<String> args = new List<String>();
                    var item = this.lexer.Next();
                    while (!this.lexer.LookAtHead(Symbols.NEWLINE))
                    {
                        var k2 = this.lexer.Next();
                        args.Add(k2.Value);
                    }
                    return new LootGenerator<String>.LootItem(new Loot<String>(item.Value, args.ToArray()), probability, strict);
                }
            }

            private Double ParseProbability()
            {
                var num1 = this.lexer.NextOfKind<NumberToken>();
                this.lexer.NextOfKind(Symbols.DIVIDE);
                var num2 = this.lexer.NextOfKind<NumberToken>();
                var v1 = Double.Parse(num1.Value);
                var v2 = Double.Parse(num2.Value);
                return v1 / v2;
            }


            private LootGenerator<String>.ILoot ParseInclude()
            {
                this.lexer.NextOfKind(Symbols.SHARP);
                this.lexer.NextOfKind(Symbols.INCLUDE);
                var file = this.lexer.NextOfKind<IdentifierToken>();
                var parser = new TxtLootTableParser(Path.Combine(Directory, file.Value));
                return parser.Parse();
            }
        }


        /* 
          * 
            1/5         G1          1 2 3 4 5
            1/9         G2          2 3 4 5 6
            1/1         G3          
            1/10 ! {
                1/2         B1
                1/2         B2
                #include gold-coin.loot
            }         
         */
        /// <summary>
        /// 从 loot文件加载爆率模板配置
        /// 格式：
        /// 概率 [!不受掉落率影响]  {     } | (物品 ... 参数)
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static LootGenerator<String> Load(String filename)
        {
            var lootGenerator = new LootGenerator<String>();
            var parser = new TxtLootTableParser(filename);
            lootGenerator.root = parser.Parse();
            return lootGenerator;
        }


    }
}
