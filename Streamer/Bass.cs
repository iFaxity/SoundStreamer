using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Streamer {
    /* Streamer Bass-Sharp
    [Flags]
    public enum InitFlag
    {
        Default = 0,
        _8bits = 1,
        Mono = 2,
        _3D = 4,
        Latency = 256,
        CpSpeaker = 1024,
        Speakers = 2048,
        NnSpeaker = 4096,
        Dmix = 8192,
        Freq = 16384,
    }
    [Flags]
    public enum AttributeFlag
    {
        FREQ = 1,
        VOL = 2,
        PAN = 3,
        EAXMIX = 4,
        NOBUFFER = 5,
        CPU = 7,
        SRC = 8,
        NET_RESUME = 9,
        SCANINFO = 10,
        MUSIC_AMPLIFY = 256,
        MUSIC_PANSEP = 257,
        MUSIC_PSCALER = 258,
        MUSIC_BPM = 259,
        MUSIC_SPEED = 260,
        MUSIC_VOL_GLOBAL = 261,
        MUSIC_ACTIVE = 262,
        MUSIC_VOL_CHAN = 512,
        MUSIC_VOL_INST = 768,
        TEMPO = 65536,
        TEMPO_PITCH = 65537,
        TEMPO_FREQ = 65538,
        TEMPO_OPTION_USE_AA_FILTER = 65552,
        TEMPO_OPTION_AA_FILTER_LENGTH = 65553,
        TEMPO_OPTION_USE_QUICKALGO = 65554,
        TEMPO_OPTION_SEQUENCE_MS = 65555,
        TEMPO_OPTION_SEEKWINDOW_MS = 65556,
        TEMPO_OPTION_OVERLAP_MS = 65557,
        TEMPO_OPTION_PREVENT_CLICK = 65558,
        REVERSE_DIR = 69632,
        MIDI_PPQN = 73728,
        MIDI_CPU = 73729,
        MIDI_CHANS = 73730,
        MIDI_VOICES = 73731,
        MIDI_VOICES_ACTIVE = 73732,
        MIDI_TRACK_VOL = 73984,
        OPUS_ORIGFREQ = 77824,
        DSD_GAIN = 81920,
        DSD_RATE = 81921,
    }
    [Flags]
    public enum SyncFlag
    {
        BASS_SYNC_POS = 0,
        BASS_SYNC_MUSICINST = 1,
        BASS_SYNC_END = 2,
        BASS_SYNC_MUSICFX = BASS_SYNC_END | BASS_SYNC_MUSICINST,
        BASS_SYNC_META = 4,
        BASS_SYNC_SLIDE = BASS_SYNC_META | BASS_SYNC_MUSICINST,
        BASS_SYNC_STALL = BASS_SYNC_META | BASS_SYNC_END,
        BASS_SYNC_DOWNLOAD = BASS_SYNC_STALL | BASS_SYNC_MUSICINST,
        BASS_SYNC_FREE = 8,
        BASS_SYNC_MUSICPOS = BASS_SYNC_FREE | BASS_SYNC_END,
        BASS_SYNC_SETPOS = BASS_SYNC_MUSICPOS | BASS_SYNC_MUSICINST,
        BASS_SYNC_OGG_CHANGE = BASS_SYNC_FREE | BASS_SYNC_META,
        BASS_SYNC_STOP = BASS_SYNC_OGG_CHANGE | BASS_SYNC_END,
        BASS_SYNC_MIXTIME = 1073741824,
        BASS_SYNC_ONETIME = -2147483648,
        BASS_SYNC_MIXER_ENVELOPE = 66048,
        BASS_SYNC_MIXER_ENVELOPE_NODE = BASS_SYNC_MIXER_ENVELOPE | BASS_SYNC_MUSICINST,
        BASS_SYNC_WMA_CHANGE = 65792,
        BASS_SYNC_WMA_META = BASS_SYNC_WMA_CHANGE | BASS_SYNC_MUSICINST,
        BASS_SYNC_CD_ERROR = 1000,
        BASS_SYNC_CD_SPEED = BASS_SYNC_CD_ERROR | BASS_SYNC_END,
        BASS_WINAMP_SYNC_BITRATE = 100,
        BASS_SYNC_MIDI_MARKER = 65536,
        BASS_SYNC_MIDI_CUE = BASS_SYNC_MIDI_MARKER | BASS_SYNC_MUSICINST,
        BASS_SYNC_MIDI_LYRIC = BASS_SYNC_MIDI_MARKER | BASS_SYNC_END,
        BASS_SYNC_MIDI_TEXT = BASS_SYNC_MIDI_LYRIC | BASS_SYNC_MUSICINST,
        BASS_SYNC_MIDI_EVENT = BASS_SYNC_MIDI_MARKER | BASS_SYNC_META,
        BASS_SYNC_MIDI_TICK = BASS_SYNC_MIDI_EVENT | BASS_SYNC_MUSICINST,
        BASS_SYNC_MIDI_TIMESIG = BASS_SYNC_MIDI_EVENT | BASS_SYNC_END,
        BASS_SYNC_MIDI_KEYSIG = BASS_SYNC_MIDI_TIMESIG | BASS_SYNC_MUSICINST,
    }
    [Flags]
    public enum StreamFlag
    {
        Default = 0,
        Float = 256,
        Mono = 2,
        Software = 16,
        _3D = 8,
        Loop = 4,
        Fx = 128,
        Prescan = 131072,
        AutoFree = 262144,
        Decode = 2097152,
        AsyncFile = 1073741824,
        Unicode = -2147483648,

        //Speakers
        Speaker_Front = 16777216,
        Speaker_Rear = 33554432,
        Speaker_CenterLFE = Speaker_Rear | Speaker_Front,
        Speaker_Rear2 = 67108864,
        Speaker_Left = 268435456,
        Speaker_Right = 536870912,
        Speaker_FrontLeft = Speaker_Left | Speaker_Front,
        Speaker_FrontRight = Speaker_Right | Speaker_Front,
        Speaker_RearLeft = Speaker_Left | Speaker_Rear,
        Speaker_RearRight = Speaker_Right | Speaker_Rear,
        Speaker_Center = Speaker_RearLeft | Speaker_Front,
        Speaker_LFE = Speaker_RearRight | Speaker_Front,
        Speaker_Rear2Left = Speaker_Left | Speaker_Rear2,
        Speaker_Rear2Right = Speaker_Right | Speaker_Rear2,
        Speaker_Pair1 = Speaker_Front,
        Speaker_Pair2 = Speaker_Rear,
        Speaker_Pair3 = Speaker_Pair2 | Speaker_Pair1,
        Speaker_Pair4 = Speaker_Rear2,
        Speaker_Pair5 = Speaker_Pair4 | Speaker_Pair1,
        Speaker_Pair6 = Speaker_Pair4 | Speaker_Pair2,
        Speaker_Pair7 = Speaker_Pair6 | Speaker_Pair1,
        Speaker_Pair8 = 134217728,
        Speaker_Pair9 = Speaker_Pair8 | Speaker_Pair1,
        Speaker_Pair10 = Speaker_Pair8 | Speaker_Pair2,
        Speaker_Pair11 = Speaker_Pair10 | Speaker_Pair1,
        Speaker_Pair12 = Speaker_Pair8 | Speaker_Pair4,
        Speaker_Pair13 = Speaker_Pair12 | Speaker_Pair1,
        Speaker_Pair14 = Speaker_Pair12 | Speaker_Pair2,
        Speaker_Pair15 = Speaker_Pair14 | Speaker_Pair1,
    }
    [Flags]
    public enum DeviceFlag
    {
        None = 0,
        Enabled = 1,
        Default = 2,
        Init = 4,
        //Types
        Type_Digital = 134217728,
        Type_DisplayPort = 1073741824,
        Type_Network = 16777216,
        Type_Headphones = 67108864,
        Type_Speakers = 33554432,
        Type_Mask = -16777216,
        Type_Line = Type_Speakers | Type_Network,
        Type_Microphone = Type_Headphones | Type_Network,
        Type_Headset = Type_Headphones | Type_Speakers,
        Type_Handset = Type_Headset | Type_Network,
        Type_SPDIF = Type_Digital | Type_Network,
        Type_HDMI = Type_Digital | Type_Speakers,
    }

    public sealed class BassDeviceInfo
    {
        string name;
        public string Name { get; private set; }
        string driver;
        public string Driver { get; private set; }
        DeviceFlag flags;
        public DeviceFlag Flags { get; private set; }

        public DeviceFlag type { get { return flags & DeviceFlag.Type_Mask; } }
        public bool IsEnabled { get { return (flags & DeviceFlag.Enabled) != DeviceFlag.None; } }
        public bool IsDefault { get { return (flags & DeviceFlag.Default) != DeviceFlag.None; } }
        public bool IsInitialized { get { return (flags & DeviceFlag.Init) != DeviceFlag.None; } }
    }

    //[return: MarshalAs(UnmanagedType.Bool)]

    [SuppressUnmanagedCodeSecurity]
    public sealed class BassChannel
    {
        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_GetVersion"]
        static extern int Version();

        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_Init")]
        public static extern bool Init(int device, int freq, InitFlag flags, IntPtr win, Guid clsid);

        public static bool Init(int device, int freq, InitFlag flags)
        {
            return Init(device, freq, flags, IntPtr.Zero, Guid.Empty);
        }

        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_ChannelPlay")]
        public static extern bool Play(int handle, bool restart = false);
        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_ChannelPause")]
        public static extern bool Pause(int handle);
        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_ChannelStop")]
        public static extern bool Stop(int handle);

        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_ChannelSetPosition")]
        public static extern bool SetPosition(int handle, long pos, int mode = 0);
        public static bool SetPosition(int handle, double seconds)
        {
            return SetPosition(handle, Seconds2Bytes(handle, seconds));
        }

        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_ChannelGetPosition")]
        public static extern long GetPosition(int handle, int mode = 0);
        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_ChannelGetLength")]
        public static extern long GetLength(int handle, int mode = 0);

        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_ChannelSeconds2Bytes")]
        public static extern long Seconds2Bytes(int handle, double pos);
        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_ChannelBytes2Seconds")]
        public static extern double Bytes2Seconds(int handle, long pos);

        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_ChannelGetAttribute")]
        public static extern bool GetAttribute(int handle, AttributeFlag attrib, ref float value);
        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_ChannelSetAttribute")]
        public static extern bool SetAttribute(int handle, AttributeFlag attrib, float value);


        public static float GetVolume(int handle)
        {
            float vol = 0f;
            GetAttribute(handle, AttributeFlag.VOL, ref vol);
            return vol;
        }
        public static bool SetVolume(int handle, float volume)
        {
            return SetAttribute(handle, AttributeFlag.VOL, volume);
        }

        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_ChannelSetDevice")]
        public static extern int GetDevice(int handle);
        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_ChannelSetDevice")]
        public static extern bool SetDevice(int handle);

        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_ChannelGetData")]
        public static extern int GetData(int handle, IntPtr buffer, int length);
        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BBASS_ChannelGetData")]
        public static extern int GetData(int handle, float[] buffer, int length);
        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_ChannelGetData")]
        public static extern int GetData(int handle, byte[] buffer, int length);

        public delegate void SyncProc(int handle, int channel, int data, IntPtr user);
        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_ChannelSetSync")]
        public static extern bool SetSync(int handle, SyncFlag type, long param, SyncProc proc, IntPtr user);

        public static extern bool GetDeviceInfo(int device, ref BassDeviceInfo info);
        public static extern bool GetDeviceInfo(int device, BassDeviceInfo info)
        {
            bool deviceInfo = GetDeviceInfo(device, ref info);
            if (deviceInfo)
            {
                if (Bass.e)
                {
                    int len;
                    info.Name = StringUtf8(info.a.a, out len);
                    info.Driver = StringUtf8(info.a.b, out len);
                    if (len > 0 && Version() > 33818624)
                    {
                        if (Environment.OSVersion.Platform < PlatformID.WinCE)
                        {
                            try
                            {
                                info.id = Utils.IntPtrAsStringUtf8(new IntPtr((void*)((IntPtr)info.a.b.ToPointer() + len + new IntPtr(1))), out len);
                            }
                            catch { }
                        }
                    }
                }
                else
                {
                    info.name = Utils.IntPtrAsStringAnsi(info.a.a);
                    info.driver = Utils.IntPtrAsStringAnsi(info.a.b);
                    if (!string.IsNullOrEmpty(info.driver) && Bass.BASS_GetVersion() > 33818624)
                    {
                        if (Environment.OSVersion.Platform < PlatformID.WinCE)
                        {
                            try
                            {
                                info.id = Utils.IntPtrAsStringAnsi(new IntPtr((void*)((IntPtr)info.a.b.ToPointer() + info.driver.Length + new IntPtr(1))));
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                info.flags = info.a.c;
            }
            return deviceInfoInternal;
        }

        public static extern bool BASS_GetDeviceInfos()
        {
            BassDeviceInfo info;
            for (int i = 0; BASS_GetDeviceInfo(i, out info); i++)
            {

            }
        }

        [DllImport("bass.dll", CharSet = CharSet.Auto, EntryPoint = "BASS_StreamCreateFile")]
        private static extern int StreamCreateFile(bool mem, string file, long offset, long length, StreamFlag flags);
        public static int StreamCreateFile(string file, long offset = 0, long length = 0, StreamFlag flag = StreamFlag.Default | StreamFlag.AutoFree)
        {
            return StreamCreateFile(false, file, offset, length, flag | StreamFlag.Unicode);
        }

        public delegate void DOWNLOADPROC(IntPtr buffer, int length, IntPtr user);

        [DllImport("bass.dll", EntryPoint = "BASS_StreamCreateURL", CharSet = CharSet.Auto)]
        private static extern int StreamCreateURL([MarshalAs(UnmanagedType.LPWStr), In] string A_0, int A_1, StreamFlag flag, DOWNLOADPROC proc, IntPtr user);

        public static extern int ChannelFromURL(string url, int offset, StreamFlag flag = StreamFlag.Default | StreamFlag.AutoFree)
        {
            return StreamCreateURL(url, offset, flag | StreamFlag.Unicode, null, IntPtr.Zero);
        }

        static string StringUtf8(IntPtr ptr)
        {
            int length = 0;
            var arr = new byte[length];
            Marshal.Copy(ptr, arr, length);
            return Encoding.UTF8.GetString(arr, 0, length);
        }
    }
    */
}
