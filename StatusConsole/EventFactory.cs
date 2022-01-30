using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole {

    public class ParDef {
        public string Name { get; set; }
        public string Short { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
    }

    public class EventConf {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Short { get; set; }

        public List<ParDef> Pars { get; set; } = new List<ParDef>();
    }
    public class ModConf {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Short { get; set; }

        public List<EventConf> Events { get; set; } = new List<EventConf>();
    }


    public class EnumValConf {
        public int Val { get; set; }
        public string Name { get; set; }
        public string Short { get; set; }
    }


    public class EnumConf {
        public string Name { get; set; }
        public List<EnumValConf> Entries { get; set; } = new List<EnumValConf>();
    }

    public class EventFactory {
      

        private Dictionary<int, ModConf> MyModules = new Dictionary<int, ModConf>();
        private Dictionary<string, EnumConf> MyEnums = new Dictionary<string,EnumConf>();

        public EventFactory(IConfigurationSection debugConfig) {
            IConfigurationSection ms = debugConfig.GetSection("MODULES");
            var mymods = ms.Get<List<ModConf>>();
           
            foreach (var m in mymods) {
                MyModules.Add(m.Id, m);
            }

            IConfigurationSection enums = debugConfig.GetSection("ENUMS");
            var enumConfigs = enums.Get<List<EnumConf>>();

            
            foreach (var e in enumConfigs) {
                MyEnums.Add(e.Name, e);
            
            }

        }

        public string GetAsString(byte moduleId, byte eventId, byte[] rxData, int len) {
            string retVal = "";
            rxData[1] &= 0x3F;  // mask the severity bits for hex strings.
            int dataIdx = 2;
            int bitIdx = 0;

            if (MyModules.ContainsKey(moduleId)) {
                var mod = MyModules[moduleId];
                retVal += mod.Short;

                var evt = mod.Events.FirstOrDefault(e => e.Id == eventId);
                if (evt != null) {
                    retVal += "/" + evt.Short + " ";

                    foreach (var p in evt.Pars) {
                        if (p.Type == "char[]") {
                            retVal += p.Short + ":'" + Encoding.ASCII.GetString(rxData, dataIdx, len - dataIdx) + "' ";
                            dataIdx = len;
                            break;  // This must always be the last Par
                        } else if (p.Type == "float") {
                            float value = BitConverter.ToSingle(rxData.AsSpan(dataIdx, 4));
                            dataIdx += 4;
                            retVal += p.Short + ":" + value.ToString(p.Format) + " ";
                        } else if (p.Type == "double") {
                            double value = BitConverter.ToDouble(rxData.AsSpan(dataIdx, 8));
                            dataIdx += 8;
                            retVal += p.Short + ":" + value.ToString(p.Format) + " ";
                        } else if (p.Type == "uint32") {
                            UInt32 value = BitConverter.ToUInt32(rxData.AsSpan(dataIdx, 4));
                            dataIdx += 4;
                            retVal += p.Short + ":" + value.ToString() + " ";
                        } else if (p.Type == "uint16") {
                            UInt16 value = BitConverter.ToUInt16(rxData.AsSpan(dataIdx, 2));
                            dataIdx += 2;
                            retVal += p.Short + ":" + value.ToString() + " ";
                        } else if (p.Type.StartsWith("char[],")) {
                            int cnt;
                            if (int.TryParse(p.Type.Substring(7), out cnt)) {
                                retVal += p.Short + ":'" + Encoding.ASCII.GetString(rxData, dataIdx, cnt) + "' ";
                                dataIdx += cnt;
                            }
                        } else if (p.Type.StartsWith("byte[],")) {
                            int cnt;
                            if (int.TryParse(p.Type.Substring(7), out cnt)) {
                                retVal += p.Short + ":" + BitConverter.ToString(rxData, dataIdx, cnt) + " ";
                                dataIdx += cnt;
                            }
                        } else if (p.Type.StartsWith("bit:")) {
                            int result = 0x0000;
                            int cnt;
                            if (int.TryParse(p.Type.Substring(4), out cnt)) {
                                if (cnt + bitIdx <= 8) {
                                    int db = rxData[dataIdx];

                                    int mask = 0x0080;
                                    for (int i = 0; i <= 7; i++) {
                                        if ((i >= bitIdx) && (i < bitIdx + cnt)) {
                                            result |= db & mask;
                                        }
                                        mask >>= 1;
                                    }
                                    result >>= (8 - cnt);
                                    bitIdx += cnt;
                                    if (bitIdx >= 8) {
                                        bitIdx = 0;
                                        dataIdx++;
                                    }
                                } else {
                                    // .... wenns über bytegrenzen geht .....
                                }
                                retVal += p.Short + ":" + result.ToString() + " ";
                            }
                        } else if (p.Type.Equals("byte")) {
                            int val = rxData[dataIdx];
                            if (p.Format.StartsWith("{")) {
                                retVal += p.Short + ":" + ConvertToEnumTxt(p.Format.Substring(1, p.Format.IndexOf('}') - 1), val) + " ";
                            } else {
                                retVal += p.Short + ":" + val.ToString() + " ";
                            }
                            dataIdx++;
                        }
                    }

                } else { 
                    retVal += "/" + BitConverter.ToString(rxData, 1, 1) + " ";
                }
                if ((len - dataIdx) > 0) {
                    // Rest of frame is written as Hex Dump.
                    retVal += " " + BitConverter.ToString(rxData, dataIdx, len - dataIdx);
                }

            } else {
                // No definition found. Convert to Hex
                retVal += BitConverter.ToString(rxData, 0, 2) + " " + BitConverter.ToString(rxData, 2, len-2);
            }
            return retVal;
        }

        private string ConvertToEnumTxt(string enumName, int val) {
            String retVal = enumName + "-" + val.ToString();
            if (MyEnums.ContainsKey(enumName)) {
                var ec = MyEnums[enumName];
                
                var entry = ec.Entries.Where(e=>e.Val == val).FirstOrDefault();
                if (entry != null) {
                    retVal = entry.Name;
                }
            }
            return retVal;
        }
    }
}
