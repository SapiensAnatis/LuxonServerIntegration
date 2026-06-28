namespace Luxon.Server.Host.Helpers;

internal static class IpcValueTypes
{
    public const int Null = 0;
    public const int Boolean = 1;
    public const int UInt8 = 2;
    public const int Int16 = 3;
    public const int Int32 = 4;
    public const int Int64 = 5;
    public const int Float32 = 6;
    public const int Float64 = 7;
    public const int String = 8;
    public const int ByteArray = 9;
    public const int BoolArray = 10;
    public const int Int16Array = 11;
    public const int Int32Array = 12;
    public const int Int64Array = 13;
    public const int Float32Array = 14;
    public const int Float64Array = 15;
    public const int StringArray = 16;
    public const int ObjectArray = 17;
    public const int JaggedArray = 18;
    public const int Dictionary = 19;
    public const int GenericDict = 20;
    public const int Hashtable = 21;
    public const int RawCustomVal = 22;
    public const int EventMsg = 23;
    public const int OpReqMsg = 24;
    public const int OpRespMsg = 25;
    public const int DictArray = 26;
    public const int GenDictArray = 27;
    public const int HashTblArray = 28;
    public const int RawCustArray = 29;
    public const int PreSerialized = 30;
}
