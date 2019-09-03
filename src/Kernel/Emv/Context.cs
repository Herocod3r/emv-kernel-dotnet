using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kernel.Tlv;

namespace Kernel.Emv
{
    public class Context
    {
        private Card card;
        private ContextConfig config;
        private List<byte> sdaData;
        private List<byte> dataAuthenticationCode;
        private ulong tvr;
        private ulong cvr;

        public Context(Card card,ContextConfig config)
        {
            this.card = card;
            this.config = config;
            this.sdaData = new List<byte>();
            dataAuthenticationCode = new List<byte>();
        }

        public ProcessingOptions ProcessingOptions { get; set; } = new ProcessingOptions();
        public CardInformation CardInformation { get; set; } = new CardInformation();
        public Application Application { get; set; } = new Application();

        public async Task<List<ApplicationInformation>> ListApplicationsAsync(bool contactless,ApplicationHint[] hints)
        {
            var pseFile = contactless ? Encoding.ASCII.GetBytes("2PAY.SYS.DDF01") : Encoding.ASCII.GetBytes("1PAY.SYS.DDF01");
            
            var pse = await this.card.SelectApplicationAsync(pseFile, true);

            if (pse is null) return await ListNonPseApplications(hints);
            else return await ListPseApplications(pse);

        }

        public async void SelectApplicationAsync(byte[] applicationName)
        {
            var pdol = new Tlv.Tlv();
            var app = await card.SelectApplicationAsync(applicationName,true);
            if(app is null) throw new ArgumentException("Application not found");
            if (app.Template.ProcessingObjects != null)
            {
                pdol = BuildDol(app.Template.ProcessingObjects, null);
            }
            else
            {
                pdol[0x83] = new byte[] { };
            }

            var options = await this.card.GetProcessingAsync(pdol);
            this.Application = app;
            ProcessingOptions = options;
            ProcessCardInformation();
            
        }


        private async void ProcessCardInformation()
        {
            var groupTlv = new Tlv.Tlv();
            foreach (var app in ProcessingOptions.ApplicationFileList)
            {
                var sdaCount = app.SdaCount;
                
                for (int i = app.Start; i <= app.End; i++)
                {
                    var record = await card.ReadAsync(app.Sfi,i);
                    var body = new Tlv.Tlv(record.Body);
                    if(!body.ContainsKey(0x70)) throw new InvalidOperationException("malformed application file");
                    if (sdaCount > 0)
                    {
                        if(app.Sfi <= 10) sdaData.AddRange(body[0x70]);
                        else sdaData.AddRange(record.Body);
                        sdaCount--;
                    }

                    var template = new Tlv.Tlv(body[0x70]);
                    groupTlv.CopyFrom(template);
                }

            }
            CardInformation = TlvSerializer.Deserialize<CardInformation>(groupTlv);
        }

        private async Task<List<ApplicationInformation>> ListPseApplications(Application application)
        {
            var result = new List<ApplicationInformation>();
            var record = 1;
            while (true)
            {
                var res = await card.ReadAsync(application.Template.Sfi, record);
                if(res.Body is null || res.Body.Length < 1)break;;
                var tlv = new Tlv.Tlv(res.Body);
                if(!tlv.ContainsKey(0x70)) throw new InvalidOperationException("Invalid Pse Record");
                tlv = new Tlv.Tlv(tlv[0x70]);
                if(!tlv.ContainsKey(0x61)) throw new InvalidOperationException("Invalid Pse Record");
                var info = TlvSerializer.Deserialize<ApplicationInformation>(tlv,0x61);
                result.Add(info);
                record++;
            }

            return result;
        }

        private async Task<List<ApplicationInformation>> ListNonPseApplications(ApplicationHint[] hints)
        {
            var result = new List<ApplicationInformation>();
            foreach (var hint in hints)
            {
                var first = true;
                while (true)
                {
                    var app = await this.card.SelectApplicationAsync(hint.Name, first);
                    if(app is null) break;
                    var duplicated = false;
                    foreach (var info in result)
                    {
                        if (info.Name == app.DedicatedFileName)
                        {
                            duplicated = true;
                            break;
                        }
                    }
                    if(!duplicated) result.Add(new ApplicationInformation{Name = app.DedicatedFileName,Label = app.Template.Label,Priority = app.Template.Priority});
                    if(!hint.Partial) break;
                    first = false;
                }
            }

            return result;
        }

        private Tlv.Tlv BuildDol(DataObjectList dol,Transaction trx)
        {
            var tlv = new Tlv.Tlv();
            foreach (var item in dol)
            {
                switch (item.Key)
                {
                    case 0x9F02:
                        tlv[item.Key] = BerHelpers.EncodeUint((ulong)trx.Amount);
                        break;
                    case 0x9F03:
                        tlv[item.Key] = BerHelpers.EncodeUint((ulong)trx.AdditionalAmount);
                        break;
                    case 0x9F1A:
                        tlv[item.Key] = BerHelpers.EncodeUint((ulong)this.config.Terminal.CurrencyCode);
                        break;
                    case 0x95:
                        tlv[item.Key] = BerHelpers.EncodeUint(this.tvr);
                        break;
                    case 0x5F2A:
                        tlv[item.Key] = BerHelpers.EncodeUint((ulong)this.config.Terminal.CurrencyCode);
                        break; 
                    case 0x9A:
                        tlv[item.Key] = BerHelpers.EncodeUint((ulong)trx.Date.Ticks);
                        break; 
                    case 0x9C:
                        tlv[item.Key] = BerHelpers.EncodeUint((ulong)trx.Type);
                        break;
                    case 0x9F37:
                        tlv[item.Key] = GenerateRandomNumber(item.Value);
                        break;
                    case 0x9F35:
                        tlv[item.Key] = BerHelpers.EncodeUint((ulong)this.config.Terminal.Type);
                        break;
                    case 0x9F45:
                        tlv[item.Key] = dataAuthenticationCode.ToArray();
                        break;
                    case 0x9F34:
                        tlv[item.Key] = BerHelpers.EncodeUint(cvr);
                        break;
                    default:
                        if (CardInformation.Raw.TryGetValue(item.Key,out byte[] value) || this.ProcessingOptions.Raw.TryGetValue(item.Key,out value))
                        {
                            tlv[item.Key] = value;
                        }
                        break;
                }
            }

            return tlv;
        }

        private byte[] GenerateRandomNumber(int size)
        {
            var buffer = new byte[size];
            new Random().NextBytes(buffer);
            return buffer;
        }

    }
}