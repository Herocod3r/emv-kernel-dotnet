namespace Kernel.Tlv
{
    public interface ITlvDecoder
    {
        void Decode(byte[] bytes);
    }
}