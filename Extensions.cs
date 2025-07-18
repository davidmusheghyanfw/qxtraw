using System;
using System.Diagnostics;
using Quixant.LibRAV;

public static class Extensions
{
    public static void ExecuteWithMenuOption(this RAVDevice device, MenuOption option)
    {
        Stopwatch sw;
        RAVCommand cmd = null;

        if (device.Protocol == ProtocolIdentifier.JCM)
        {
            cmd = new JCMCommand(ToJCMInstruction(option), 0, 0);
        }
        else if (device.Protocol == ProtocolIdentifier.MEI)
        {
            cmd = new MEICommand(ToMEIInstruction(option), 0, 0);
        }

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

    private static JCMInstruction ToJCMInstruction(MenuOption option)
    {
        switch (option)
        {
            case MenuOption.StatusRequest:
                return JCMInstruction.GetStatus;
            case MenuOption.EnableDisableRequest:
                return JCMInstruction.GetEnableDisable;
            case MenuOption.SecurityRequest:
                return JCMInstruction.GetSecurity;
            case MenuOption.CommunicationModeRequest:
                return JCMInstruction.GetCommunicationMode;
            case MenuOption.InhibitRequest:
                return JCMInstruction.GetInhibit;
            case MenuOption.DirectionRequest:
                return JCMInstruction.GetDirection;
            case MenuOption.OptionalFunctionRequest:
                return JCMInstruction.GetOptionalFunction;
            case MenuOption.BarcodeFunctionRequest:
                return JCMInstruction.GetBarcodeFunction;
            case MenuOption.BarInhibitRequest:
                return JCMInstruction.GetBarInhibit;
            case MenuOption.BootVersionRequest:
                return JCMInstruction.GetBootVersion;
            case MenuOption.VersionRequest:
                return JCMInstruction.GetVersion;
            case MenuOption.CurrencyAssignmentRequest:
                return JCMInstruction.GetCurrencyAssignment;
            case MenuOption.SerialNumber:
                return JCMInstruction.GetSerialNumber;
            case MenuOption.EnableDisable:
                return JCMInstruction.SetEnableDisable;
            case MenuOption.Security:
                return JCMInstruction.SetSecurity;
            case MenuOption.OptionalFunction:
                return JCMInstruction.SetOptionalFunction;
            case MenuOption.BarcodeFunction:
                return JCMInstruction.SetBarcodeFunction;
            case MenuOption.CommunicationMode:
                return JCMInstruction.SetCommunicationMode;
            case MenuOption.Inhibit:
                return JCMInstruction.SetInhibit;
            case MenuOption.Direction:
                return JCMInstruction.SetDirection;
            case MenuOption.BarInhibit:
                return JCMInstruction.SetBarInhibit;
            case MenuOption.Reset:
                return JCMInstruction.Reset;
            case MenuOption.Stack1:
                return JCMInstruction.Stack1;
            case MenuOption.Stack2:
                return JCMInstruction.Stack2;
            case MenuOption.Return:
                return JCMInstruction.Return;
            case MenuOption.Hold:
                return JCMInstruction.Hold;
            case MenuOption.Wait:
                return JCMInstruction.Wait;
            case MenuOption.ProgramSignature:
                return JCMInstruction.ProgramSignature;
            case MenuOption.Acknowledge:
                return JCMInstruction.Ack;
            default:
                throw new NotImplementedException();
        }
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
