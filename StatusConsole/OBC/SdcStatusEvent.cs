namespace StatusConsole.OBC {
    internal class SdcStatusEvent : ObcEvent {

        // Enums from OBC Software
        //typedef enum ado_sdc_cardtype_e {
        enum cardType { 
            C_UNKNOWN = 0,
            C_1XSD,              // V1.xx Card (not accepting CMD8 -> standard capacity
            C_20SD,              // V2.00 Card standard capacity
            C_20HCXC,            // V2.00 Card High Capacity HC or ExtendenCapacity XC
        }
        //ado_sdc_cardtype_t;

        //typedef enum ado_sdc_status_e {
        enum cardStatus {
            ADO_SDC_CARDSTATUS_UNDEFINED = 0,
            ADO_SDC_CARDSTATUS_INIT_RESET,
            ADO_SDC_CARDSTATUS_INIT_RESET2,
            ADO_SDC_CARDSTATUS_INIT_CMD8,
            ADO_SDC_CARDSTATUS_INIT_ACMD41_1,
            ADO_SDC_CARDSTATUS_INIT_ACMD41_2,
            ADO_SDC_CARDSTATUS_INIT_ACMD41_3,
            ADO_SDC_CARDSTATUS_INIT_READ_OCR,
            ADO_SDC_CARDSTATUS_INITIALIZED,
            ADO_SDC_CARDSTATUS_READ_SBCMD,
            ADO_SDC_CARDSTATUS_READ_SBWAITDATA,
            ADO_SDC_CARDSTATUS_READ_SBWAITDATA2,
            ADO_SDC_CARDSTATUS_READ_SBDATA,
            ADO_SDC_CARDSTATUS_WRITE_SBCMD,
            ADO_SDC_CARDSTATUS_WRITE_SBDATA,
            ADO_SDC_CARDSTATUS_WRITE_BUSYWAIT,
            ADO_SDC_CARDSTATUS_WRITE_CHECKSTATUS,
            ADO_SDC_CARDSTATUS_ERROR = 99
        }
        //ado_sdc_status_t;


        public SdcStatusEvent(byte[] data, int len) : base(data, len) {
            if (len == 4) {
                MyText = $"{severity}: ";
                cardType ct = (cardType)(data[2] >> 4);
                int idx = (data[2] & 0x0F);
                cardStatus stat = (cardStatus)(data[3]);
                if (severity == EventSeverity.ERROR) {
                    MyText += $"card[{idx}]-{ct}: PreError Status: {stat}";
                } else {
                    MyText += $"card[{idx}]-{ct}: Status: {stat}";
                }
                
            } else {
                MyText = $"{severity}: SdcStatusEvent - Data Error - wrong len: " + System.BitConverter.ToString(data, 2, len - 2);
            }
        }
    }
}