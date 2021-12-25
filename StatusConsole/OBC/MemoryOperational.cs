﻿using System;

namespace StatusConsole.OBC {
    public class MemoryOperationalEvent : ObcEvent {
        public MemoryOperationalEvent(byte[] data, int len) : base(data, len) {
            if (len == 20) {
                string sdcOp = BitConverter.ToString(data, 2, 1);
                string mramOp = BitConverter.ToString(data, 3, 1);

                string instanceName = System.Text.Encoding.ASCII.GetString(data, 3, 16);
                MyText = $"Sdc: {sdcOp} Mram: {mramOp} Obc: '{instanceName}'";
            } else {
                MyText = "Data Error - wrong len";
            }
        }
    }
}