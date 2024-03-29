using System;

namespace PKHeX.EncounterSlotDumper;

public static class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");
        foreach (var arg in args)
            Console.WriteLine(arg);

        // Keep track of how much time the entire operation takes.
        var start = DateTime.Now;

        Dumper2h.Dump();

        Console.WriteLine();
        Console.WriteLine("Dumping Gen1 tables...");
        Dumper1.DumpGen1();

        Console.WriteLine();
        Console.WriteLine("Dumping Gen2 tables...");
        Dumper2.DumpGen2();

        Console.WriteLine();
        Console.WriteLine("Dumping Gen3 tables...");
        Dumper3.DumpGen3();

        Console.WriteLine();
        Console.WriteLine("Dumping Gen4 tables...");
        Dumper4.DumpGen4();
        Dumper4Walker.DumpGen4();

        Console.WriteLine();
        Console.WriteLine("Dumping Gen5 tables...");
        Dumper5.DumpGen5();

        Console.WriteLine();
        Console.WriteLine("Dumping Gen6 tables...");
        Dumper6.DumpGen6();

        Console.WriteLine();
        Console.WriteLine("Dumping Gen7 tables...");
        Dumper7.DumpGen7();

        Console.WriteLine();
        Console.WriteLine("Dumping Gen7b tables...");
        Dumper7b.DumpGen7b();

        Console.WriteLine();
        Console.WriteLine("Dumping Gen8 tables...");
        Dumper8.DumpGen8();

        Console.WriteLine();
        Console.WriteLine("Done!");

        // Print out how long the operation took.
        Console.WriteLine("Time taken: " + (DateTime.Now - start));
    }
}
