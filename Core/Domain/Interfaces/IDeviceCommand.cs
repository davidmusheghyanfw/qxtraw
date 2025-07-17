public interface IDeviceCommand
{
    byte[] InputBuffer { get; }
    byte[] OutputBuffer { get; }
    // void RunOn(IDeviceAdapter device);
}
