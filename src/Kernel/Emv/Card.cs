using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kernel.Emv.Apdu;
using Kernel.Tlv;
using Kernel.Utils;

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
                commandToSend.Add( (byte) command.Data.Length);
                commandToSend.AddRange(command.Data);
            }
            
            commandToSend.Add(command.ExpectedLength);

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

        public Task<APDUResponse> SelectAsync(byte[] nameAid,bool first)
        {
            byte p2 = first ? (byte)0x00 : (byte)2;
            return this.SendApduAsync(new APDUCommand{Instruction = 0xA4,Parameter1 = 0x04,Parameter2 = p2,Data = nameAid});
        }


        public Task<APDUResponse> ReadAsync(int sfi, int record) => SendApduAsync(new APDUCommand{Instruction = 0xB2,Parameter1 = (byte)record,Parameter2 = (byte)((sfi<<3)|0x4)});

        public async Task<Application> SelectApplicationAsync(byte[] name,bool first)
        {
            var rsp = await SelectAsync(name, first);
            if (rsp.SW1 == 0x6A && rsp.SW2 == 0x82) return null;
            if (rsp.SW1 != 0x90 && rsp.SW2 != 0x00) return null;

            var tlv = new Tlv.Tlv(rsp.Body);
            try
            {
               return TlvSerializer.Deserialize<Application>(tlv,0x6f);

            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error decoding Tlv");
            }

        }

        public async Task<ProcessingOptions> GetProcessingAsync(Tlv.Tlv pdol)
        {
            var pdolData = pdol.Encode();
            var res = await SendApduAsync(new APDUCommand
            {
                Class = 0x80,Instruction = 0xA8,Data = pdolData
            });
            var body = new Tlv.Tlv(res.Body);
            if (body.ContainsKey(0x77))return TlvSerializer.Deserialize<ProcessingOptions>(body, 0x77);
            
            if(!body.TryGetValue(0x80,out byte[] raw)) throw new InvalidOperationException("Invalid message");

            var aip = BerHelpers.DecodeUint(Helpers.ArraySlice(raw,0, 2));
            var po = new ProcessingOptions {ApplicationInterchangeProfile = (int) aip};
            po.ApplicationFileList = new ApplicationFileList();;
            po.ApplicationFileList.Decode(Helpers.ArraySlice(raw,2, raw.Length));

            return po;

        }


        public async Task<bool> VerifyPinAsync(string pin)
        {
            var pinBlock = new byte[8];
            if(pin.Length < 4 || pin.Length > 12) throw new ArgumentException("Pin length is incorrect");
            pinBlock[0] = (byte)((1 << 5) | pin.Length);
            for (int i = 0; i < 12; i++)
            {
                var digit = (byte) 0;
                if (i < pin.Length) digit = (byte) (pin[i] - '0');
                else digit = 0xF;
                var offset = i / 2;
                var nibble = 1 - (i % 2);
                var shift = nibble * 4;
                pinBlock[1 + offset] |= (byte)(digit << shift);
            }

            pinBlock[7] = 0xFF;
            var rsp = await SendApduAsync(new APDUCommand
            {
                Instruction = 0x20,Parameter2 = 1<<7,Data = pinBlock
            });
            return rsp.SW1 == 0x90 && rsp.SW2 == 0x00;
        }

        public async Task<GeneratedAC> GenerateAcAsync(int kind,Tlv.Tlv dol)
        {
            var rsp = await SendApduAsync(new APDUCommand
            {
                Class = 0x80,Instruction = 0xAE,Parameter1 = (byte)kind,Data = dol.Encode()
            });
            
            if(rsp.SW1 != 0x90 && rsp.SW2 != 0x00) throw new InvalidOperationException("An error occurred processing transaction");
            var body = new Tlv.Tlv(rsp.Body);
            if(!body.ContainsKey(0x77)) throw new InvalidOperationException("An error occurred processing transaction");

            return TlvSerializer.Deserialize<GeneratedAC>(body,0x77);
            
        }
        
        public void Dispose()
        {
            cardChip.Dispose();
        }
        
    }


  
}