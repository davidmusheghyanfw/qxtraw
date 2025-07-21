using Quixant.LibRAV;

public class MEIExtendedCommandAdapter : IDeviceExtendedCommand
{
    public MEIExtendedCommand Raw { get; }

    public MEIExtendedCommandAdapter(MEIMessageExtendedSubtype subtype, int inputLen)
    {
        Raw = new MEIExtendedCommand(subtype, inputLen);
    }

    public byte[] InputBuffer => Raw.InputBuffer;
    public byte[] OutputBuffer => Raw.OutputBuffer;

    // public void RunOn(IDeviceAdapter device)
    // {
    //     device.Run(this);
    // }

}
