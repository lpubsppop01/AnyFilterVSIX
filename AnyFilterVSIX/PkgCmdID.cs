// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace lpubsppop01.AnyTextFilterVSIX
{
    static class PkgCmdIDList
    {
        public const uint cmdidSettings = 0x100;
        public const uint cmdidRunFilter01 = 0x110;
        public const uint cmdidRunFilter02 = 0x120;
        public const uint cmdidRunFilter03 = 0x130;
        public const uint cmdidRunFilter04 = 0x140;
        public const uint cmdidRunFilter05 = 0x150;
        public const uint cmdidRunFilter06 = 0x160;
        public const uint cmdidRunFilter07 = 0x170;
        public const uint cmdidRunFilter08 = 0x180;
        public const uint cmdidRunFilter09 = 0x190;
        public const uint cmdidRunFilter10 = 0x200;
        public const uint cmdidRunFilter11 = 0x210;
        public const uint cmdidRunFilter12 = 0x220;
        public const uint cmdidRunFilter13 = 0x230;
        public const uint cmdidRunFilter14 = 0x240;
        public const uint cmdidRunFilter15 = 0x250;
        public const uint cmdidRunFilter16 = 0x260;
        public const uint cmdidRunFilter17 = 0x270;
        public const uint cmdidRunFilter18 = 0x280;
        public const uint cmdidRunFilter19 = 0x290;
        public const uint cmdidRunFilter20 = 0x300;

        public const int FilterCountMax = 20;

        public static uint GetCmdidRunFilter(int iFilter)
        {
            if (iFilter == 0) return cmdidRunFilter01;
            else if (iFilter == 1) return cmdidRunFilter02;
            else if (iFilter == 2) return cmdidRunFilter03;
            else if (iFilter == 3) return cmdidRunFilter04;
            else if (iFilter == 4) return cmdidRunFilter05;
            else if (iFilter == 5) return cmdidRunFilter06;
            else if (iFilter == 6) return cmdidRunFilter07;
            else if (iFilter == 7) return cmdidRunFilter08;
            else if (iFilter == 8) return cmdidRunFilter09;
            else if (iFilter == 9) return cmdidRunFilter10;
            else if (iFilter == 10) return cmdidRunFilter11;
            else if (iFilter == 11) return cmdidRunFilter12;
            else if (iFilter == 12) return cmdidRunFilter13;
            else if (iFilter == 13) return cmdidRunFilter14;
            else if (iFilter == 14) return cmdidRunFilter15;
            else if (iFilter == 15) return cmdidRunFilter16;
            else if (iFilter == 16) return cmdidRunFilter17;
            else if (iFilter == 17) return cmdidRunFilter18;
            else if (iFilter == 18) return cmdidRunFilter19;
            else if (iFilter == 19) return cmdidRunFilter20;
            return 0;
        }
    };
}