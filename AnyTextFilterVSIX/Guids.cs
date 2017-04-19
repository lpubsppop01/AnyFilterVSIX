// Guids.cs
// MUST match guids.h
using System;

namespace lpubsppop01.AnyTextFilterVSIX
{
    static class GuidList
    {
        public const string guidAnyTextFilterPkgString = "79bb93d0-029a-465a-a38c-f19398220f8a";
        public const string guidAnyTextFilterCmdSetString = "b9c0649d-bc1d-410e-8ddc-3b50a500af50";
        public static readonly Guid guidAnyTextFilterCmdSet = new Guid(guidAnyTextFilterCmdSetString);
    };
}