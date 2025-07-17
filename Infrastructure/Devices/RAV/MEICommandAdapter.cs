using Quixant.LibRAV;

public class MEICommandAdapter : IDeviceCommand
{
    public MEICommand Raw { get; }

    public MEICommandAdapter(MEIInstruction instruction, int inputLen, int outputLen)
    {
        Raw = new MEICommand(instruction, inputLen, outputLen);
    }

    public byte[] InputBuffer => Raw.InputBuffer;
    public byte[] OutputBuffer => Raw.OutputBuffer;

    public void RunOn(IDeviceAdapter device)
    {

    }

}
