using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Motwane_UVSS
{
    public class HCNetSDK
    {
        [DllImport(@"C:\Users\ADMIN\Downloads\EN-HCNetSDKV6.1.9.4_build20220412_win64\EN-HCNetSDKV6.1.9.4_build20220412_win64\EN-HCNetSDKV6.1.9.4_build20220412_win64\lib\HCNetSDK.dll")]
        public static extern bool NET_DVR_Init();

        [DllImport(@"C:\Users\ADMIN\Downloads\EN-HCNetSDKV6.1.9.4_build20220412_win64\EN-HCNetSDKV6.1.9.4_build20220412_win64\EN-HCNetSDKV6.1.9.4_build20220412_win64\lib\HCNetSDK.dll")]
        public static extern void NET_DVR_Cleanup();

        [DllImport(@"C:\Users\ADMIN\Downloads\EN-HCNetSDKV6.1.9.4_build20220412_win64\EN-HCNetSDKV6.1.9.4_build20220412_win64\EN-HCNetSDKV6.1.9.4_build20220412_win64\lib\HCNetSDK.dll")]
        public static extern int NET_DVR_Login_V30(string sDVRIP, int wDVRPort, string sUserName, string sPassword, ref NET_DVR_DEVICEINFO_V30 lpDeviceInfo);

        [DllImport(@"C:\Users\ADMIN\Downloads\EN-HCNetSDKV6.1.9.4_build20220412_win64\EN-HCNetSDKV6.1.9.4_build20220412_win64\EN-HCNetSDKV6.1.9.4_build20220412_win64\lib\HCNetSDK.dll")]
        public static extern bool NET_DVR_Logout(int lUserID);

        [DllImport(@"C:\Users\ADMIN\Downloads\EN-HCNetSDKV6.1.9.4_build20220412_win64\EN-HCNetSDKV6.1.9.4_build20220412_win64\EN-HCNetSDKV6.1.9.4_build20220412_win64\lib\HCNetSDK.dll")]
        public static extern bool NET_DVR_CaptureJPEGPicture(int lUserID, int lChannel, ref NET_DVR_JPEGPARA lpJpegPara, string sPicFileName);

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DVR_DEVICEINFO_V30
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] sSerialNumber;
            public byte byAlarmInPortNum;
            public byte byAlarmOutPortNum;
            public byte byDiskNum;
            public byte byDVRType;
            public byte byChanNum;
            public byte byStartChan;
            public byte byAudioChanNum;
            public byte byIPChanNum;
            public byte byZeroChanNum;
            public byte byMainProto;
            public byte bySubProto;
            public byte bySupport;
            public byte bySupport1;
            public byte bySupport2;
            public ushort wDevType;
            public byte bySupport3;
            public byte byMultiStreamProto;
            public byte byStartDChan;
            public byte byStartDTalkChan;
            public byte byHighDChanNum;
            public byte bySupport4;
            public byte byLanguageType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] byRes2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DVR_JPEGPARA
        {
            public ushort wPicSize;
            public ushort wPicQuality;
        }
    }
}
