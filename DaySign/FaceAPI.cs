using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DaySign
{
    public struct AFD_FSDK_FACERES
    {
        public int nFace; //人脸矩形框信息
        public IntPtr rcFace; //人脸个数
        public IntPtr lfaceOrient; //人脸角度信息
    }

    public struct MRECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    public struct AFD_FSDK_Version
    {
        public int lCodebase; //代码库版本号
        public int lMajor; //主版本号
        public int lMinor; //次版本号
        public int lBuild; //编译版本号，递增
        public IntPtr Version; //字符串形式的版本号
        public IntPtr BuildDate; //编译时间
        public IntPtr CopyRight; //版权信息
    }

    public enum AFD_FSDK_OrientCode //定义脸部角度的检测范围 
    {
        AFD_FSDK_FOC_0 = 1,
        AFD_FSDK_FOC_90 = 2,
        AFD_FSDK_FOC_270 = 3,
        AFD_FSDK_FOC_180 = 4,
        AFD_FSDK_FOC_30 = 5,
        AFD_FSDK_FOC_60 = 6,
        AFD_FSDK_FOC_120 = 7,
        AFD_FSDK_FOC_150 = 8,
        AFD_FSDK_FOC_210 = 9,
        AFD_FSDK_FOC_240 = 10,
        AFD_FSDK_FOC_300 = 11,
        AFD_FSDK_FOC_330 = 12
    }

    public enum AFD_FSDK_OrientPriority //定义人脸检测结果中的人脸角度
    {
        AFD_FSDK_OPF_0_ONLY = 1,
        AFD_FSDK_OPF_90_ONLY = 2,
        AFD_FSDK_OPF_270_ONLY = 3,
        AFD_FSDK_OPF_180_ONLY = 4,
        AFD_FSDK_OPF_0_HIGHER_EXT = 5
    }

    public struct ASVLOFFSCREEN
    {

        public int u32PixelArrayFormat;
        public int i32Width;
        public int i32Height;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.SysUInt)]
        public IntPtr[] ppu8Plane;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.I4)]
        public int[] pi32Pitch;
    }

    public struct AFR_FSDK_FaceInput
    {
        public MRECT rcFace;
        public int lOrient;
    }

    public struct AFR_FSDK_FaceModel
    {
        public IntPtr pbFeature;
        public int lFeatureSize;
    }

    public class FaceAPI
    {
        //public delegate int InitialFaceEngineAPI(string appId, string sdkKey, IntPtr pMem, int lMemSize, ref IntPtr pEngine, int iOrientPriority, int nScale, int nMaxFaceNum);
        
        public static int InitialFaceEngine_FD(string appId, string sdkKey, IntPtr pMem, int lMemSize, ref IntPtr pEngine, int iOrientPriority, int nScale, int nMaxFaceNum)
        {
            return AFD_FSDK_InitialFaceEngine(appId, sdkKey, pMem, lMemSize, ref pEngine, iOrientPriority, nScale, nMaxFaceNum);
        }

        public static int InitialFaceEngine_FR(string appId, string sdkKey, IntPtr pMem, int lMemSize, ref IntPtr pEngine, int iOrientPriority, int nScale, int nMaxFaceNum)
        {
            return AFR_FSDK_InitialEngine(appId, sdkKey, pMem, lMemSize, ref pEngine);
        }

        /// <summary>
        /// 初始化脸部检测引擎
        /// </summary>
        /// <param name="appId">申请SDK时获取的App Id</param>
        /// <param name="sdkKey">申请SDK时获取的SDK Key</param>
        /// <param name="pMem">分配给引擎使用的内存地址</param>
        /// <param name="lMemSize">分配给引擎使用的内存大小</param>
        /// <param name="pEngine">引擎handle</param>
        /// <param name="iOrientPriority">期望的脸部检测角度范围</param>
        /// <param name="nScale">用于数值表示的最小人脸尺寸 有效值范围[2,50] 推荐值 16。</param>
        /// <param name="nMaxFaceNum">用户期望引擎最多能检测出的人脸数 有效值范围[1,50]</param>
        /// <returns></returns>
        [DllImport("libarcsoft_fsdk_face_detection.dll", EntryPoint = "AFD_FSDK_InitialFaceEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern int AFD_FSDK_InitialFaceEngine(string appId, string sdkKey, IntPtr pMem, int lMemSize, ref IntPtr pEngine, int iOrientPriority, int nScale, int nMaxFaceNum);
        
        /// <summary>
        ///  根据输入的图像检测出人脸位置，一般用于静态图像检测
        /// </summary>
        /// <param name="pEngine">引擎handle</param>
        /// <param name="pImgData">待检测的图像信息</param>
        /// <param name="pFaceRes">人脸检测结果</param>
        /// <returns></returns>
        [DllImport("libarcsoft_fsdk_face_detection.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AFD_FSDK_StillImageFaceDetection(IntPtr pEngine, IntPtr pImgData, ref IntPtr pFaceRes);

        /// <summary>
        /// 销毁引擎，释放相应资源
        /// </summary>
        /// <param name="pEngine">引擎handle</param>
        /// <returns></returns>
        [DllImport("libarcsoft_fsdk_face_detection.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AFD_FSDK_UninitialFaceEngine(IntPtr pEngine);
        
        /// <summary>
        /// 创建FS引擎
        /// </summary>
        /// <param name="AppId">申请SDK时获取的App Id</param>
        /// <param name="SDKKey">申请SDK时获取的SDK Key</param>
        /// <param name="pMem">分配给引擎使用的内存地址</param>
        /// <param name="lMemSize">分配给引擎使用的内存大小</param>
        /// <param name="phEngine">引擎handle</param>
        /// <returns></returns>
        [DllImport("libarcsoft_fsdk_face_recognition.dll", EntryPoint = "AFR_FSDK_InitialEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern int AFR_FSDK_InitialEngine(string AppId, string SDKKey, IntPtr pMem, int lMemSize, ref IntPtr phEngine);

        /// <summary>
        /// 提取人脸特征值
        /// </summary>
        /// <param name="hEngine">引擎handle</param>
        /// <param name="pInputImage">输入的图像数据</param>
        /// <param name="pFaceRes">已检测到的脸部信息</param>
        /// <param name="pFaceModels">提取的脸部特征信息</param>
        /// <returns></returns>
        [DllImport("libarcsoft_fsdk_face_recognition.dll", EntryPoint = "AFR_FSDK_ExtractFRFeature", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AFR_FSDK_ExtractFRFeature(IntPtr hEngine, IntPtr pInputImage, IntPtr pFaceRes, IntPtr pFaceModels);

        /// <summary>
        /// 比较人脸相似度
        /// </summary>
        /// <param name="hEngine">引擎handle</param>
        /// <param name="reffeature">已有脸部特征信息</param>
        /// <param name="probefeature">被比较的脸部特征信息</param>
        /// <param name="pfSimilScore">脸部特征相似程度数值</param>
        /// <returns></returns>
        [DllImport("libarcsoft_fsdk_face_recognition.dll", EntryPoint = "AFR_FSDK_FacePairMatching", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AFR_FSDK_FacePairMatching(IntPtr hEngine, IntPtr reffeature, IntPtr probefeature, ref float pfSimilScore);

        /// <summary>
        /// 销毁引擎
        /// </summary>
        /// <param name="hEngine">引擎handle</param>
        /// <returns></returns>
        [DllImport("libarcsoft_fsdk_face_recognition.dll", EntryPoint = "AFR_FSDK_UninitialEngine", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AFR_FSDK_UninitialEngine(IntPtr hEngine);


    }
}
