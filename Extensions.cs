using System;
using System.Diagnostics;
using Quixant.LibRAV;

public static class Extensions
{
    public static void ExecuteWithMenuOption(this RAVDevice device, MenuOption option)
    {
        Stopwatch sw;
        RAVCommand cmd;

        cmd = new MEICommand(ToMEIInstruction(option), 0, 0);

        try
        {
            sw = Stopwatch.StartNew();
            device.Execute(cmd);
            sw.Stop();
        }
        catch (Exception exc)
        {
            Console.WriteLine("Error executing command: " + exc.Message);
            return;
        }

        Console.WriteLine("Command executed");
        printTime(sw.ElapsedTicks, 1);
    }

    private static MEIInstruction ToMEIInstruction(MenuOption option)
    {
        byte val = (byte)((int)option - 4000);
        return (MEIInstruction)val;
    }

    private static void printTime(long ticks, int repetitions)
    {
        double seconds = ((double)ticks) / ((double)Stopwatch.Frequency);
        double avg = seconds / repetitions;

        Console.Write("\n----------------  Results  ----------------\n\n");
        Console.Write("Total execution time: ");

        if (seconds > 1)
            Console.WriteLine("{0:f4}s", seconds);
        else if (seconds * 1000 > 1)
            Console.WriteLine("{0:f4}ms", seconds * 1000);
        else
            Console.WriteLine("{0:f4}us", seconds * 1000 * 1000);

        Console.Write("Average cycle execution time: ");

        if (avg > 1)
            Console.WriteLine("{0:f4}s", avg);
        else if (avg * 1000 > 1)
            Console.WriteLine("{0:f4}ms", avg * 1000);
        else
            Console.WriteLine("{0:f4}us", avg * 1000 * 1000);

        Console.Write("\n----------------  End of results  ----------------\n");
    }
}
