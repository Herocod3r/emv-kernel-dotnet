using System;
using System.Threading.Tasks;

namespace Kernel
{
    public interface ICardChip : IDisposable
    {
        byte[] CardId { get; } //This should contain the ATR info
        Task<byte[]> TransmitCommandAsync(byte[] command);
    }
}
