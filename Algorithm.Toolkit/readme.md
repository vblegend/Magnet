``` csharp


        var lottery = Lottery<String>.Load("lotterys/unlimited.txt");
        var lootGenerator = LootGenerator<String>.Load("loots/default.loot");
        TestSccriptUnload();


        using (new WatchTimer("Loot Generate 100000"))
        {
            for (int i = 0; i < 100000; i++)
            {
                lootGenerator.Generate();
            }
        }


        using (new WatchTimer("Loot Generate 1"))
        {
            var loots = lootGenerator.Generate(1);
            if (loots.Count() > 0)
            {
                foreach (var loot in loots)
                {
                    Console.WriteLine($"Drop Item: {loot}");
                }
            }
            else
            {
                Console.WriteLine("Not Dorp Items¡£");
            }
        }

        using (new WatchTimer("Draw SS With"))
        {
            var count = 0;
            while (true)
            {
                count++;
                var drops = lootGenerator.Generate(1.0);
                var Count = drops.Where(e => e.Value == "SSS").Count();

                if (Count > 0) break;
            }
            Console.WriteLine(count);
        }

        using (new WatchTimer("Draw Minimum Guarantee 75"))
        {
            for (int i = 0; i < 100; i++)
            {
                var drawItem = lottery.Draw();
                if (drawItem == null) break;
                Console.Write($"Draw Item ");
                if (drawItem[0] == 'S') Console.BackgroundColor = ConsoleColor.Red;
                Console.Write(drawItem);
                if (drawItem[0] == 'S') Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine($" With {i + 1} Count.");
            }
        }
        using (new WatchTimer("Draw SSS With"))
        {
            var count = 0;
            while (true)
            {
                count++;
                var lottery2 = lottery.Clone();
                var drawItem = lottery2.Draw();
                if (drawItem == "SSS") break;
            }
            Console.WriteLine(count);
        }


```
