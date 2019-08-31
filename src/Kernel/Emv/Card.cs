using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kernel.Emv.Apdu;

namespace Kernel.Emv
{
    public class Card : IDisposable
    {
        private readonly ICardChip cardChip;

        public Card(ICardChip cardChip)
        {
            this.cardChip = cardChip;
        }


        private async Task<APDUResponse> SendRawApduAsync(APDUCommand command)
        {
            var commandToSend = new List<byte> {command.Class,command.Instruction,command.Parameter1,command.Parameter2 };
            if (command.Data != null && command.Data.Length > 0)
            {
                commandToSend.AddRange(command.Data);
                commandToSend.Add(command.ExpectedLength == 0x00 ? (byte) command.Data.Length : command.ExpectedLength);
            }
            else commandToSend.Add(command.ExpectedLength);

            byte[] response = null;
            try
            {
                response = await this.cardChip.TransmitCommandAsync(commandToSend.ToArray());
            }
            catch (Exception e)
            {
                throw new APDUException("Unable to transmit command",e);
            }
            if(response is null) throw  new APDUException("Unable to get a response",command);

            if (response.Length == 2) return new APDUResponse {SW1 = response[0], SW2 = response[1]};
            return new APDUResponse {Body = response.Take(response.Length-2).ToArray(),SW1 = response[response.Length-2],SW2 = response[response.Length-1]};
        }


        public async Task<APDUResponse> SendApduAsync(APDUCommand command)
        {
            var response = await SendRawApduAsync(command);
            switch (response.SW1)
            {
                case 0x61:
                    return await SendRawApduAsync(new APDUCommand{Instruction = 0XC0,ExpectedLength = response.SW2});
                case 0x6c:
                    command.ExpectedLength = response.SW2;
                    return await SendRawApduAsync(command);
            }

            return response;
        }
        

        public void Dispose()
        {
            cardChip.Dispose();
        }
        
    }
}